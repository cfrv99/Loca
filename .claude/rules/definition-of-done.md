# Definition of Done (DoD) — MUST check before marking any task complete

## For EVERY task:
- [ ] Code compiles without errors (`dotnet build` or `npx tsc --noEmit`)
- [ ] At least 1 test written (unit or integration)
- [ ] All existing tests still pass
- [ ] No hardcoded secrets, passwords, or API keys
- [ ] Git commit with proper format: `type(scope): description [LOCA-{id}]`

## For Backend tasks:
- [ ] FluentValidation on every input field
- [ ] Result<T> pattern used (no thrown exceptions for business logic)
- [ ] ApiResponse<T> wrapper on all controller responses
- [ ] XML comments on controller methods (Swagger auto-gen)
- [ ] EF Core migration created if schema changed
- [ ] Logging added for important operations (LogInformation, LogWarning)

## For Mobile tasks:
- [ ] Loading, error, and empty states handled in every screen
- [ ] NativeWind classes only (no inline style objects)
- [ ] Dark mode supported (dark: prefix on all colored classes)
- [ ] Touch targets ≥ 44x44 points
- [ ] FlatList for scrollable lists (not ScrollView for dynamic data)
- [ ] React Query for all server data, Zustand only for client state
- [ ] accessibilityLabel on all interactive elements

## For SignalR/Real-time tasks:
- [ ] Hub methods: PascalCase (server), camelCase (client callbacks)
- [ ] Reconnection handling with exponential backoff
- [ ] Group management: join on connect, leave on disconnect
- [ ] Error event handling (connection lost, timeout)

## For Game tasks:
- [ ] Server-authoritative — client sends actions, server validates
- [ ] Fog of war — players see only their allowed data
- [ ] Disconnect grace period (60s)
- [ ] Timeout auto-action for AFK players
- [ ] Full game simulation test (lobby → play → result)

## IMPORTANT: Claude MUST run the Stop hook verification before completing any task.
The Stop hook will auto-check build, types, and tests. If it fails, fix before stopping.
