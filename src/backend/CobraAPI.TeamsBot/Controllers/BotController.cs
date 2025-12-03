using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace CobraAPI.TeamsBot.Controllers;

/// <summary>
/// Controller for handling Bot Framework messages.
/// This is the entry point for all incoming messages from Microsoft Teams and Bot Framework Emulator.
/// </summary>
[Route("api/messages")]
[ApiController]
public class BotController : ControllerBase
{
    private readonly IBotFrameworkHttpAdapter _adapter;
    private readonly IBot _bot;
    private readonly ILogger<BotController> _logger;

    /// <summary>
    /// Initializes a new instance of the BotController.
    /// </summary>
    /// <param name="adapter">The Bot Framework HTTP adapter.</param>
    /// <param name="bot">The bot implementation to handle activities.</param>
    /// <param name="logger">Logger for diagnostic information.</param>
    public BotController(
        IBotFrameworkHttpAdapter adapter,
        IBot bot,
        ILogger<BotController> logger)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles incoming POST requests from Bot Framework and Microsoft Teams.
    /// All messages, reactions, and other activities come through this endpoint.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task PostAsync()
    {
        _logger.LogDebug("Received incoming Bot Framework activity");

        // Delegate the processing of the HTTP POST to the adapter.
        // The adapter will invoke the bot.
        await _adapter.ProcessAsync(Request, Response, _bot);
    }
}
