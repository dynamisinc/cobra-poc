using System.Text.Json;
using CobraAPI.TeamsBot.Models;
using CobraAPI.TeamsBot.Services;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;
using Microsoft.Agents.Extensions.Teams.Compat;
using Microsoft.Agents.Extensions.Teams.Models;
using Microsoft.Extensions.Options;

namespace CobraAPI.TeamsBot.Bots;

/// <summary>
/// COBRA Teams Bot - Handles bi-directional messaging between COBRA and Microsoft Teams.
/// Inherits from TeamsActivityHandler to get Teams-specific functionality.
/// </summary>
/// <remarks>
/// Key responsibilities:
/// - Receive channel messages from Teams (with RSC permissions, receives all messages without @mention)
/// - Forward Teams messages to COBRA chat system
/// - Send COBRA messages to Teams channels
/// - Handle bot installation/removal events
/// - Manage conversation references for proactive messaging
///
/// Migration note: Updated from Bot Framework SDK v4 to Microsoft 365 Agents SDK.
/// Namespace changes:
/// - Microsoft.Bot.Builder -> Microsoft.Agents.Builder
/// - Microsoft.Bot.Builder.Teams -> Microsoft.Agents.Extensions.Teams.Compat
/// - Microsoft.Bot.Schema -> Microsoft.Agents.Core.Models
/// - Microsoft.Bot.Schema.Teams -> Microsoft.Agents.Extensions.Teams.Models
/// </remarks>
public class CobraTeamsBot : TeamsActivityHandler
{
    private readonly ILogger<CobraTeamsBot> _logger;
    private readonly IConversationReferenceService _conversationReferenceService;
    private readonly ICobraApiClient _cobraApiClient;
    private readonly IBotMetrics _metrics;
    private readonly ConversationState _conversationState;
    private readonly UserState _userState;
    private readonly BotSettings _botSettings;

    /// <summary>
    /// Initializes a new instance of the CobraTeamsBot.
    /// </summary>
    public CobraTeamsBot(
        ILogger<CobraTeamsBot> logger,
        IConversationReferenceService conversationReferenceService,
        ICobraApiClient cobraApiClient,
        IBotMetrics metrics,
        ConversationState conversationState,
        UserState userState,
        IOptions<BotSettings> botSettings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _conversationReferenceService = conversationReferenceService ?? throw new ArgumentNullException(nameof(conversationReferenceService));
        _cobraApiClient = cobraApiClient ?? throw new ArgumentNullException(nameof(cobraApiClient));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
        _userState = userState ?? throw new ArgumentNullException(nameof(userState));
        _botSettings = botSettings?.Value ?? new BotSettings();
    }

    /// <summary>
    /// Called when any activity is received. Used to capture conversation references.
    /// </summary>
    public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
    {
        // Capture/update conversation reference for proactive messaging
        await CaptureConversationReferenceAsync(turnContext, cancellationToken);

        await base.OnTurnAsync(turnContext, cancellationToken);

        // Save any state changes
        await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
    }

    /// <summary>
    /// Called when a message activity is received from a user in Teams.
    /// With RSC permissions, this receives ALL channel messages (not just @mentions).
    /// </summary>
    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var activity = turnContext.Activity;
        var senderName = activity.From?.Name ?? "Unknown";
        var messageText = activity.Text ?? string.Empty;
        var channelId = activity.ChannelId ?? "unknown";
        var conversationId = activity.Conversation?.Id ?? "unknown";

        // Record metric for received message
        _metrics.IncrementMessagesReceived(channelId);

        _logger.LogInformation(
            "Received message from {SenderName} in channel {ChannelId}, conversation {ConversationId}: {MessagePreview}",
            senderName,
            channelId,
            conversationId,
            messageText.Length > 50 ? messageText[..50] + "..." : messageText);

        // Filter out our own messages to prevent echo loops
        if (IsOwnMessage(activity))
        {
            _logger.LogDebug("Ignoring own message");
            return;
        }

        // Check if this is a command (starts with known command or is addressed to bot)
        var command = ExtractCommand(messageText);
        if (command != null)
        {
            await HandleCommandAsync(turnContext, command, conversationId, cancellationToken);
            return;
        }

        // Build webhook payload for CobraAPI
        var payload = BuildWebhookPayload(activity, "message");

        // Try to get the channel mapping ID from stored conversation reference metadata
        // For POC: We'll use the conversationId to look up the mapping
        // In production, this would be stored in the ConversationReferenceService with mapping info
        var mappingId = await GetChannelMappingIdAsync(conversationId);

