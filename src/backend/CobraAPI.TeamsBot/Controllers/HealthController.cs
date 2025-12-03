using Microsoft.AspNetCore.Mvc;

namespace CobraAPI.TeamsBot.Controllers;

/// <summary>
/// Health check endpoint for monitoring bot availability.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class HealthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IConfiguration configuration, ILogger<HealthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Returns the health status of the bot.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var appId = _configuration["MicrosoftAppId"];
        var hasAppId = !string.IsNullOrEmpty(appId);

        return Ok(new
        {
            status = "healthy",
            service = "COBRA Teams Bot",
            timestamp = DateTime.UtcNow,
            configuration = new
            {
                hasAppId,
                appIdPrefix = hasAppId && appId != null ? appId[..Math.Min(8, appId.Length)] + "..." : null
            }
        });
    }
}
