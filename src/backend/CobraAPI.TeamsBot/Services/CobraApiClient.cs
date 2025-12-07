using System.Net.Http.Json;
using CobraAPI.TeamsBot.Models;
using Microsoft.Extensions.Options;

namespace CobraAPI.TeamsBot.Services;

/// <summary>
/// HTTP client for communicating with CobraAPI.
/// Forwards Teams messages to COBRA webhook endpoints.
/// Includes retry logic for transient failures.
/// </summary>
public class CobraApiClient : ICobraApiClient
{
    private readonly HttpClient _httpClient;
    private readonly CobraApiSettings _settings;
    private readonly ILogger<CobraApiClient> _logger;
    private readonly RetryOptions _retryOptions;

    public CobraApiClient(
        HttpClient httpClient,
        IOptions<CobraApiSettings> settings,
        ILogger<CobraApiClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        // Configure retry options for CobraAPI calls
        _retryOptions = new RetryOptions
        {
            MaxRetries = 3,
            InitialDelay = TimeSpan.FromMilliseconds(500),
            MaxDelay = TimeSpan.FromSeconds(10),
            BackoffMultiplier = 2.0,
            AddJitter = true
        };

        // Set base address from configuration
        if (!string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/");
        }

        // Add API key header if configured
        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _settings.ApiKey);
        }
    }

    /// <summary>
    /// Checks if CobraAPI is reachable and healthy.
    /// </summary>
    public async Task<bool> CheckHealthAsync()
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            return false;
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await _httpClient.GetAsync("api/health", cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "CobraAPI health check failed");
            return false;
        }
    }

    /// <summary>
    /// Sends a Teams message to the CobraAPI webhook endpoint with retry logic.
    /// </summary>
    public async Task<bool> SendWebhookAsync(Guid mappingId, TeamsWebhookPayload payload)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogWarning("CobraAPI BaseUrl is not configured. Message not forwarded.");
            return false;
        }

        _logger.LogInformation(
            "Forwarding Teams message {MessageId} to CobraAPI webhook for mapping {MappingId}",
            payload.MessageId, mappingId);

        var result = await RetryPolicy.ExecuteAsync(
            async ct =>
            {
                var response = await _httpClient.PostAsJsonAsync(
                    $"api/webhooks/teams/{mappingId}",
                    payload,
                    ct);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                // Check if this is a transient error that should be retried
                if (RetryPolicy.IsTransientHttpStatusCode(response.StatusCode))
                {
                    var errorContent = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogWarning(
                        "CobraAPI webhook returned transient error {StatusCode}: {Error}",
                        response.StatusCode, errorContent);
                    return false; // Will trigger retry
                }

                // Non-transient error - log and don't retry
                var content = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning(
                    "CobraAPI webhook returned {StatusCode}: {Error}",
                    response.StatusCode, content);
                return false;
            },
            _retryOptions,
            _logger,
            $"SendWebhook({mappingId})");

        if (!result.Success && result.LastException != null)
        {
            _logger.LogError(result.LastException,
                "Failed to forward message to CobraAPI after {Attempts} attempts",
                result.Attempts);
        }

        return result.Success && result.Value;
    }

    /// <summary>
    /// Sends a Teams message to CobraAPI when no mapping exists yet with retry logic.
    /// Used during initial channel linking flow.
    /// </summary>
    public async Task<bool> SendUnmappedMessageAsync(string conversationId, TeamsWebhookPayload payload)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogWarning("CobraAPI BaseUrl is not configured. Message not forwarded.");
            return false;
        }

        _logger.LogInformation(
            "Forwarding unmapped Teams message {MessageId} from conversation {ConversationId}",
            payload.MessageId, conversationId);

        var result = await RetryPolicy.ExecuteAsync(
            async ct =>
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "api/webhooks/teams/unmapped",
                    payload,
                    ct);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                // 404 is expected if no mapping exists - don't retry
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogDebug("No mapping found for conversation {ConversationId} - message not stored",
                        conversationId);
                    return false;
                }

                // Check if transient
                if (RetryPolicy.IsTransientHttpStatusCode(response.StatusCode))
                {
                    return false; // Will trigger retry
                }

                var errorContent = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning(
                    "CobraAPI returned {StatusCode} for unmapped message: {Error}",
                    response.StatusCode, errorContent);
                return false;
            },
            _retryOptions,
            _logger,
            $"SendUnmappedMessage({conversationId})");

        return result.Success && result.Value;
    }

    /// <summary>
    /// Gets the channel mapping ID for a Teams conversation from CobraAPI with retry logic.
    /// </summary>
    public async Task<Guid?> GetMappingIdForConversationAsync(string conversationId)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogWarning("CobraAPI BaseUrl is not configured. Cannot lookup mapping.");
            return null;
        }

        // URL encode the conversation ID (Teams IDs can contain special characters)
        var encodedId = Uri.EscapeDataString(conversationId);

        var result = await RetryPolicy.ExecuteAsync<Guid?>(
            async ct =>
            {
                var response = await _httpClient.GetAsync(
                    $"api/chat/diagnostics/teams-mapping/{encodedId}",
                    ct);

                if (response.IsSuccessStatusCode)
                {
                    var mappingResult = await response.Content.ReadFromJsonAsync<MappingLookupResponse>(ct);
                    if (mappingResult != null)
                    {
                        _logger.LogInformation(
                            "Found mapping {MappingId} for conversation {ConversationId}",
                            mappingResult.MappingId, conversationId);
                        return mappingResult.MappingId;
                    }
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogDebug("No mapping found for conversation {ConversationId}", conversationId);
                    return null;
                }

                // Check if transient - should retry
                if (RetryPolicy.IsTransientHttpStatusCode(response.StatusCode))
                {
                    throw new HttpRequestException($"Transient error: {response.StatusCode}",
                        null, response.StatusCode);
                }

                _logger.LogWarning(
                    "Unexpected response {StatusCode} when looking up mapping for {ConversationId}",
                    response.StatusCode, conversationId);
                return null;
            },
            result => false, // null result doesn't trigger retry - only exceptions do
            _retryOptions,
            _logger,
            $"GetMappingId({conversationId})");

        return result.Value;
    }

    // === Stateless Architecture Methods (UC-TI-029) ===

    /// <summary>
    /// Stores or updates a ConversationReference in CobraAPI with retry logic.
    /// Called on every incoming message to keep the reference current.
    /// </summary>
    public async Task<StoreConversationReferenceResult?> StoreConversationReferenceAsync(
        StoreConversationReferenceRequest request)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogWarning("CobraAPI BaseUrl is not configured. Cannot store ConversationReference.");
            return null;
        }

        _logger.LogDebug(
            "Storing ConversationReference for {ConversationId}, TenantId: {TenantId}, IsEmulator: {IsEmulator}",
            request.ConversationId, request.TenantId, request.IsEmulator);

        var result = await RetryPolicy.ExecuteAsync<StoreConversationReferenceResult?>(
            async ct =>
            {
                var response = await _httpClient.PutAsJsonAsync(
                    "api/chat/teams/conversation-reference",
                    request,
                    ct);

                if (response.IsSuccessStatusCode)
                {
                    var storeResult = await response.Content.ReadFromJsonAsync<StoreConversationReferenceResult>(ct);
                    if (storeResult != null)
                    {
                        _logger.LogInformation(
                            "Stored ConversationReference for {ConversationId}, MappingId: {MappingId}, IsNew: {IsNew}",
                            request.ConversationId, storeResult.MappingId, storeResult.IsNewMapping);
                        return storeResult;
                    }
                }

                // Check if transient
                if (RetryPolicy.IsTransientHttpStatusCode(response.StatusCode))
                {
                    throw new HttpRequestException($"Transient error: {response.StatusCode}",
                        null, response.StatusCode);
                }

                var errorContent = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning(
                    "Failed to store ConversationReference. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);
                return null;
            },
            result => false, // null result doesn't trigger retry
            _retryOptions,
            _logger,
            $"StoreConversationReference({request.ConversationId})");

        return result.Value;
    }

    /// <summary>
    /// Gets a ConversationReference from CobraAPI by conversation ID with retry logic.
    /// </summary>
    public async Task<GetConversationReferenceResult?> GetConversationReferenceAsync(string conversationId)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogWarning("CobraAPI BaseUrl is not configured. Cannot get ConversationReference.");
            return null;
        }

        var encodedId = Uri.EscapeDataString(conversationId);

        var result = await RetryPolicy.ExecuteAsync<GetConversationReferenceResult?>(
            async ct =>
            {
                var response = await _httpClient.GetAsync(
                    $"api/chat/teams/conversation-reference/{encodedId}",
                    ct);

                if (response.IsSuccessStatusCode)
                {
                    var getResult = await response.Content.ReadFromJsonAsync<GetConversationReferenceResult>(ct);
                    if (getResult != null)
                    {
                        _logger.LogDebug(
                            "Retrieved ConversationReference for {ConversationId}, MappingId: {MappingId}",
                            conversationId, getResult.MappingId);
                        return getResult;
                    }
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogDebug("No ConversationReference found for {ConversationId}", conversationId);
                    return null;
                }

                // Check if transient
                if (RetryPolicy.IsTransientHttpStatusCode(response.StatusCode))
                {
                    throw new HttpRequestException($"Transient error: {response.StatusCode}",
                        null, response.StatusCode);
                }

                _logger.LogWarning(
                    "Unexpected response {StatusCode} when getting ConversationReference for {ConversationId}",
                    response.StatusCode, conversationId);
                return null;
            },
            result => false, // null doesn't trigger retry
            _retryOptions,
            _logger,
            $"GetConversationReference({conversationId})");

        return result.Value;
    }

    /// <summary>
    /// Notifies CobraAPI that the bot has been removed from a Teams conversation.
    /// This deactivates the corresponding ExternalChannelMapping.
    /// </summary>
    public async Task<bool> NotifyBotRemovedAsync(string conversationId, string? removedBy = null)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogWarning("CobraAPI BaseUrl is not configured. Cannot notify bot removal.");
            return false;
        }

        _logger.LogInformation(
            "Notifying CobraAPI that bot was removed from conversation {ConversationId} by {RemovedBy}",
            conversationId, removedBy ?? "unknown");

        var encodedId = Uri.EscapeDataString(conversationId);
        var requestBody = new { removedBy };

        var result = await RetryPolicy.ExecuteAsync(
            async ct =>
            {
                var response = await _httpClient.PostAsJsonAsync(
                    $"api/chat/teams/bot-removed/{encodedId}",
                    requestBody,
                    ct);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Successfully notified CobraAPI of bot removal from {ConversationId}",
                        conversationId);
                    return true;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // No mapping existed - that's okay
                    _logger.LogDebug(
                        "No mapping found for conversation {ConversationId} during bot removal notification",
                        conversationId);
                    return true; // Not a failure
                }

                // Check if transient
                if (RetryPolicy.IsTransientHttpStatusCode(response.StatusCode))
                {
                    throw new HttpRequestException($"Transient error: {response.StatusCode}",
                        null, response.StatusCode);
                }

                var errorContent = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning(
                    "Failed to notify bot removal. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);
                return false;
            },
            _retryOptions,
            _logger,
            $"NotifyBotRemoved({conversationId})");

        return result.Success && result.Value;
    }

    // === Bot Command Support Methods ===

    /// <summary>
    /// Gets a list of active events that can be linked to this Teams channel.
    /// Optionally filters by tenant ID to show only events relevant to the organization.
    /// </summary>
    public async Task<List<EventSummary>?> GetAvailableEventsAsync(string? tenantId = null)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogWarning("CobraAPI BaseUrl is not configured. Cannot get events.");
            return null;
        }

        try
        {
            // Build URL with optional tenant filter
            var url = "api/events";
            if (!string.IsNullOrEmpty(tenantId))
            {
                url += $"?tenantId={Uri.EscapeDataString(tenantId)}";
                _logger.LogDebug("Filtering events by tenant ID: {TenantId}", tenantId);
            }

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var events = await response.Content.ReadFromJsonAsync<List<EventSummary>>();
                _logger.LogDebug("Retrieved {Count} events for tenant {TenantId}",
                    events?.Count ?? 0, tenantId ?? "all");
                return events;
            }

            _logger.LogWarning("Failed to get events. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available events");
            return null;
        }
    }

    /// <summary>
    /// Links a Teams connector to a COBRA event.
    /// </summary>
    public async Task<LinkResult?> LinkConnectorToEventAsync(Guid mappingId, Guid eventId)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogWarning("CobraAPI BaseUrl is not configured. Cannot link connector.");
            return null;
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/chat/teams/mappings/{mappingId}/link",
                new { eventId });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LinkApiResponse>();
                return new LinkResult
                {
                    Success = true,
                    MappingId = mappingId,
                    EventId = result?.EventId ?? eventId,
                    EventName = result?.EventName,
                    Message = result?.Message ?? "Linked successfully"
                };
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to link connector. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, error);

            return new LinkResult
            {
                Success = false,
                MappingId = mappingId,
                EventId = eventId,
                Message = $"Failed to link: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking connector {MappingId} to event {EventId}", mappingId, eventId);
            return null;
        }
    }

    /// <summary>
    /// Unlinks a Teams connector from its current event.
    /// </summary>
    public async Task<bool> UnlinkConnectorAsync(Guid mappingId)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogWarning("CobraAPI BaseUrl is not configured. Cannot unlink connector.");
            return false;
        }

        try
        {
            var response = await _httpClient.PostAsync(
                $"api/chat/teams/mappings/{mappingId}/unlink",
                null);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully unlinked connector {MappingId}", mappingId);
                return true;
            }

            _logger.LogWarning("Failed to unlink connector. Status: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking connector {MappingId}", mappingId);
            return false;
        }
    }

    /// <summary>
    /// Gets the current connector status including linked event info.
    /// </summary>
    public async Task<ConnectorStatus?> GetConnectorStatusAsync(string conversationId)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogWarning("CobraAPI BaseUrl is not configured. Cannot get connector status.");
            return null;
        }

        var encodedId = Uri.EscapeDataString(conversationId);

        try
        {
            // First get the conversation reference which has the mapping ID
            var refResponse = await _httpClient.GetAsync($"api/chat/teams/conversation-reference/{encodedId}");

            if (!refResponse.IsSuccessStatusCode)
            {
                return null;
            }

            var refResult = await refResponse.Content.ReadFromJsonAsync<GetConversationReferenceResult>();
            if (refResult == null)
            {
                return null;
            }

            // Get the full mapping details
            var mappingsResponse = await _httpClient.GetAsync("api/chat/teams/mappings");
            if (mappingsResponse.IsSuccessStatusCode)
            {
                var mappingsResult = await mappingsResponse.Content.ReadFromJsonAsync<ListMappingsResponse>();
                var mapping = mappingsResult?.Connectors?.FirstOrDefault(m => m.MappingId == refResult.MappingId);

                if (mapping != null)
                {
                    return new ConnectorStatus
                    {
                        MappingId = mapping.MappingId,
                        DisplayName = mapping.DisplayName,
                        IsActive = mapping.IsActive,
                        IsLinked = mapping.IsLinked,
                        LinkedEventId = mapping.LinkedEventId,
                        LinkedEventName = mapping.LinkedEventName,
                        LastActivityAt = mapping.LastActivityAt,
                        CreatedAt = mapping.CreatedAt
                    };
                }
            }

            // Fallback - return basic info from the reference
            return new ConnectorStatus
            {
                MappingId = refResult.MappingId,
                IsActive = refResult.IsActive,
                IsLinked = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connector status for {ConversationId}", conversationId);
            return null;
        }
    }

    /// <summary>
    /// Creates a new COBRA channel from the bot.
    /// </summary>
    public async Task<CreateChannelResult?> CreateChannelAsync(
        Guid mappingId,
        string channelName,
        string? description = null,
        string? createdByName = null)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogWarning("CobraAPI BaseUrl is not configured. Cannot create channel.");
            return null;
        }

        try
        {
            var request = new CreateChannelFromBotRequest
            {
                ChannelName = channelName,
                Description = description,
                CreatedByName = createdByName
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"api/chat/teams/mappings/{mappingId}/create-channel",
                request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CreateChannelResult>();
                if (result != null)
                {
                    _logger.LogInformation(
                        "Created channel '{ChannelName}' ({ChannelId}) for event {EventName}",
                        result.ChannelName, result.ChannelId, result.EventName);
                    return result;
                }
            }

            // Try to parse error response for better error messages
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning(
                "Failed to create channel. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);

            // Try to extract error message from JSON response
            string errorMessage = $"Failed to create channel: {response.StatusCode}";
            try
            {
                var errorResponse = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(
                    errorContent,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (!string.IsNullOrEmpty(errorResponse?.Error))
                {
                    errorMessage = errorResponse.Error;
                }
            }
            catch
            {
                // Ignore JSON parsing errors, use default message
            }

            return new CreateChannelResult
            {
                Success = false,
                Message = errorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating channel for mapping {MappingId}", mappingId);
            return null;
        }
    }
}

/// <summary>
/// Request to create a channel from bot.
/// </summary>
internal class CreateChannelFromBotRequest
{
    public required string ChannelName { get; set; }
    public string? Description { get; set; }
    public string? IconName { get; set; }
    public string? CreatedByName { get; set; }
}

/// <summary>
/// Response from link API.
/// </summary>
internal class LinkApiResponse
{
    public string? Message { get; set; }
    public Guid MappingId { get; set; }
    public Guid EventId { get; set; }
    public string? EventName { get; set; }
}

/// <summary>
/// Response from list mappings API.
/// </summary>
internal class ListMappingsResponse
{
    public int Count { get; set; }
    public List<ConnectorDto>? Connectors { get; set; }
}

/// <summary>
/// Connector DTO from API.
/// </summary>
internal class ConnectorDto
{
    public Guid MappingId { get; set; }
    public string? DisplayName { get; set; }
    public bool IsActive { get; set; }
    public bool IsLinked { get; set; }
    public Guid? LinkedEventId { get; set; }
    public string? LinkedEventName { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Response model for mapping lookup.
/// </summary>
public class MappingLookupResponse
{
    public Guid MappingId { get; set; }
    public Guid? EventId { get; set; }
    public string TeamsConversationId { get; set; } = string.Empty;
    public string WebhookUrl { get; set; } = string.Empty;
}

/// <summary>
/// Generic error response from API.
/// </summary>
internal class ErrorResponse
{
    public string? Error { get; set; }
}

/// <summary>
/// Configuration settings for CobraAPI client.
/// </summary>
public class CobraApiSettings
{
    /// <summary>
    /// Base URL of the CobraAPI service (e.g., "http://localhost:5000").
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Optional API key for authentication.
    /// For POC, this can be left empty if on the same network.
    /// </summary>
    public string? ApiKey { get; set; }
}
