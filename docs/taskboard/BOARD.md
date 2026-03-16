# Loca Task Board

> Claude Code agents read and update this file. Status changes trigger next actions.
> Statuses: `TODO` | `IN_PROGRESS` | `IN_REVIEW` | `DONE` | `BLOCKED`

---

## Sprint 1 -- Foundation (COMPLETE)

### DONE
- [DONE] LOCA-1: Monorepo scaffolding -> @architect
- [DONE] LOCA-2: ASP.NET Core 8 Clean Architecture solution -> @architect
- [DONE] LOCA-3: PostgreSQL 16 + PostGIS Docker + migrations -> @backend-1
- [DONE] LOCA-4: Redis 7 Docker + connection wrapper -> @backend-1
- [DONE] LOCA-5: React Native Expo project init -> @mobile-1
- [DONE] LOCA-8: Docker Compose full stack -> @backend-1
- [DONE] LOCA-9: Shared TypeScript types -> @mobile-1
- [DONE] LOCA-10: Design system (NativeWind config) -> @mobile-1
- [DONE] LOCA-11: Swagger/OpenAPI + JWT auth setup -> @backend-2

## Sprint 2 -- Auth (COMPLETE)

### DONE
- [DONE] LOCA-20: Identity Service: JWT + refresh rotation -> @backend-1
- [DONE] LOCA-21: Google OAuth 2.0 integration -> @backend-1
- [DONE] LOCA-23: User profile CRUD -> @backend-1
- [DONE] LOCA-25: Onboarding data model -> @backend-1
- [DONE] LOCA-26: Mobile: Login screen -> @mobile-1
- [DONE] LOCA-30: Mobile: Secure token storage -> @mobile-1
- [DONE] LOCA-34: Unit tests: Identity service (3 tests) -> @qa-1

## Sprint 3 -- Venue Discovery (COMPLETE)

### DONE
- [DONE] LOCA-40: Venue Service + PostGIS CRUD -> @backend-1
- [DONE] LOCA-42: Real-time venue counter (Redis) -> @backend-1
- [DONE] LOCA-44: QR validation + check-in endpoint -> @backend-2
- [DONE] LOCA-45: Geofence verification -> @backend-2
- [DONE] LOCA-47: Venue nearby search -> @backend-1
- [DONE] LOCA-48: Mobile: Discover screen -> @mobile-1
- [DONE] LOCA-57: Check-in handler tests (4 tests) -> @qa-1

## Sprint 4 -- Social Hub (COMPLETE)

### DONE
- [DONE] LOCA-60: Social Service + SignalR hub -> @backend-2
- [DONE] LOCA-61: VenueChatHub (Join, Leave, SendMessage) -> @backend-2
- [DONE] LOCA-62: Chat message types -> @backend-2
- [DONE] LOCA-63: Chat persistence (PostgreSQL + Redis) -> @backend-2
- [DONE] LOCA-66: Venue feed (posts, comments, likes) -> @backend-1
- [DONE] LOCA-70: Mobile: Social Hub tab -> @mobile-1

## Sprint 5 -- Games (COMPLETE)

### DONE
- [DONE] LOCA-90: Game Service + GameHub SignalR -> @backend-2
- [DONE] LOCA-91: Game lobby system -> @backend-2
- [DONE] LOCA-92: Server-authoritative game engine -> @backend-2
- [DONE] LOCA-93: Mafia game logic -> @backend-2
- [DONE] LOCA-94: Truth or Dare (Azerbaijani content) -> @backend-2
- [DONE] LOCA-103: Mafia engine tests (5 tests) -> @qa-1

## Sprint 6-8 -- Matching, Economy, Polish (COMPLETE)

### DONE
- [DONE] LOCA-110: Matching entities -> @backend-1
- [DONE] LOCA-113: Private chat (SignalR) -> @backend-2
- [DONE] LOCA-115: Report/Block system -> @backend-1
- [DONE] LOCA-130: Economy Service (wallets, transactions, gifts) -> @backend-1

---

## Build Status
- Backend: PASSING (12 projects, 0 errors)
- Unit Tests: 25/25 PASSING
- Mobile TypeScript: PASSING (0 errors)
- Docker: 4/4 services healthy

---

## Update Rules
1. START task: `[TODO]` -> `[IN_PROGRESS]`
2. FINISH coding: -> `[IN_REVIEW]`
3. QA APPROVES: -> `[DONE]`
4. QA REJECTS: -> `[IN_PROGRESS]` + rejection note
5. BLOCKED: -> `[BLOCKED]` + blocker note
