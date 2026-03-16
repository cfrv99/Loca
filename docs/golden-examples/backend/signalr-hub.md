# Golden Example: SignalR Hub Implementation

Match this pattern for ALL SignalR hubs.

## Hub (C#)
```csharp
[Authorize]
public class VenueChatHub : Hub
{
    private readonly IMediator _mediator;
    private readonly IRedisService _redis;
    private readonly ILogger<VenueChatHub> _logger;

    public VenueChatHub(IMediator mediator, IRedisService redis, ILogger<VenueChatHub> logger)
    {
        _mediator = mediator;
        _redis = redis;
        _logger = logger;
    }

    // ── Connection lifecycle ──
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User!.FindFirst("sub")!.Value;
        await _redis.SetUserOnlineAsync(userId, true);
        _logger.LogInformation("User {UserId} connected to VenueChatHub", userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User!.FindFirst("sub")!.Value;
        await _redis.SetUserOnlineAsync(userId, false);
        // Remove from all venue groups
        var venues = await _redis.GetUserVenuesAsync(userId);
        foreach (var venueId in venues)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"venue_{venueId}");
            await _redis.RemoveActiveUserAsync(venueId, userId);
            await Clients.Group($"venue_{venueId}").SendAsync("userLeft", userId);
        }
        _logger.LogInformation("User {UserId} disconnected", userId);
        await base.OnDisconnectedAsync(exception);
    }

    // ── Client → Server methods (PascalCase) ──
    public async Task JoinVenue(string venueId)
    {
        var userId = Context.User!.FindFirst("sub")!.Value;
        
        // Verify active check-in
        var hasCheckIn = await _mediator.Send(new VerifyActiveCheckInQuery(userId, Guid.Parse(venueId)));
        if (!hasCheckIn)
        {
            await Clients.Caller.SendAsync("error", new { code = "NOT_CHECKED_IN", message = "Check in first" });
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"venue_{venueId}");
        await _redis.AddActiveUserAsync(venueId, userId);
        
        var user = await _mediator.Send(new GetUserBriefQuery(Guid.Parse(userId)));
        await Clients.Group($"venue_{venueId}").SendAsync("userJoined", user);  // camelCase for client
        
        // Send current active users count
        var stats = await _redis.GetVenueStatsAsync(venueId);
        await Clients.Caller.SendAsync("activeUsersUpdated", stats);
    }

    public async Task SendMessage(string venueId, string content, string type, string? replyToId, object? metadata)
    {
        var userId = Context.User!.FindFirst("sub")!.Value;
        
        var result = await _mediator.Send(new SendChatMessageCommand(
            Guid.Parse(userId), Guid.Parse(venueId), content, type, 
            replyToId != null ? Guid.Parse(replyToId) : null, metadata
        ));

        result.Match(
            msg => Clients.Group($"venue_{venueId}").SendAsync("receiveMessage", msg),  // camelCase
            err => Clients.Caller.SendAsync("error", new { code = err.Code, message = err.Message })
        );
    }

    public async Task StartTyping(string venueId)
    {
        var userId = Context.User!.FindFirst("sub")!.Value;
        var name = Context.User!.FindFirst("name")?.Value ?? "Anonymous";
        await Clients.OthersInGroup($"venue_{venueId}").SendAsync("typingStarted", userId, name);
    }
}
```

## Mobile Hook (TypeScript)
```tsx
import { useEffect, useRef, useCallback, useState } from 'react';
import { HubConnection, HubConnectionBuilder, LogLevel, HttpTransportType } from '@microsoft/signalr';
import { useAuthStore } from '../stores/auth-store';

const API_URL = __DEV__ ? 'http://localhost:5001' : 'https://api.loca.az';

export function useSignalR(hubPath: string) {
  const connectionRef = useRef<HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const { accessToken } = useAuthStore();
  const retryDelayRef = useRef(1000);

  useEffect(() => {
    if (!accessToken) return;

    const connection = new HubConnectionBuilder()
      .withUrl(`${API_URL}${hubPath}`, {
        accessTokenFactory: () => accessToken,
        transport: HttpTransportType.WebSockets,
        skipNegotiation: true,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (context) => {
          // Exponential backoff: 1s, 2s, 4s, 8s, max 30s
          const delay = Math.min(retryDelayRef.current, 30000);
          retryDelayRef.current *= 2;
          return delay;
        },
      })
      .configureLogging(__DEV__ ? LogLevel.Information : LogLevel.Error)
      .build();

    connection.onreconnecting(() => setIsConnected(false));
    connection.onreconnected(() => { setIsConnected(true); retryDelayRef.current = 1000; });
    connection.onclose(() => setIsConnected(false));

    connection.start()
      .then(() => { setIsConnected(true); retryDelayRef.current = 1000; })
      .catch((err) => console.error('SignalR connect failed:', err));

    connectionRef.current = connection;
    return () => { connection.stop(); };
  }, [accessToken, hubPath]);

  return { connection: connectionRef.current, isConnected };
}
```

## Key Rules
- Hub methods: PascalCase (C#) — `SendMessage`, `JoinVenue`
- Client callbacks: camelCase (JS) — `receiveMessage`, `userJoined`
- ALWAYS verify auth (JWT from Context.User)
- ALWAYS verify business rules (check-in active, not blocked, etc.)
- Use Redis backplane groups for venue/conversation scoping
- Handle disconnect gracefully (remove from groups, update counters)
- Auto-reconnect with exponential backoff on client
