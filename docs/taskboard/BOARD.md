# Loca Task Board

> Agents read/update this file. Status changes trigger next actions.
> Format: `- [STATUS] LOCA-{id}: {title} -> @{assignee}`
> Statuses: `TODO` | `IN_PROGRESS` | `IN_REVIEW` | `DONE` | `BLOCKED`

---

## Sprint 1 — Foundation (Week 1-2) [COMPLETE]

### DONE
- [DONE] LOCA-1: Monorepo scaffolding -> @architect
- [DONE] LOCA-2: ASP.NET Core 8 solution + Clean Architecture -> @architect
- [DONE] LOCA-3: Docker Compose (PG+Redis+MinIO) -> @backend-1
- [DONE] LOCA-4: DB schemas + PostGIS + seed data (15 venues, 10 users) -> @backend-1
- [DONE] LOCA-5: Redis connection wrapper + health check -> @backend-1
- [DONE] LOCA-6: Expo project init (Router+NativeWind+TS+Zustand+RQ) -> @mobile-1
- [DONE] LOCA-7: Shared UI: LoadingSkeleton, ErrorState, EmptyState, Button, Avatar -> @mobile-1
- [DONE] LOCA-8: Design tokens (tailwind.config.js) -> @mobile-2
- [DONE] LOCA-9: Swagger/OpenAPI + API versioning + ApiResponse<T> -> @backend-2
- [DONE] LOCA-10: SignalR base hub + Redis backplane -> @backend-2
- [DONE] LOCA-12: Environment configs (dev/staging/prod) -> @architect
- [DONE] LOCA-13: Shared TypeScript types package -> @mobile-2
- [DONE] LOCA-14: App shell tabs (Discover/Hub/Matches/Profile) -> @mobile-1
- [DONE] LOCA-15: Sprint 1 QA verification -> @qa-1

## Sprint 2 — Auth (Week 3-4) [COMPLETE]

### DONE
- [DONE] LOCA-20: Identity Service: User entity + JWT gen + refresh rotation -> @backend-1
- [DONE] LOCA-21: Google OAuth backend -> @backend-1
- [DONE] LOCA-23: Profile CRUD: GET/PUT /users/me + onboarding -> @backend-1
- [DONE] LOCA-25: Auth middleware: JWT validation -> @backend-2
- [DONE] LOCA-26: Mobile: Login screen (Google + Apple buttons) -> @mobile-1
- [DONE] LOCA-28: Mobile: API client with JWT interceptor -> @mobile-1
- [DONE] LOCA-32: Unit tests: JWT gen, validation, profile validators -> @qa-1

## Sprint 3 — Venue Discovery + QR (Week 5-6) [COMPLETE]

### DONE
- [DONE] LOCA-40: Venue Service: entity + geography + CRUD -> @backend-1
- [DONE] LOCA-41: Real-time counter: Redis per venue -> @backend-1
- [DONE] LOCA-42: Nearby search: GET /venues/nearby -> @backend-1
- [DONE] LOCA-43: QR system: TOTP rotating payload (60s) -> @backend-2
- [DONE] LOCA-44: Check-in: POST /checkin -> @backend-2
- [DONE] LOCA-46: Mobile: Home/Discover screen -> @mobile-1
- [DONE] LOCA-54: Tests: check-in handler tests -> @qa-1

## Sprint 4 — Social Hub: Chat + People (Week 7-8) [COMPLETE]

### DONE
- [DONE] LOCA-60: Social Service: Message entities + EF config -> @backend-2
- [DONE] LOCA-61: VenueChatHub: JoinVenue, LeaveVenue, SendMessage -> @backend-2
- [DONE] LOCA-62: Message types + persistence (PG + Redis cache) -> @backend-2
- [DONE] LOCA-63: Active users: Redis SET per venue -> @backend-1
- [DONE] LOCA-67: Mobile: useSignalR hook -> @mobile-2

## Sprint 5 — Feed + Games (Week 9-10) [COMPLETE]

### DONE
- [DONE] LOCA-81: Game Service: GameSession + GameHub SignalR -> @backend-2
- [DONE] LOCA-82: Game lobby: create/join/start -> @backend-2
- [DONE] LOCA-83: Game engine: state machine + reconnect grace -> @backend-2
- [DONE] LOCA-84: Mafia: roles, night/day, voting, elimination -> @backend-1
- [DONE] LOCA-85: Truth or Dare: 35 AZ questions, random pick -> @backend-1
- [DONE] LOCA-92: Game tests: Mafia roles, player limits -> @qa-1

## Sprint 6 — Matching + Private Chat (Week 11-12) [COMPLETE]

### DONE
- [DONE] LOCA-100: Match request entity + social configurations -> @backend-1
- [DONE] LOCA-102: Conversation + Block + Report entities -> @backend-2
- [DONE] LOCA-104: Report/Block entities + configurations -> @backend-1

## Sprint 7 — Gifting + Economy (Week 13-14) [COMPLETE]

### DONE
- [DONE] LOCA-120: Economy Service: Wallet, Transaction, GiftCatalog -> @backend-1
- [DONE] LOCA-122: Gift catalog entity + coin packages -> @backend-1

## Sprint 8 — Launch Prep (Week 15-16) [IN PROGRESS]

### TODO
- [TODO] LOCA-140: Production: Docker + Nginx + SSL -> @architect
- [TODO] LOCA-148: Admin web MVP -> @mobile-2
- [TODO] LOCA-151: Security audit -> @qa-1

---

## Build Summary

### Backend (.NET 8)
- **12 projects** in solution (Domain, Application, Infrastructure, API, 6 services, 2 test projects)
- **Build**: 0 errors, 0 warnings
- **Tests**: 27 passing (unit tests for Result, Venue, CheckIn, GoogleLogin, Mafia, Validators)
- **Architecture**: Clean Architecture + CQRS (MediatR) + FluentValidation + Result<T>
- **Database**: EF Core 8 + PostgreSQL + PostGIS (6 schemas: identity, venue, social, game, economy, notification)
- **Real-time**: SignalR hubs (VenueChatHub, GameHub) with Redis backplane
- **Auth**: JWT Bearer + Google OAuth + refresh token rotation

### Mobile (React Native Expo)
- **Expo Router** file-based navigation with 4 tabs
- **NativeWind** styling with custom design tokens
- **Zustand** auth store + **React Query** data fetching
- **SignalR** hook with auto-reconnect + exponential backoff
- **Screens**: Login, Discover, Hub, Matches, Profile
- **Components**: VenueCard, LoadingSkeleton, ErrorState, EmptyState
- **All UI strings in Azerbaijani**

### Infrastructure
- **Docker Compose**: PostgreSQL 16+PostGIS, Redis 7, MinIO, Adminer, Redis Commander
