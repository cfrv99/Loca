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

## Sprint 2 — Auth & Onboarding (Week 3-4) [COMPLETE]

### DONE
- [DONE] LOCA-20: Identity Service: User entity + JWT gen + refresh rotation -> @backend-1
- [DONE] LOCA-21: Google OAuth backend -> @backend-1
- [DONE] LOCA-22: Apple Sign-In backend -> @backend-1
- [DONE] LOCA-23: Profile CRUD: GET/PUT /users/me + onboarding -> @backend-1
- [DONE] LOCA-25: Auth middleware: JWT validation -> @backend-2
- [DONE] LOCA-26: Mobile: Login screen (Google + Apple buttons) -> @mobile-1
- [DONE] LOCA-28: Mobile: API client with JWT interceptor -> @mobile-1
- [DONE] LOCA-29: Mobile: 5-step onboarding wizard -> @mobile-1
- [DONE] LOCA-30: Mobile: Profile view/edit screen -> @mobile-1
- [DONE] LOCA-32: Unit tests: JWT gen, validation, profile validators -> @qa-1
- [DONE] LOCA-35: Onboarding endpoint: PUT /auth/onboarding -> @backend-1

## Sprint 3 — Venue Discovery + QR (Week 5-6) [COMPLETE]

### DONE
- [DONE] LOCA-40: Venue Service: entity + geography + CRUD -> @backend-1
- [DONE] LOCA-41: Real-time counter: Redis per venue -> @backend-1
- [DONE] LOCA-42: Nearby search: GET /venues/nearby -> @backend-1
- [DONE] LOCA-43: QR system: TOTP rotating payload (60s) -> @backend-2
- [DONE] LOCA-44: Check-in: POST /checkin -> @backend-2
- [DONE] LOCA-45: Check-out: POST /checkout -> @backend-1
- [DONE] LOCA-46: Mobile: Home/Discover screen -> @mobile-1
- [DONE] LOCA-47: Mobile: Venue detail screen -> @mobile-1
- [DONE] LOCA-48: Mobile: QR scanner (expo-camera barcode) -> @mobile-1
- [DONE] LOCA-49: Mobile: Location permission flow (expo-location) -> @mobile-1
- [DONE] LOCA-50: Mobile: Map view with venue markers + activity coloring -> @mobile-2
- [DONE] LOCA-54: Tests: check-in handler tests -> @qa-1
- [DONE] LOCA-VD: Venue detail endpoint: GET /venues/{id} -> @backend-1
- [DONE] LOCA-VS: Venue stats endpoint: GET /venues/{id}/stats -> @backend-1
- [DONE] LOCA-VP: Venue people endpoint: GET /venues/{id}/people -> @backend-1
- [DONE] LOCA-VM: Venue messages endpoint: GET /venues/{id}/messages -> @backend-1

## Sprint 4 — Social Hub: Chat + People (Week 7-8) [COMPLETE]

### DONE
- [DONE] LOCA-60: Social Service: Message entities + EF config -> @backend-2
- [DONE] LOCA-61: VenueChatHub: JoinVenue, LeaveVenue, SendMessage, reactions, typing, pinning -> @backend-2
- [DONE] LOCA-62: Message types + persistence (PG + Redis cache) -> @backend-2
- [DONE] LOCA-63: Active users: Redis SET per venue -> @backend-1
- [DONE] LOCA-64: People browser: GET /venues/{id}/people -> @backend-1
- [DONE] LOCA-66: Push notification service + NotificationHub + device token endpoint -> @backend-2
- [DONE] LOCA-67: Mobile: useSignalR hook -> @mobile-2
- [DONE] LOCA-68: Mobile: Social Hub tabs (Chat/People/Feed/Games) -> @mobile-1
- [DONE] LOCA-69: Mobile: Public chat screen (messages, input, send, scroll, load-more) -> @mobile-2
- [DONE] LOCA-70: Mobile: Chat components (text bubble, image, GIF, reply, timestamp, reactions) -> @mobile-2
- [DONE] LOCA-71: Mobile: People browser grid (avatar, name, interests) -> @mobile-1
- [DONE] LOCA-72: Mobile: Other user profile screen -> @mobile-1

## Sprint 5 — Feed + Games (Week 9-10) [COMPLETE]

### DONE
- [DONE] LOCA-80: Venue feed: Post CRUD, like/comment endpoints -> @backend-1
- [DONE] LOCA-81: Game Service: GameSession + GameHub SignalR -> @backend-2
- [DONE] LOCA-82: Game lobby: create/join/start -> @backend-2
- [DONE] LOCA-83: Game engine: state machine + 60s reconnect grace -> @backend-2
- [DONE] LOCA-84: Mafia: roles, night/day, voting, elimination -> @backend-1
- [DONE] LOCA-85: Truth or Dare: 35 AZ questions, random pick -> @backend-1
- [DONE] LOCA-87: Mobile: Feed tab (vertical scroll, like, comment, post creation) -> @mobile-2
- [DONE] LOCA-88: Mobile: Games tab + active games list + "Create Game" -> @mobile-2
- [DONE] LOCA-89: Mobile: Game lobby UI (avatars, ready, host controls, countdown) -> @mobile-2
- [DONE] LOCA-90: Mobile: Mafia UI (role reveal, voting, night action, results) -> @mobile-2
- [DONE] LOCA-91: Mobile: Truth or Dare UI (card flip, timer, skip) -> @mobile-2
- [DONE] LOCA-92: Game tests: Mafia roles, player limits, GameHub -> @qa-1

## Sprint 6 — Matching + Private Chat (Week 11-12) [COMPLETE]

