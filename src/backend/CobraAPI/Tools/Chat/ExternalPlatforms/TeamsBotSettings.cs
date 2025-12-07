namespace CobraAPI.Tools.Chat.ExternalPlatforms;

/// <summary>
/// Configuration settings for Teams Bot integration.
/// </summary>
public class TeamsBotSettings
{
    public const string SectionName = "TeamsBot";

    /// <summary>
    /// Base URL of the TeamsBot service.
    /// Example: http://localhost:3978 (local) or https://your-bot.azurewebsites.net (Azure)
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Optional API key for authenticating requests to TeamsBot.
    /// For POC, can be left empty if on same network.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Whether Teams Bot integration is enabled (has a configured base URL).
    /// </summary>
    public bool IsEnabled => !string.IsNullOrEmpty(BaseUrl);
}
