using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace CobraAPI.TeamsBot.Bots;

/// <summary>
/// Bot Framework adapter with global error handling.
/// Catches exceptions during bot processing and logs them appropriately.
/// </summary>
public class AdapterWithErrorHandler : CloudAdapter
{
    private readonly ILogger<AdapterWithErrorHandler> _logger;

    /// <summary>
    /// Initializes the adapter with error handling middleware.
    /// </summary>
    public AdapterWithErrorHandler(
        BotFrameworkAuthentication auth,
        ILogger<AdapterWithErrorHandler> logger)
        : base(auth, logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        OnTurnError = async (turnContext, exception) =>
        {
            // Log the exception
            _logger.LogError(
                exception,
                "Bot Framework Error: {ErrorType} - {ErrorMessage}. Activity: {ActivityType} from {From}",
                exception.GetType().Name,
                exception.Message,
                turnContext.Activity?.Type,
                turnContext.Activity?.From?.Name ?? "Unknown");

            // Send a message to the user (if possible)
            try
            {
                // Only send error message if this is a message activity (not for system events)
                if (turnContext.Activity?.Type == ActivityTypes.Message)
                {
                    var errorMessage = "Sorry, something went wrong processing your message. " +
                                       "The error has been logged. Please try again or contact support.";
                    await turnContext.SendActivityAsync(MessageFactory.Text(errorMessage));
                }
            }
            catch (Exception sendException)
            {
                _logger.LogError(sendException, "Failed to send error message to user");
            }

            // Send a trace activity for debugging in Bot Framework Emulator
            // Trace activities are only visible in the Emulator and not sent to production channels
            await turnContext.TraceActivityAsync(
                "OnTurnError Trace",
                exception.ToString(),
                "https://www.botframework.com/schemas/error",
                "TurnError");
        };

        _logger.LogInformation("Bot adapter initialized with error handling");
    }
}