### DONE
- [DONE] LOCA-100: Match request: send/respond/inbox + same-venue + 5/day limit + spam protection -> @backend-1
- [DONE] LOCA-102: PrivateChatHub: SendPrivateMessage, MarkRead, typing, read receipts, online status -> @backend-2
- [DONE] LOCA-103: Typing indicator + read receipts + online status (Redis) -> @backend-2
- [DONE] LOCA-104: Report/Block: endpoints + handlers + UsersController -> @backend-1
- [DONE] LOCA-105: Mobile: Match request button on profile + request sheet -> @mobile-1
- [DONE] LOCA-106: Mobile: Match inbox (pending/accepted/declined) -> @mobile-1
- [DONE] LOCA-107: Mobile: Matches tab — conversations list -> @mobile-2
- [DONE] LOCA-108: Mobile: Private chat (reuse components + typing + read receipts) -> @mobile-2
- [DONE] LOCA-109: Mobile: Report/Block flow (reason sheet, confirm) -> @mobile-1

## Sprint 7 — Gifting + Economy (Week 13-14) [COMPLETE]

### DONE
- [DONE] LOCA-120: Economy Service: Wallet, Transaction, GiftCatalog entities -> @backend-1
- [DONE] LOCA-121: Economy endpoints: balance, gift catalog, send gift, purchase coins -> @backend-1
- [DONE] LOCA-122: Gift catalog entity + coin packages -> @backend-1
- [DONE] LOCA-123: Send gift handler: validate balance, deduct, transaction, SignalR broadcast -> @backend-1
- [DONE] LOCA-125: Mobile: Coin shop (4 IAP packages) -> @mobile-1
- [DONE] LOCA-126: Mobile: Gift picker bottom sheet -> @mobile-1
- [DONE] LOCA-127: Mobile: Gift button in chat input + send in public/private chat -> @mobile-2
- [DONE] LOCA-128: Mobile: Gift animation system (full-screen animations per tier) -> @mobile-2

## Sprint 7 — Gifting + Economy (Week 13-14) [COMPLETE]

### DONE
- [DONE] LOCA-120: Economy Service: Wallet, Transaction, GiftCatalog entities -> @backend-1
- [DONE] LOCA-121: Economy endpoints: balance, gift catalog, send gift, purchase coins -> @backend-1
- [DONE] LOCA-122: Gift catalog entity + coin packages -> @backend-1
- [DONE] LOCA-123: Send gift handler: validate balance, deduct, transaction, SignalR broadcast -> @backend-1
- [DONE] LOCA-125: Mobile: Coin shop (4 IAP packages) -> @mobile-1
- [DONE] LOCA-126: Mobile: Gift picker bottom sheet -> @mobile-1
- [DONE] LOCA-127: Mobile: Gift button in chat input + send in public/private chat -> @mobile-2
- [DONE] LOCA-128: Mobile: Gift animation system (full-screen animations per tier) -> @mobile-2

## Sprint 8 — Launch Prep (Week 15-16) [TODO]

### TODO
- [TODO] LOCA-140: Production: Docker + Nginx + SSL -> @architect
- [TODO] LOCA-144: EAS Build: dev+prod profiles, iOS+Android -> @mobile-1
- [TODO] LOCA-148: Admin web MVP: venue CRUD + QR display + stats -> @mobile-2
- [TODO] LOCA-149: PostHog analytics -> @backend-2
- [TODO] LOCA-151: Security audit: OWASP top 10 -> @qa-1
- [TODO] LOCA-152: Full regression on staging -> @qa-1
- [TODO] LOCA-153: Mobile E2E suite -> @qa-2

---

## Build Summary (Sprint 7 Complete)

### Backend (.NET 8 — 157 C# files across 12 projects)
- **7 Controllers**: Auth, Venues, Matches, Economy, Feed, Users, Notification
- **28 Handlers**: All CQRS handlers for identity, venue, social, game, economy
- **28 Commands/Queries**: Full CRUD for all domains
- **13 Validators**: FluentValidation on every command
- **6 Repositories**: User, Venue, CheckIn, Match, Economy, Post
- **4 SignalR Hubs**: VenueChatHub, GameHub, PrivateChatHub, NotificationHub
- **Build**: 0 errors, 0 warnings
- **Tests**: 85 passing (unit tests across all domains)
- **Architecture**: Clean Architecture + CQRS (MediatR) + FluentValidation + Result<T>

### Mobile (React Native Expo — 63 TypeScript files)
- **15 Screens**: Login, Onboarding, Discover, Venue Detail, QR Scanner, Hub (4 tabs), Matches, Profile, Profile Edit, Private Chat, Game, User Profile, Coin Shop, Map View
- **20 Feature Components**: ChatBubble, MessageInput, TypingIndicator, ReactionPicker, PeopleGrid, VenueCard, VenueFeed, GamesList, GiftPicker, GiftAnimation, MatchRequestSheet, ReportSheet, etc.
- **12 Custom Hooks**: useSignalR, useVenueChat, usePrivateChat, useGameLobby, useMafiaGame, useVenuesNearby, useVenueDetail, useCheckin, useLocation, useMatches, useEconomy, usePublicProfile
- **2 Stores**: AuthStore (Zustand), VenueStore (Zustand)
- **All UI in Azerbaijani**, NativeWind styling, dark mode support

### Infrastructure
- **Docker Compose**: PostgreSQL 16+PostGIS, Redis 7, MinIO, Adminer, Redis Commander
- **SignalR**: Redis backplane, 4 hubs, auto-reconnect
