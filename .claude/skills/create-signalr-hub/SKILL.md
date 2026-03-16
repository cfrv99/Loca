---
name: create-signalr-hub
description: Use when implementing any real-time feature (chat, games, notifications, matching). Creates server hub + client hook + tests following SignalR contracts.
---

# Create SignalR Hub Skill

## Step 1: Read the Contract FIRST
Read `docs/architecture/signalr-contracts.md` for the hub you're implementing.
If the hub isn't documented → document it first, then implement.

## Step 2: Use context7
```
"use context7 for SignalR .NET 8 hubs groups Redis backplane"
"use context7 for @microsoft/signalr JavaScript client"
```

## Step 3: Backend Hub
Match `docs/golden-examples/backend/signalr-hub.md` pattern exactly:
1. [Authorize] attribute on hub class
2. OnConnectedAsync: extract userId from JWT, set online in Redis, log
3. OnDisconnectedAsync: cleanup groups, remove from active sets, broadcast
4. Each method: verify auth → validate business rule → persist if needed → broadcast to group
5. Server methods: PascalCase. Client callbacks: camelCase.
6. Use `Clients.Group("venue_{id}")` for venue-scoped, `Clients.User(userId)` for targeted
7. Error handling: `Clients.Caller.SendAsync("error", ...)` — never throw

## Step 4: Mobile Hook
Match `docs/golden-examples/backend/signalr-hub.md` useSignalR pattern:
1. Create `useSignalR(hubPath)` if not exists (shared hook)
2. Create feature hook: `useVenueChat(venueId)` / `useGameSession(sessionId)`
3. In useEffect: invoke join → register all `on()` listeners → return cleanup
4. Expose: data + actions + connection state

## Step 5: Tests
- Hub test: mock connections, verify group management, verify broadcasts
- Client test: verify hook connects, listens, and cleans up

## Checklist
- [ ] Contract exists in signalr-contracts.md?
- [ ] Hub has [Authorize]?
- [ ] OnConnectedAsync + OnDisconnectedAsync handle cleanup?
- [ ] Server methods validate auth + business rules?
- [ ] Client callbacks are camelCase?
- [ ] Mobile hook has auto-reconnect?
- [ ] Mobile hook cleans up listeners on unmount?
- [ ] Redis backplane configured for horizontal scaling?
