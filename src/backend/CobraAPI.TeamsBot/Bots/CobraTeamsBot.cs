using CobraAPI.TeamsBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;

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
/// </remarks>
public class CobraTeamsBot : TeamsActivityHandler
{
    private readonly ILogger<CobraTeamsBot> _logger;
    private readonly IConversationReferenceService _conversationReferenceService;
    private readonly ConversationState _conversationState;
    private readonly UserState _userState;

    /// <summary>
    /// Initializes a new instance of the CobraTeamsBot.
    /// </summary>
    public CobraTeamsBot(
        ILogger<CobraTeamsBot> logger,
        IConversationReferenceService conversationReferenceService,
        ConversationState conversationState,
        UserState userState)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _conversationReferenceService = conversationReferenceService ?? throw new ArgumentNullException(nameof(conversationReferenceService));
        _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
        _userState = userState ?? throw new ArgumentNullException(nameof(userState));
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
        var activity = turnContext.Activity;
        var senderName = activity.From?.Name ?? "Unknown";
        var messageText = activity.Text ?? string.Empty;
        var channelId = activity.ChannelId;
        var conversationId = activity.Conversation?.Id ?? "unknown";

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

        // For POC: Echo the message back to confirm receipt
        // TODO: In Phase 2 (UC-TI-009), this will forward to COBRA via IChatService
        var echoMessage = $"🔄 **COBRA received:** \"{messageText}\" from {senderName}";
        await turnContext.SendActivityAsync(MessageFactory.Text(echoMessage), cancellationToken);

        _logger.LogDebug("Message processed successfully");
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

                // Send welcome message
                var welcomeText = @"👋 **Welcome to COBRA Communications!**

I'm the COBRA bot, connecting this Teams channel to your COBRA incident management system.

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
    /// </summary>
    protected override async Task OnMembersRemovedAsync(IList<ChannelAccount> membersRemoved, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        var activity = turnContext.Activity;
        var botId = activity.Recipient?.Id;

        foreach (var member in membersRemoved)
        {
            if (member.Id == botId)
            {
                _logger.LogInformation(
                    "Bot removed from team/channel. Conversation: {ConversationId}",
                    activity.Conversation?.Id);

                // Remove the conversation reference
                if (activity.Conversation?.Id != null)
                {
                    await _conversationReferenceService.RemoveAsync(activity.Conversation.Id, cancellationToken);
                }

                // TODO: In Phase 4 (UC-TI-021), deactivate ExternalChannelMapping
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
    /// </summary>
    private async Task CaptureConversationReferenceAsync(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        var activity = turnContext.Activity;
        if (activity.Conversation != null)
        {
            var reference = activity.GetConversationReference();
            await _conversationReferenceService.AddOrUpdateAsync(
                activity.Conversation.Id,
                reference,
                cancellationToken);
        }
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
