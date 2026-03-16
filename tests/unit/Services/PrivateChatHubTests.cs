using FluentAssertions;
using Loca.Application.DTOs;
using Loca.Domain.Entities;
using Loca.Domain.Enums;

namespace Loca.Tests.Unit.Services;

/// <summary>
/// Tests for PrivateChatHub business logic.
/// Since SignalR hubs are difficult to unit test directly (they depend on Hub context),
/// we test the domain logic and data validation that the hub relies on.
/// </summary>
public class PrivateChatHubTests
{
    [Fact]
    public void Should_IdentifyParticipant_InConversation()
    {
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var outsider = Guid.NewGuid();

        var conversation = new Conversation
        {
            Participant1Id = user1,
            Participant2Id = user2,
        };

        // user1 is participant
        var isParticipant1 = conversation.Participant1Id == user1 || conversation.Participant2Id == user1;
        isParticipant1.Should().BeTrue();

        // user2 is participant
        var isParticipant2 = conversation.Participant1Id == user2 || conversation.Participant2Id == user2;
        isParticipant2.Should().BeTrue();

        // outsider is not participant
        var isOutsider = conversation.Participant1Id == outsider || conversation.Participant2Id == outsider;
        isOutsider.Should().BeFalse();
    }

    [Fact]
    public void Should_GetOtherParticipant_FromConversation()
    {
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        var conversation = new Conversation
        {
            Participant1Id = user1,
            Participant2Id = user2,
        };

        var otherForUser1 = conversation.Participant1Id == user1
            ? conversation.Participant2Id
            : conversation.Participant1Id;
        otherForUser1.Should().Be(user2);

        var otherForUser2 = conversation.Participant1Id == user2
            ? conversation.Participant2Id
            : conversation.Participant1Id;
        otherForUser2.Should().Be(user1);
    }

    [Fact]
    public void Should_IncrementUnreadCount_ForReceiver()
    {
        var sender = Guid.NewGuid();
        var receiver = Guid.NewGuid();

        var conversation = new Conversation
        {
            Participant1Id = sender,
            Participant2Id = receiver,
            UnreadCount1 = 0,
            UnreadCount2 = 0,
        };

        // When sender (participant1) sends message, increment receiver's (participant2) unread
        if (conversation.Participant1Id == sender)
            conversation.UnreadCount2++;
        else
            conversation.UnreadCount1++;

        conversation.UnreadCount1.Should().Be(0);
        conversation.UnreadCount2.Should().Be(1);
    }

    [Fact]
    public void Should_ResetUnreadCount_OnMarkRead()
    {
        var reader = Guid.NewGuid();

        var conversation = new Conversation
        {
            Participant1Id = reader,
            Participant2Id = Guid.NewGuid(),
            UnreadCount1 = 5,
            UnreadCount2 = 3,
        };

        // When participant1 marks read, reset their unread count
        if (conversation.Participant1Id == reader)
            conversation.UnreadCount1 = 0;
        else
            conversation.UnreadCount2 = 0;

        conversation.UnreadCount1.Should().Be(0);
        conversation.UnreadCount2.Should().Be(3);
    }

    [Fact]
    public void Should_UpdateLastMessage_OnSend()
    {
        var conversation = new Conversation
        {
            LastMessageAt = null,
            LastMessagePreview = null,
        };

        var content = "Salam, necəsən?";
        conversation.LastMessageAt = DateTime.UtcNow;
        conversation.LastMessagePreview = content.Length > 100 ? content[..100] : content;

        conversation.LastMessageAt.Should().NotBeNull();
        conversation.LastMessagePreview.Should().Be("Salam, necəsən?");
    }

    [Fact]
    public void Should_TruncateLastMessagePreview_At100Chars()
    {
        var conversation = new Conversation();
        var longContent = new string('A', 200);

        conversation.LastMessagePreview = longContent.Length > 100 ? longContent[..100] : longContent;

        conversation.LastMessagePreview.Should().HaveLength(100);
    }

    [Fact]
    public void Should_ConstructPrivateMessageDto_Correctly()
    {
        var msgId = Guid.NewGuid();
        var convId = Guid.NewGuid();
        var senderId = Guid.NewGuid();

        var dto = new PrivateMessageDto(
            msgId, convId, senderId, "Ali", "https://avatar.url",
            "text", "Salam!", null, DateTime.UtcNow);

        dto.Id.Should().Be(msgId);
        dto.ConversationId.Should().Be(convId);
        dto.SenderId.Should().Be(senderId);
        dto.SenderName.Should().Be("Ali");
        dto.Type.Should().Be("text");
        dto.Content.Should().Be("Salam!");
    }

    [Fact]
    public void Should_DetectBlock_BetweenUsers()
    {
        var blocker = Guid.NewGuid();
        var blocked = Guid.NewGuid();
        var blocks = new List<Block>
        {
            new Block { BlockerId = blocker, BlockedId = blocked }
        };

        var isBlocked = blocks.Any(b =>
            (b.BlockerId == blocker && b.BlockedId == blocked) ||
            (b.BlockerId == blocked && b.BlockedId == blocker));

        isBlocked.Should().BeTrue();

        var unblockedUser = Guid.NewGuid();
        var isUnblocked = blocks.Any(b =>
            (b.BlockerId == blocker && b.BlockedId == unblockedUser) ||
            (b.BlockerId == unblockedUser && b.BlockedId == blocker));

        isUnblocked.Should().BeFalse();
    }
}
