using CobraAPI.TeamsBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;

namespace CobraAPI.TeamsBot.Controllers;

/// <summary>
/// Diagnostic endpoints for testing the bot locally.
/// These endpoints allow testing without the Bot Framework Emulator.
/// WARNING: Disable or secure these endpoints in production.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class DiagnosticsController : ControllerBase
{
    private readonly IConversationReferenceService _conversationReferenceService;
    private readonly IBotFrameworkHttpAdapter _adapter;
    private readonly IBot _bot;
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(
        IConversationReferenceService conversationReferenceService,
        IBotFrameworkHttpAdapter adapter,
        IBot bot,
        ILogger<DiagnosticsController> logger)
    {
        _conversationReferenceService = conversationReferenceService;
        _adapter = adapter;
        _bot = bot;
        _logger = logger;
    }

    /// <summary>
    /// List all stored conversation references.
    /// Useful for verifying proactive messaging setup.
    /// </summary>
    [HttpGet("conversations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConversations()
    {
        var references = await _conversationReferenceService.GetAllAsync();

        var result = references.Select(kvp => new
        {
            conversationId = kvp.Key,
            serviceUrl = kvp.Value.ServiceUrl,
            channelId = kvp.Value.ChannelId,
            botId = kvp.Value.Bot?.Id,
            botName = kvp.Value.Bot?.Name
        });

        return Ok(new
        {
            count = references.Count,
            conversations = result
        });
    }

    /// <summary>
    /// Simulate sending a proactive message to a stored conversation.
    /// Tests the outbound messaging capability.
    /// </summary>
    [HttpPost("send-proactive/{conversationId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendProactiveMessage(string conversationId, [FromBody] ProactiveMessageRequest request)
    {
        var reference = await _conversationReferenceService.GetAsync(conversationId);
        if (reference == null)
        {
            return NotFound(new { error = $"Conversation '{conversationId}' not found" });
        }

        _logger.LogInformation("Sending proactive message to {ConversationId}: {Message}", conversationId, request.Message);

        try
        {
            // Use the adapter to continue the conversation
            await ((CloudAdapter)_adapter).ContinueConversationAsync(
                botAppId: string.Empty, // Empty for local testing
                reference,
                async (turnContext, cancellationToken) =>
                {
                    var formattedMessage = $"📢 **[COBRA]** {request.Message}";
                    await turnContext.SendActivityAsync(MessageFactory.Text(formattedMessage), cancellationToken);
                },
                default);

            return Ok(new
            {
                success = true,
                conversationId,
                message = request.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send proactive message to {ConversationId}", conversationId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Simulate a bot installation event (adds a test conversation reference).
    /// Useful for testing without the emulator.
    /// </summary>
    [HttpPost("simulate-install")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SimulateInstall()
    {
        var testConversationId = $"test-conversation-{Guid.NewGuid():N}";

        var testReference = new ConversationReference
        {
            ChannelId = "emulator",
            ServiceUrl = "http://localhost:3978",
            Conversation = new ConversationAccount
            {
                Id = testConversationId,
                Name = "Test Conversation",
                IsGroup = true
            },
            Bot = new ChannelAccount
            {
                Id = "cobra-bot",
                Name = "COBRA Bot"
            },
            User = new ChannelAccount
            {
                Id = "test-user",
                Name = "Test User"
            }
        };

        await _conversationReferenceService.AddOrUpdateAsync(testConversationId, testReference);

        _logger.LogInformation("Simulated bot installation for conversation {ConversationId}", testConversationId);

        return Ok(new
        {
            success = true,
            conversationId = testConversationId,
            message = "Test conversation reference created. Use this ID to test proactive messaging."
        });
    }
}

/// <summary>
/// Request model for sending proactive messages.
/// </summary>
public class ProactiveMessageRequest
{
    /// <summary>
    /// The message to send.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