        if (mappingId.HasValue)
        {
            // Forward to CobraAPI webhook
            var success = await _cobraApiClient.SendWebhookAsync(mappingId.Value, payload);
            _metrics.IncrementWebhooksSent(success);

            if (success)
            {
                _logger.LogDebug("Message forwarded to CobraAPI successfully");
            }
            else
            {
                _logger.LogWarning("Failed to forward message to CobraAPI");
                _metrics.IncrementMessagesFailed(channelId, "webhook_failed");
                // Still echo back for debugging in POC
                var errorMessage = "⚠️ Message received but failed to forward to COBRA. Check logs.";
                await turnContext.SendActivityAsync(MessageFactory.Text(errorMessage), cancellationToken);
            }
        }
        else
        {
            // No mapping found - channel not linked to COBRA event yet
            _logger.LogDebug("No channel mapping found for conversation {ConversationId}", conversationId);

            // For POC: Echo back to show the bot is working
            var echoMessage = $"🔄 **COBRA received:** \"{messageText}\" from {senderName}\n\n" +
                              "_Note: This channel is not linked to a COBRA event yet. Messages are not being stored._\n\n" +
                              "_Type `help` to see available commands._";
            await turnContext.SendActivityAsync(MessageFactory.Text(echoMessage), cancellationToken);
        }

        // Record latency
        var latency = (DateTime.UtcNow - startTime).TotalMilliseconds;
        _metrics.RecordMessageLatency(latency);

