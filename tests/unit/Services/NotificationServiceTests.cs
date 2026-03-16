using FluentAssertions;
using Loca.Application.DTOs;
using Loca.Application.Interfaces;
using Loca.Domain.Entities;
using Loca.Infrastructure.Persistence;
using Loca.Services.Notification;
using Loca.Services.Notification.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Loca.Tests.Unit.Services;

public class NotificationServiceTests
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IHubClients _hubClients;
    private readonly IClientProxy _clientProxy;
    private readonly LocaDbContext _db;
    private readonly ILogger<NotificationService> _logger;
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _hubContext = Substitute.For<IHubContext<NotificationHub>>();
        _hubClients = Substitute.For<IHubClients>();
        _clientProxy = Substitute.For<IClientProxy>();
        _logger = Substitute.For<ILogger<NotificationService>>();

        _hubContext.Clients.Returns(_hubClients);
        _hubClients.Group(Arg.Any<string>()).Returns(_clientProxy);

        var options = new DbContextOptionsBuilder<LocaDbContext>()
            .UseInMemoryDatabase(databaseName: $"NotificationTest_{Guid.NewGuid()}")
            .Options;
        _db = new LocaDbContext(options);

        _service = new NotificationService(_hubContext, _db, _logger);
    }

    [Fact]
    public async Task Should_SendMatchRequestNotification_ViaSignalR()
    {
        var receiverId = Guid.NewGuid();
        var notification = new MatchNotificationDto(
            Guid.NewGuid(), Guid.NewGuid(), "Test User", null, "Salam!", Guid.NewGuid());

        await _service.SendMatchRequestNotificationAsync(receiverId, notification);

        _hubClients.Received(1).Group($"user_{receiverId}");
        await _clientProxy.Received(1).SendCoreAsync(
            "matchRequestReceived",
            Arg.Any<object[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_SendMatchAcceptedNotification_ViaSignalR()
    {
        var userId = Guid.NewGuid();
        var notification = new MatchAcceptedNotificationDto(Guid.NewGuid(), Guid.NewGuid(), "Responder");

        await _service.SendMatchAcceptedNotificationAsync(userId, notification);

        _hubClients.Received(1).Group($"user_{userId}");
        await _clientProxy.Received(1).SendCoreAsync(
            "matchAccepted",
            Arg.Any<object[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_SendGiftNotification_ViaSignalR()
    {
        var recipientId = Guid.NewGuid();
        var notification = new GiftNotificationDto(
            Guid.NewGuid(), "Sender", "Rose", "Basic", null, 10, "public_chat");

        await _service.SendGiftNotificationAsync(recipientId, notification);

        _hubClients.Received(1).Group($"user_{recipientId}");
        await _clientProxy.Received(1).SendCoreAsync(
            "giftReceived",
            Arg.Any<object[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_RegisterDeviceToken_NewToken()
    {
        var userId = Guid.NewGuid();

        await _service.RegisterDeviceTokenAsync(userId, "fcm-token-123", "android");

        var token = await _db.DeviceTokens.FirstOrDefaultAsync(d => d.UserId == userId);
        token.Should().NotBeNull();
        token!.Token.Should().Be("fcm-token-123");
        token.Platform.Should().Be("android");
        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Should_UpdateExistingDeviceToken_WhenSameUserAndToken()
    {
        var userId = Guid.NewGuid();

        // Register first time
        await _service.RegisterDeviceTokenAsync(userId, "fcm-token-123", "android");

        // Register same token again (should update, not duplicate)
        await _service.RegisterDeviceTokenAsync(userId, "fcm-token-123", "ios");

        var tokens = await _db.DeviceTokens.Where(d => d.UserId == userId).ToListAsync();
        tokens.Should().HaveCount(1);
        tokens[0].Platform.Should().Be("ios");
    }

    [Fact]
    public async Task Should_LogNotification_ToDatabase()
    {
        var userId = Guid.NewGuid();

        await _service.LogNotificationAsync(userId, "test_type", "Test Title", "Test Body", new { key = "value" });

        var log = await _db.NotificationLogs.FirstOrDefaultAsync(n => n.UserId == userId);
        log.Should().NotBeNull();
        log!.Type.Should().Be("test_type");
        log.Title.Should().Be("Test Title");
        log.Body.Should().Be("Test Body");
        log.IsSent.Should().BeTrue();
        log.SentAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_PersistNotificationLog_WhenSendingMatchRequest()
    {
        var receiverId = Guid.NewGuid();
        var notification = new MatchNotificationDto(
            Guid.NewGuid(), Guid.NewGuid(), "Ali", null, null, Guid.NewGuid());

        await _service.SendMatchRequestNotificationAsync(receiverId, notification);

        var logs = await _db.NotificationLogs.Where(n => n.UserId == receiverId).ToListAsync();
        logs.Should().HaveCount(1);
        logs[0].Type.Should().Be("match_request");
    }
}
