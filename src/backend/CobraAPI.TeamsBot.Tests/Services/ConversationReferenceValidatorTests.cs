using CobraAPI.TeamsBot.Services;
using Microsoft.Agents.Core.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace CobraAPI.TeamsBot.Tests.Services;

public class ConversationReferenceValidatorTests
{
    private readonly Mock<ILogger<ConversationReferenceValidator>> _loggerMock;
    private readonly ConversationReferenceValidator _validator;

    public ConversationReferenceValidatorTests()
    {
        _loggerMock = new Mock<ILogger<ConversationReferenceValidator>>();
        _validator = new ConversationReferenceValidator(_loggerMock.Object);
    }

    [Fact]
    public void Validate_NullReference_ReturnsMissing()
    {
        var result = _validator.Validate(null);

        Assert.Equal(ConversationReferenceStatus.Missing, result.Status);
        Assert.False(result.CanAttemptSend);
        Assert.Equal(404, result.SuggestedHttpStatusCode);
    }

    [Fact]
    public void Validate_MissingServiceUrl_ReturnsInvalid()
    {
        var reference = new ConversationReference
        {
            ServiceUrl = null,
            Conversation = new ConversationAccount { Id = "conv-123" }
        };

        var result = _validator.Validate(reference);

        Assert.Equal(ConversationReferenceStatus.Invalid, result.Status);
        Assert.False(result.CanAttemptSend);
        Assert.Contains("ServiceUrl", result.Message);
    }

    [Fact]
    public void Validate_MissingConversation_ReturnsInvalid()
    {
        var reference = new ConversationReference
        {
            ServiceUrl = "https://smba.trafficmanager.net/teams/",
            Conversation = null
        };

        var result = _validator.Validate(reference);

        Assert.Equal(ConversationReferenceStatus.Invalid, result.Status);
        Assert.False(result.CanAttemptSend);
        Assert.Contains("Conversation", result.Message);
    }

    [Fact]
    public void Validate_MissingConversationId_ReturnsInvalid()
    {
        var reference = new ConversationReference
        {
            ServiceUrl = "https://smba.trafficmanager.net/teams/",
            Conversation = new ConversationAccount { Id = "" }
        };

        var result = _validator.Validate(reference);

        Assert.Equal(ConversationReferenceStatus.Invalid, result.Status);
        Assert.False(result.CanAttemptSend);
    }

    [Fact]
    public void Validate_InvalidServiceUrlFormat_ReturnsInvalid()
    {
        var reference = new ConversationReference
        {
            ServiceUrl = "not-a-valid-url",
            Conversation = new ConversationAccount { Id = "conv-123" }
        };

        var result = _validator.Validate(reference);

        Assert.Equal(ConversationReferenceStatus.Invalid, result.Status);
        Assert.False(result.CanAttemptSend);
        Assert.Contains("valid HTTP", result.Message);
    }

    [Fact]
    public void Validate_ValidReference_ReturnsValid()
    {
        var reference = new ConversationReference
        {
            ServiceUrl = "https://smba.trafficmanager.net/teams/",
            Conversation = new ConversationAccount { Id = "19:abc123@thread.tacv2" }
        };

        var result = _validator.Validate(reference);

        Assert.Equal(ConversationReferenceStatus.Valid, result.Status);
        Assert.True(result.CanAttemptSend);
        Assert.Null(result.SuggestedHttpStatusCode);
    }

    [Fact]
    public void Validate_HttpServiceUrl_ReturnsValid()
    {
        // Local emulator uses HTTP
        var reference = new ConversationReference
        {
            ServiceUrl = "http://localhost:3978/",
            Conversation = new ConversationAccount { Id = "test-conv" }
        };

        var result = _validator.Validate(reference);

        Assert.Equal(ConversationReferenceStatus.Valid, result.Status);
        Assert.True(result.CanAttemptSend);
    }

    [Theory]
    [InlineData(403)]
    [InlineData(404)]
    public void IsExpiredReferenceException_ExpiredStatusCodes_ReturnsTrue(int statusCode)
    {
        var exception = new HttpRequestException("Error", null, (System.Net.HttpStatusCode)statusCode);

        var result = _validator.IsExpiredReferenceException(exception);

        Assert.True(result);
    }

    [Theory]
    [InlineData(400)]
    [InlineData(500)]
    [InlineData(503)]
    public void IsExpiredReferenceException_NonExpiredStatusCodes_ReturnsFalse(int statusCode)
    {
        var exception = new HttpRequestException("Error", null, (System.Net.HttpStatusCode)statusCode);

        var result = _validator.IsExpiredReferenceException(exception);

        Assert.False(result);
    }

    [Theory]
    [InlineData("Bot is not part of the conversation")]
    [InlineData("Conversation not found")]
    [InlineData("The bot is not installed")]
    [InlineData("Resource not found")]
    public void IsExpiredReferenceException_ExpiredErrorMessages_ReturnsTrue(string message)
    {
        var exception = new Exception(message);

        var result = _validator.IsExpiredReferenceException(exception);

        Assert.True(result);
    }

    [Fact]
    public void IsExpiredReferenceException_GenericException_ReturnsFalse()
    {
        var exception = new Exception("Some random error");

        var result = _validator.IsExpiredReferenceException(exception);

        Assert.False(result);
    }

    [Fact]
    public void GetExpirationResult_ReturnsExpiredStatus()
    {
        var exception = new HttpRequestException("Bot removed", null, System.Net.HttpStatusCode.Forbidden);

        var result = _validator.GetExpirationResult(exception);

        Assert.Equal(ConversationReferenceStatus.Expired, result.Status);
        Assert.False(result.CanAttemptSend);
        Assert.Equal(410, result.SuggestedHttpStatusCode);
        Assert.Contains("403", result.Message);
    }
}