        _logger.LogDebug("Message processed successfully in {Latency}ms", latency);
    }

    /// <summary>
    /// Extracts a command from the message text.
    /// Commands can be plain (help) or prefixed with bot mention.
    /// </summary>
    private static string? ExtractCommand(string messageText)
    {
        if (string.IsNullOrWhiteSpace(messageText))
            return null;

        // Remove bot mention if present (e.g., "<at>COBRA Bot</at> help" -> "help")
        var text = System.Text.RegularExpressions.Regex.Replace(messageText, @"<at>.*?</at>\s*", "").Trim();

        // Check for known commands (case-insensitive)
        var lowerText = text.ToLowerInvariant();

        // Direct command matches
        if (lowerText == "help" || lowerText.StartsWith("help "))
            return "help";
        if (lowerText == "status" || lowerText.StartsWith("status "))
            return "status";
        if (lowerText == "unlink" || lowerText.StartsWith("unlink "))
            return "unlink";
        if (lowerText.StartsWith("link"))
            return text; // Return full text for link command (may have event ID)
        if (lowerText.StartsWith("create channel") || lowerText.StartsWith("create-channel") || lowerText.StartsWith("newchannel"))
            return text; // Return full text for create command (has channel name)

        return null;
    }

    /// <summary>
    /// Handles bot commands.
    /// </summary>
    private async Task HandleCommandAsync(ITurnContext turnContext, string command, string conversationId, CancellationToken cancellationToken)
    {
        var lowerCommand = command.ToLowerInvariant();
        _logger.LogInformation("Processing command: {Command}", command);

        if (lowerCommand == "help")
        {
            await HandleHelpCommandAsync(turnContext, cancellationToken);
        }
        else if (lowerCommand == "status")
        {
            await HandleStatusCommandAsync(turnContext, conversationId, cancellationToken);
        }
        else if (lowerCommand == "unlink")
        {
            await HandleUnlinkCommandAsync(turnContext, conversationId, cancellationToken);
        }
        else if (lowerCommand.StartsWith("link"))
        {
            await HandleLinkCommandAsync(turnContext, command, conversationId, cancellationToken);
        }
        else if (lowerCommand.StartsWith("create channel") || lowerCommand.StartsWith("create-channel") || lowerCommand.StartsWith("newchannel"))
        {
            await HandleCreateChannelCommandAsync(turnContext, command, conversationId, cancellationToken);
        }
    }

    /// <summary>
    /// Handles the help command - shows available commands.
    /// </summary>
    private async Task HandleHelpCommandAsync(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        var helpText = $@"**{_botSettings.DisplayName} Commands**

| Command | Description |
|---------|-------------|
| `help` | Show this help message |
| `status` | Check connection status and linked event |
| `link` | Show available events to link to |
| `link [event-id]` | Link this channel to a specific COBRA event |
| `unlink` | Unlink this channel from the current event |
| `create channel [name]` | Create a new channel in the linked event |

**Examples:**
• `status` - See if this channel is connected to COBRA
• `link` - Browse available events
• `link abc12345` - Link to event with ID abc12345
• `create channel Logistics` - Create a channel named 'Logistics'

_For more help, visit COBRA documentation or contact your administrator._";

        await turnContext.SendActivityAsync(MessageFactory.Text(helpText), cancellationToken);
    }

    /// <summary>
    /// Handles the status command - shows connector status.
    /// </summary>
    private async Task HandleStatusCommandAsync(ITurnContext turnContext, string conversationId, CancellationToken cancellationToken)
    {
        var status = await _cobraApiClient.GetConnectorStatusAsync(conversationId);

        if (status == null)
        {
            var notFoundMessage = @"**Connection Status: Not Registered**

This channel is not yet registered with COBRA.

_The bot may need to be reinstalled, or COBRA API may be unavailable._";
            await turnContext.SendActivityAsync(MessageFactory.Text(notFoundMessage), cancellationToken);
            return;
        }

        var statusEmoji = status.IsActive ? "🟢" : "🔴";
        var linkStatus = status.IsLinked
            ? $"**Linked to:** {status.LinkedEventName ?? "Unknown Event"}"
            : "**Not linked** - Messages are not being stored";

        var lastActivity = status.LastActivityAt.HasValue
            ? status.LastActivityAt.Value.ToString("g") + " UTC"
            : "Never";

        var statusMessage = $@"**{statusEmoji} Connection Status**

| Property | Value |
|----------|-------|
| **Status** | {(status.IsActive ? "Active" : "Inactive")} |
| **Display Name** | {status.DisplayName ?? "Teams Channel"} |
| {linkStatus} |
| **Last Activity** | {lastActivity} |
| **Connector ID** | `{status.MappingId:N}` |

{(status.IsLinked ? "_Messages from this channel are being synced to COBRA._" : "_Use `link` command to connect to a COBRA event._")}";

        await turnContext.SendActivityAsync(MessageFactory.Text(statusMessage), cancellationToken);
    }

    /// <summary>
    /// Handles the link command - links channel to an event.
    /// </summary>
    private async Task HandleLinkCommandAsync(ITurnContext turnContext, string command, string conversationId, CancellationToken cancellationToken)
    {
        // Get tenant ID for filtering events by organization
        var tenantId = turnContext.Activity.Conversation?.TenantId;

        // Parse event ID from command if provided (e.g., "link abc123")
        var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 1)
        {
            // Just "link" - show available events
            await ShowAvailableEventsAsync(turnContext, tenantId, cancellationToken);
            return;
        }

        // Try to parse event ID
        var eventIdStr = parts[1].Trim();
        if (!Guid.TryParse(eventIdStr, out var eventId))
        {
            // Try partial GUID match - take first 8 chars as prefix
            var events = await _cobraApiClient.GetAvailableEventsAsync(tenantId);
            var matchingEvent = events?.FirstOrDefault(e =>
                e.Id.ToString("N").StartsWith(eventIdStr, StringComparison.OrdinalIgnoreCase) ||
                e.Id.ToString().StartsWith(eventIdStr, StringComparison.OrdinalIgnoreCase));

            if (matchingEvent != null)
            {
                eventId = matchingEvent.Id;
            }
            else
            {
                await turnContext.SendActivityAsync(
                    MessageFactory.Text($"⚠️ Could not find event with ID `{eventIdStr}`.\n\nUse `link` to see available events."),
                    cancellationToken);
                return;
            }
        }

        // Get the mapping ID for this conversation
        var status = await _cobraApiClient.GetConnectorStatusAsync(conversationId);
        if (status == null)
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text("⚠️ This channel is not registered with COBRA. Try removing and re-adding the bot."),
                cancellationToken);
            return;
        }

        // Link the connector to the event
        var result = await _cobraApiClient.LinkConnectorToEventAsync(status.MappingId, eventId);

        if (result?.Success == true)
        {
            var successMessage = $@"✅ **Channel Linked Successfully!**

This channel is now connected to:
**{result.EventName ?? "Event"}** (`{result.EventId:N}`)

All messages in this channel will now sync to COBRA.";
            await turnContext.SendActivityAsync(MessageFactory.Text(successMessage), cancellationToken);
        }
        else
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text($"⚠️ Failed to link channel: {result?.Message ?? "Unknown error"}"),
                cancellationToken);
        }
    }

    /// <summary>
    /// Shows available events that can be linked.
    /// </summary>
    /// <param name="turnContext">The turn context.</param>
    /// <param name="tenantId">Optional tenant ID to filter events by organization.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    private async Task ShowAvailableEventsAsync(ITurnContext turnContext, string? tenantId, CancellationToken cancellationToken)
    {
        var events = await _cobraApiClient.GetAvailableEventsAsync(tenantId);

        if (events == null || events.Count == 0)
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text("No events available for linking. Create an event in COBRA first."),
                cancellationToken);
            return;
        }

        var eventList = string.Join("\n", events.Take(10).Select(e =>
            $"• `{e.Id.ToString("N")[..8]}` - **{e.Name}** ({e.Status ?? "Active"})"));

        var message = $@"**Available Events**

{eventList}

To link this channel, use:
`link [event-id]`

Example: `link {events.First().Id.ToString("N")[..8]}`

_Showing up to 10 events. Use COBRA web interface for full list._";

        await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
    }

    /// <summary>
    /// Handles the unlink command - unlinks channel from current event.
    /// </summary>
    private async Task HandleUnlinkCommandAsync(ITurnContext turnContext, string conversationId, CancellationToken cancellationToken)
    {
        var status = await _cobraApiClient.GetConnectorStatusAsync(conversationId);

        if (status == null)
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text("⚠️ This channel is not registered with COBRA."),
                cancellationToken);
            return;
        }

        if (!status.IsLinked)
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text("This channel is not currently linked to any event."),
                cancellationToken);
            return;
        }

        var previousEvent = status.LinkedEventName;
        var success = await _cobraApiClient.UnlinkConnectorAsync(status.MappingId);

        if (success)
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text($"✅ Channel unlinked from **{previousEvent ?? "event"}**.\n\nMessages will no longer sync to COBRA."),
                cancellationToken);
        }
        else
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text("⚠️ Failed to unlink channel. Try again or contact an administrator."),
                cancellationToken);
        }
    }

    /// <summary>
    /// Handles the create channel command - creates a new COBRA channel in the linked event.
    /// </summary>
    private async Task HandleCreateChannelCommandAsync(ITurnContext turnContext, string command, string conversationId, CancellationToken cancellationToken)
    {
        // Parse channel name from command
        // Supports: "create channel My Channel", "create-channel My Channel", "newchannel My Channel"
        var lowerCommand = command.ToLowerInvariant();
        string channelName;

        if (lowerCommand.StartsWith("create channel "))
            channelName = command.Substring("create channel ".Length).Trim();
        else if (lowerCommand.StartsWith("create-channel "))
            channelName = command.Substring("create-channel ".Length).Trim();
        else if (lowerCommand.StartsWith("newchannel "))
            channelName = command.Substring("newchannel ".Length).Trim();
        else
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text(@"**Create Channel**

To create a new COBRA channel, use:
`create channel [channel name]`

**Examples:**
• `create channel Logistics`
• `create channel Fire Team Alpha`
• `create channel Weather Updates`

The channel will be created in your linked COBRA event."),
                cancellationToken);
            return;
        }

        // Validate channel name
        if (string.IsNullOrWhiteSpace(channelName))
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text("⚠️ Please provide a channel name.\n\nExample: `create channel My Channel Name`"),
                cancellationToken);
            return;
        }

        if (channelName.Length > 100)
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text("⚠️ Channel name is too long. Please use 100 characters or less."),
                cancellationToken);
            return;
        }

        // Get connector status to check if linked to an event
        var status = await _cobraApiClient.GetConnectorStatusAsync(conversationId);

        if (status == null)
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text("⚠️ This channel is not registered with COBRA. Start a conversation first."),
                cancellationToken);
            return;
        }

        if (!status.IsLinked)
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text("⚠️ This channel is not linked to a COBRA event.\n\nUse `link` to connect to an event first."),
                cancellationToken);
            return;
        }

        // Get the sender name for attribution
        var senderName = turnContext.Activity.From?.Name ?? "Teams User";

        _logger.LogInformation(
            "Creating channel '{ChannelName}' for mapping {MappingId} by {SenderName}",
            channelName, status.MappingId, senderName);

        // Create the channel
        var result = await _cobraApiClient.CreateChannelAsync(
            status.MappingId,
            channelName,
            description: null,
            createdByName: senderName);

        if (result == null)
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text("⚠️ Failed to create channel. Please try again or contact an administrator."),
                cancellationToken);
            return;
        }

        if (result.Success)
        {
            var successMessage = $@"✅ **Channel Created**

**{result.ChannelName}** has been created in event **{result.EventName}**.

View it in COBRA to start collaborating!";
            await turnContext.SendActivityAsync(MessageFactory.Text(successMessage), cancellationToken);
        }
        else
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text($"⚠️ Failed to create channel: {result.Message ?? "Unknown error"}"),
                cancellationToken);
        }
    }

    /// <summary>
    /// Builds a webhook payload from a Teams activity.
    /// </summary>
    private static TeamsWebhookPayload BuildWebhookPayload(IActivity activity, string activityType)
    {
        // Extract image attachment URL if present
        string? imageUrl = null;
        if (activity is IMessageActivity messageActivity && messageActivity.Attachments != null)
        {
            var imageAttachment = messageActivity.Attachments
                .FirstOrDefault(a => a.ContentType?.StartsWith("image/") == true);
            imageUrl = imageAttachment?.ContentUrl;
        }

        return new TeamsWebhookPayload
        {
            MessageId = activity.Id ?? Guid.NewGuid().ToString(),
            ConversationId = activity.Conversation?.Id ?? string.Empty,
            ChannelId = activity.ChannelId ?? string.Empty,
            SenderId = activity.From?.Id ?? string.Empty,
            SenderName = activity.From?.Name ?? "Unknown",
            Text = (activity as IMessageActivity)?.Text,
            Timestamp = activity.Timestamp?.UtcDateTime ?? DateTime.UtcNow,
            ActivityType = activityType,
            ImageUrl = imageUrl,
            ServiceUrl = activity.ServiceUrl
        };
    }

    /// <summary>
    /// Gets the COBRA channel mapping ID for a Teams conversation.
    /// Calls CobraAPI to lookup the mapping in the database.
    /// </summary>
    private async Task<Guid?> GetChannelMappingIdAsync(string conversationId)
    {
        // Call CobraAPI to lookup the mapping by conversation ID
        // This queries the ExternalChannelMappings table
        return await _cobraApiClient.GetMappingIdForConversationAsync(conversationId);
    }

    /// <summary>
    /// Called when a message is updated (edited) in Teams.
    /// </summary>
    protected override async Task OnMessageUpdateActivityAsync(ITurnContext<IMessageUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        var activity = turnContext.Activity;
        _logger.LogInformation(
            "Message updated by {SenderName}: {MessageId}",
            activity.From?.Name ?? "Unknown",
            activity.Id);

        // TODO: In Phase 2, update the corresponding ChatMessage in COBRA
        await Task.CompletedTask;
    }

    /// <summary>
    /// Called when a message is deleted in Teams.
    /// </summary>
    protected override async Task OnMessageDeleteActivityAsync(ITurnContext<IMessageDeleteActivity> turnContext, CancellationToken cancellationToken)
    {
        var activity = turnContext.Activity;
        _logger.LogInformation(
            "Message deleted: {MessageId}",
            activity.Id);

        // TODO: In Phase 2, mark the corresponding ChatMessage as deleted in COBRA
        await Task.CompletedTask;
    }

    /// <summary>
    /// Called when the bot is added to a team or a new member joins.
    /// Used to capture initial conversation reference and trigger onboarding.
    /// </summary>
    protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        var activity = turnContext.Activity;
        var botId = activity.Recipient?.Id;

        foreach (var member in membersAdded)
        {
            // Check if the bot itself was added (bot installation)
            if (member.Id == botId)
            {
                _logger.LogInformation(
                    "Bot installed in team/channel. Conversation: {ConversationId}, Channel: {ChannelId}",
                    activity.Conversation?.Id,
                    activity.ChannelId);

                // Send welcome message using configurable display name
                var welcomeText = $@"👋 **Welcome to COBRA Communications!**

I'm **{_botSettings.DisplayName}**, connecting this Teams channel to your COBRA incident management system.

**What I can do:**
• Forward messages from this channel to COBRA
• Post COBRA messages and announcements here
• Keep your team synchronized across platforms

**Get Started:**
Use the `help` command to see available options, or contact your COBRA administrator to link this channel to an event.

_This integration is part of the COBRA Unified Communications system._";

                await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText), cancellationToken);

                // Store conversation reference for proactive messaging
                if (activity.Conversation?.Id != null)
                {
                    await _conversationReferenceService.AddOrUpdateAsync(
                        activity.Conversation.Id,
                        activity.GetConversationReference(),
                        cancellationToken);
                }
            }
            else
            {
                _logger.LogDebug("New member added: {MemberName} ({MemberId})", member.Name, member.Id);
            }
        }
    }

    /// <summary>
    /// Called when members are removed from a team or the bot is uninstalled.
    /// Handles UC-TI-021: Bot Removal Detection.
    /// </summary>
    protected override async Task OnMembersRemovedAsync(IList<ChannelAccount> membersRemoved, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        var activity = turnContext.Activity;
        var botId = activity.Recipient?.Id;

        foreach (var member in membersRemoved)
        {
            if (member.Id == botId)
            {
                var conversationId = activity.Conversation?.Id;
                _logger.LogInformation(
                    "Bot removed from team/channel. Conversation: {ConversationId}",
                    conversationId);

                if (conversationId != null)
                {
                    // Remove from local in-memory storage
                    await _conversationReferenceService.RemoveAsync(conversationId, cancellationToken);

                    // Notify CobraAPI to deactivate the ExternalChannelMapping (UC-TI-021)
                    // This marks the mapping as inactive and clears the ConversationReference
                    var removedBy = activity.From?.Name;
                    var success = await _cobraApiClient.NotifyBotRemovedAsync(conversationId, removedBy);

                    if (success)
                    {
                        _logger.LogInformation(
                            "Successfully notified CobraAPI of bot removal from {ConversationId}",
                            conversationId);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Failed to notify CobraAPI of bot removal from {ConversationId}. " +
                            "The ExternalChannelMapping may remain active until manually cleaned up.",
                            conversationId);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Called when a Teams channel is created. Can be used to auto-register new channels.
    /// </summary>
    protected override async Task OnTeamsChannelCreatedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "New Teams channel created: {ChannelName} in team {TeamName}",
            channelInfo.Name,
            teamInfo.Name);

        await Task.CompletedTask;
    }

    /// <summary>
    /// Called when a Teams channel is deleted.
    /// </summary>
    protected override async Task OnTeamsChannelDeletedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Teams channel deleted: {ChannelName} in team {TeamName}",
            channelInfo.Name,
            teamInfo.Name);

        // TODO: Deactivate any associated channel mappings
        await Task.CompletedTask;
    }

    /// <summary>
    /// Called when a Teams channel is renamed.
    /// </summary>
    protected override async Task OnTeamsChannelRenamedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Teams channel renamed to: {ChannelName} in team {TeamName}",
            channelInfo.Name,
            teamInfo.Name);

        // TODO: Update ExternalChannelMapping name
        await Task.CompletedTask;
    }

    /// <summary>
    /// Captures the conversation reference for proactive messaging.
    /// Called on every turn to ensure we have the latest service URL.
    /// Stateless architecture: Stores in CobraAPI database (primary) and in-memory (fallback).
    /// </summary>
    private async Task CaptureConversationReferenceAsync(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        var activity = turnContext.Activity;
        if (activity.Conversation == null)
            return;

        var conversationId = activity.Conversation.Id;
        var reference = activity.GetConversationReference();

        // Extract metadata for stateless storage
        var tenantId = activity.Conversation.TenantId;
        var channelName = activity.ChannelId == "emulator" ? "Emulator" : (activity.Conversation.Name ?? "Teams Channel");
        var installedByName = activity.From?.Name;
        var isEmulator = activity.ChannelId == "emulator";

        // Serialize ConversationReference for storage
        var referenceJson = JsonSerializer.Serialize(reference);

        // Primary: Store in CobraAPI database
        var storeRequest = new StoreConversationReferenceRequest
        {
            ConversationId = conversationId,
            ConversationReferenceJson = referenceJson,
            TenantId = tenantId,
            ChannelName = channelName,
            InstalledByName = installedByName,
            IsEmulator = isEmulator
        };

        var result = await _cobraApiClient.StoreConversationReferenceAsync(storeRequest);
        if (result != null)
        {
            _logger.LogDebug(
                "Stored ConversationReference in CobraAPI. MappingId: {MappingId}, IsNew: {IsNew}",
                result.MappingId, result.IsNewMapping);
        }
        else
        {
            _logger.LogDebug(
                "CobraAPI storage unavailable, using in-memory fallback for {ConversationId}",
                conversationId);
        }

        // Fallback: Also store in-memory for backwards compatibility
        await _conversationReferenceService.AddOrUpdateAsync(
            conversationId,
            reference,
            cancellationToken);
    }

    /// <summary>
    /// Determines if a message was sent by this bot (to prevent echo loops).
    /// </summary>
    private bool IsOwnMessage(IActivity activity)
    {
        // The bot's messages have the same ID in From and Recipient
        return activity.From?.Id == activity.Recipient?.Id;
    }
}
