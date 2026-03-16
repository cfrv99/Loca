# LOCA — Epic & Sprint Master Plan (v2 — Audited & Fixed)

## Capacity Rules
- **Team:** 7 agents (architect, backend-1, backend-2, mobile-1, mobile-2, qa-1, qa-2)
- **Sprint:** 2 weeks, max 65 SP (with 20% buffer = effective 52 SP target)
- **Per agent per sprint:** max 13 SP (realistic for 2 weeks)
- **Non-dev roles** (@designer, @pm, @ba): tasks reassigned to dev agents or marked HUMAN-ONLY

---

## Phase 1: MVP (Month 1-4)

### EPIC-1: Project Foundation & Infrastructure
**Sprint 1 (Week 1-2) — "Ground Zero" | 48 SP**

| ID | Task | Assignee | Depends On | SP | Pri |
|----|------|----------|-----------|-----|-----|
| LOCA-1 | Monorepo scaffolding (apps/, packages/, services/, infrastructure/) + .gitignore + README | @architect | — | 3 | P0 |
| LOCA-2 | ASP.NET Core 8 solution: Clean Architecture layers + MediatR + FluentValidation + Result<T> | @architect | LOCA-1 | 5 | P0 |
| LOCA-3 | Docker Compose: PostgreSQL 16+PostGIS, Redis 7, MinIO, Adminer, Redis Commander | @backend-1 | LOCA-1 | 3 | P0 |
| LOCA-4 | DB schemas (identity/venue/social/game/economy) + PostGIS extension + seed script (15 Baku venues, 10 users) | @backend-1 | LOCA-3 | 5 | P0 |
| LOCA-5 | Redis connection wrapper (StackExchange.Redis) + health check | @backend-1 | LOCA-3 | 2 | P0 |
| LOCA-6 | Expo project init: Expo Router + NativeWind + TypeScript + Zustand + React Query boilerplate | @mobile-1 | LOCA-1 | 5 | P0 |
| LOCA-7 | Shared UI components: LoadingSkeleton, ErrorState, EmptyState, Button, Avatar, Badge | @mobile-1 | LOCA-6 | 3 | P0 |
| LOCA-8 | Design tokens: tailwind.config.js matching .claude/rules/design-system.md | @mobile-2 | LOCA-6 | 2 | P0 |
| LOCA-9 | Swagger/OpenAPI auto-gen + API versioning + ApiResponse<T> wrapper | @backend-2 | LOCA-2 | 3 | P0 |
| LOCA-10 | SignalR base hub + Redis backplane + CORS for mobile | @backend-2 | LOCA-2, LOCA-5 | 3 | P0 |
| LOCA-11 | GitHub Actions CI: backend build+test, mobile tsc+jest | @backend-2 | LOCA-2, LOCA-6 | 3 | P1 |
| LOCA-12 | Environment configs: dev/staging/prod appsettings + .env.example | @architect | LOCA-3 | 2 | P1 |
| LOCA-13 | Shared TypeScript types package matching OpenAPI spec | @mobile-2 | LOCA-9 | 2 | P1 |
| LOCA-14 | App shell: Expo Router tabs (Discover/Hub/Matches/Profile) + auth guard skeleton | @mobile-1 | LOCA-6, LOCA-8 | 3 | P0 |
| LOCA-15 | Sprint 1 QA: all builds pass, Docker runs, Swagger loads, Expo starts, CI green | @qa-1 | ALL | 3 | P0 |

---

### EPIC-2: Authentication & User Profile
**Sprint 2 (Week 3-4) — "Who Are You?" | 58 SP**

| ID | Task | Assignee | Depends On | SP | Pri |
|----|------|----------|-----------|-----|-----|
| LOCA-20 | Identity Service: User entity + JWT gen + refresh rotation | @backend-1 | LOCA-2, LOCA-4 | 5 | P0 |
| LOCA-21 | Google OAuth backend: validate ID token → create/link user → return JWT | @backend-1 | LOCA-20 | 3 | P0 |
| LOCA-22 | Apple Sign-In backend: validate identity token → create/link user → return JWT | @backend-1 | LOCA-20 | 3 | P0 |
| LOCA-23 | Profile CRUD: GET/PUT /users/me + onboarding fields (interests, purpose, vibe, privacy) | @backend-1 | LOCA-20 | 3 | P0 |
| LOCA-24 | Photo upload: POST /users/me/avatar → MinIO + resize (128px thumb, 512px medium) | @backend-2 | LOCA-23 | 3 | P1 |
| LOCA-25 | Auth middleware: JWT validation, [Authorize], user extraction from claims | @backend-2 | LOCA-20 | 2 | P0 |
| LOCA-26 | Mobile: Login screen (Google + Apple buttons, logo, loading) | @mobile-1 | LOCA-7, LOCA-8 | 3 | P0 |
| LOCA-27 | Mobile: AuthSession (Google+Apple) + expo-secure-store token storage | @mobile-1 | LOCA-26, LOCA-21 | 5 | P0 |
| LOCA-28 | Mobile: API client with JWT interceptor (auto-attach, auto-refresh on 401) | @mobile-1 | LOCA-27 | 3 | P0 |
| LOCA-29 | Mobile: 5-step onboarding wizard | @mobile-2 | LOCA-28, LOCA-23 | 5 | P0 |
| LOCA-30 | Mobile: Profile view/edit screen | @mobile-2 | LOCA-28, LOCA-23 | 3 | P1 |
| LOCA-31 | Generate TypeScript API client from OpenAPI (openapi-typescript-codegen) | @mobile-1 | LOCA-9, LOCA-23 | 2 | P0 |
| LOCA-32 | Unit tests: JWT gen, refresh, validation, profile validators | @qa-1 | LOCA-20 | 3 | P0 |
| LOCA-33 | Integration tests: register, login, refresh, profile CRUD | @qa-1 | LOCA-23 | 5 | P0 |
| LOCA-34 | Mobile tests: login flow, onboarding nav, token storage | @qa-2 | LOCA-29 | 3 | P1 |
| LOCA-35 | Acceptance criteria: expired token, revoked OAuth, duplicate email, under-18 | @architect | LOCA-20 | 2 | P0 |

---

### EPIC-3: Venue Discovery & QR Check-in
**Sprint 3 (Week 5-6) — "Find & Enter" | 62 SP**

| ID | Task | Assignee | Depends On | SP | Pri |
|----|------|----------|-----------|-----|-----|
| LOCA-40 | Venue Service: entity + PostGIS geography + CRUD + Fluent API | @backend-1 | LOCA-4 | 5 | P0 |
| LOCA-41 | Real-time counter: Redis HINCRBY per venue (total/male/female) + stats endpoint | @backend-1 | LOCA-5, LOCA-40 | 3 | P0 |
| LOCA-42 | Nearby search: GET /venues/nearby PostGIS ST_DWithin + counter data joined | @backend-1 | LOCA-41 | 3 | P0 |
| LOCA-43 | QR system: TOTP rotating payload (60s), generation endpoint for tablet | @backend-2 | LOCA-40 | 5 | P0 |
| LOCA-44 | Check-in: POST /checkin — QR validate + geofence + spoof detect + rate limit | @backend-2 | LOCA-43, LOCA-41 | 5 | P0 |
| LOCA-45 | Check-out: manual + auto (background job, 10min outside geofence) | @backend-2 | LOCA-44 | 3 | P1 |
| LOCA-46 | Mobile: Home/Discover screen — venue cards, distance, counts, activity dot | @mobile-1 | LOCA-42, LOCA-7, LOCA-31 | 5 | P0 |
| LOCA-47 | Mobile: Venue detail screen (info, stats, "Scan QR" CTA) | @mobile-1 | LOCA-46 | 3 | P0 |
| LOCA-48 | Mobile: QR scanner (expo-camera barcode) + check-in call + success/error | @mobile-1 | LOCA-47, LOCA-44 | 3 | P0 |
| LOCA-49 | Mobile: Location permission flow (expo-location, explain dialog, denied fallback) | @mobile-2 | LOCA-6 | 3 | P0 |
| LOCA-50 | Mobile: Map view with venue markers + activity heat coloring | @mobile-2 | LOCA-42, LOCA-49 | 5 | P1 |
| LOCA-51 | Mobile: Check-in success → navigate to Social Hub placeholder | @mobile-2 | LOCA-48 | 2 | P0 |
| LOCA-52 | Venue tablet PWA: dynamic QR auto-refresh every 60s | @mobile-2 | LOCA-43 | 3 | P1 |
| LOCA-53 | Regen TypeScript API client with venue+checkin endpoints | @mobile-1 | LOCA-42, LOCA-31 | 1 | P0 |
| LOCA-54 | Tests: check-in (valid, invalid QR, outside geofence, rate limit, auto-checkout) | @qa-1 | LOCA-44 | 5 | P0 |
| LOCA-55 | Mobile tests: discover render, QR scanner mock, check-in flow | @qa-2 | LOCA-48 | 3 | P1 |

---

### EPIC-4: Social Hub — Chat + People
**Sprint 4 (Week 7-8) — "Let's Talk" | 61 SP**

| ID | Task | Assignee | Depends On | SP | Pri |
|----|------|----------|-----------|-----|-----|
| LOCA-60 | Social Service: Message + ChatRoom entities + EF config | @backend-2 | LOCA-4 | 3 | P0 |
| LOCA-61 | VenueChatHub: JoinVenue, LeaveVenue, SendMessage, ReceiveMessage + groups | @backend-2 | LOCA-60, LOCA-10 | 5 | P0 |
| LOCA-62 | Message types: text, emoji, GIF URL, image URL + persistence (PG + Redis cache 100) | @backend-2 | LOCA-61 | 5 | P0 |
| LOCA-63 | Active users: Redis SET per venue + join/leave broadcast + REST endpoint | @backend-1 | LOCA-5, LOCA-44 | 3 | P0 |
| LOCA-64 | People browser: GET /venues/{id}/people — non-anonymous profiles | @backend-1 | LOCA-63, LOCA-23 | 3 | P0 |
| LOCA-65 | Anonymous mode: visible in count, hidden from people, "Guest_XXXX" in chat | @backend-1 | LOCA-64 | 2 | P0 |
| LOCA-66 | Push notification scaffold: FCM integration + device token registration endpoint | @backend-2 | LOCA-2 | 3 | P0 |
| LOCA-67 | Mobile: useSignalR hook (connect, reconnect backoff, state) — BEFORE chat screen | @mobile-2 | LOCA-61, LOCA-28 | 5 | P0 |
| LOCA-68 | Mobile: Social Hub tabs (Chat / People — Feed+Games added Sprint 5) | @mobile-1 | LOCA-51, LOCA-7 | 3 | P0 |
| LOCA-69 | Mobile: Public chat screen (messages, input, send, scroll, load-more) | @mobile-2 | LOCA-68, LOCA-67 | 8 | P0 |
| LOCA-70 | Mobile: Chat components (text bubble, image, GIF, reply, timestamp) | @mobile-2 | LOCA-69 | 3 | P0 |
| LOCA-71 | Mobile: People browser grid (avatar, name, interests, tap→profile) | @mobile-1 | LOCA-68, LOCA-64 | 3 | P0 |
| LOCA-72 | Mobile: Other user profile screen (bio, interests, past venues) | @mobile-1 | LOCA-71 | 3 | P0 |
| LOCA-73 | Mobile: expo-notifications setup (token registration, handler) | @mobile-1 | LOCA-66 | 3 | P0 |
| LOCA-74 | SignalR load test: 200 concurrent in one room | @qa-1 | LOCA-62 | 3 | P1 |
| LOCA-75 | Tests: send/receive, persistence, active users, anonymous | @qa-1 | LOCA-62 | 3 | P0 |
| LOCA-76 | Mobile tests: chat render, message send, people grid | @qa-2 | LOCA-70 | 3 | P1 |

---

### EPIC-5: Feed + Games
**Sprint 5 (Week 9-10) — "Play & Share" | 63 SP**

| ID | Task | Assignee | Depends On | SP | Pri |
|----|------|----------|-----------|-----|-----|
| LOCA-80 | Venue feed: Post entity, CRUD, like/comment, GET /venues/{id}/feed?cursor= | @backend-1 | LOCA-40 | 5 | P0 |
| LOCA-81 | Game Service: GameSession + GamePlayer entities + GameHub SignalR | @backend-2 | LOCA-60, LOCA-10 | 5 | P0 |
| LOCA-82 | Game lobby: create→params→join/leave→host starts (SignalR) | @backend-2 | LOCA-81 | 5 | P0 |
| LOCA-83 | Game engine: server-authoritative state machine + 60s reconnect grace | @backend-2 | LOCA-82 | 5 | P0 |
| LOCA-84 | Mafia: roles, night/day, voting, elimination, win detect | @backend-1 | LOCA-83 | 8 | P0 |
| LOCA-85 | Truth or Dare: 200 AZ questions, random pick, skip, custom | @backend-1 | LOCA-83 | 3 | P0 |
| LOCA-86 | Game result → auto-post to venue chat + leaderboard | @backend-2 | LOCA-84, LOCA-61 | 2 | P1 |
| LOCA-87 | Mobile: Feed screen (vertical scroll, like, comment) + post creation | @mobile-1 | LOCA-68, LOCA-80 | 5 | P0 |
| LOCA-88 | Mobile: Games tab + active games list + "Create Game" | @mobile-2 | LOCA-68, LOCA-82 | 3 | P0 |
| LOCA-89 | Mobile: Game lobby UI (avatars, ready, host controls, countdown) | @mobile-2 | LOCA-88 | 3 | P0 |
| LOCA-90 | Mobile: Mafia UI (role reveal, voting, night action, results) | @mobile-2 | LOCA-89, LOCA-84 | 5 | P0 |
| LOCA-91 | Mobile: Truth or Dare UI (card flip, timer, skip) | @mobile-1 | LOCA-89, LOCA-85 | 3 | P0 |
| LOCA-92 | Game tests: Mafia full sim (5-12 players), disconnect, timeout | @qa-1 | LOCA-84 | 5 | P0 |
| LOCA-93 | Mobile tests: feed, lobby, Mafia flow, T/D flow | @qa-2 | LOCA-90 | 3 | P1 |

---

### EPIC-6: Matching & Private Chat
**Sprint 6 (Week 11-12) — "Connect" | 56 SP**

| ID | Task | Assignee | Depends On | SP | Pri |
|----|------|----------|-----------|-----|-----|
| LOCA-100 | Match request: send→push→accept/decline→create conversation, same-venue, 5/day limit | @backend-1 | LOCA-23, LOCA-63, LOCA-66 | 5 | P0 |
| LOCA-101 | Spam protection: 3 declines = 48h block, non-anonymous only | @backend-1 | LOCA-100 | 3 | P0 |
| LOCA-102 | Private chat: Conversation entity + PrivateChatHub SignalR | @backend-2 | LOCA-61, LOCA-100 | 5 | P0 |
| LOCA-103 | Typing indicator + read receipts + online status (Redis) | @backend-2 | LOCA-102 | 3 | P1 |
| LOCA-104 | Report/Block: report(reason), block(hide globally) | @backend-1 | LOCA-23 | 3 | P0 |
| LOCA-105 | Mobile: Match Request button on profile + push handler | @mobile-1 | LOCA-72, LOCA-100, LOCA-73 | 3 | P0 |
| LOCA-106 | Mobile: Match inbox (pending/accepted/declined) | @mobile-1 | LOCA-105 | 3 | P0 |
| LOCA-107 | Mobile: Matches tab — conversations list (last msg, unread, online) | @mobile-2 | LOCA-106, LOCA-102 | 3 | P0 |
| LOCA-108 | Mobile: Private chat (reuse components + typing + read receipts) | @mobile-2 | LOCA-70, LOCA-103 | 5 | P0 |
| LOCA-109 | Mobile: Report/Block flow (reason sheet, confirm, toast) | @mobile-1 | LOCA-104 | 2 | P0 |
| LOCA-110 | Tests: match lifecycle, daily limit, spam block, report/block isolation | @qa-1 | LOCA-101, LOCA-104 | 5 | P0 |
| LOCA-111 | Mobile tests: match flow, private chat, block hides user | @qa-2 | LOCA-108 | 3 | P1 |
| LOCA-112 | E2E: profile→match→accept→private chat→send message | @qa-2 | LOCA-108 | 3 | P1 |

---

### EPIC-7: Gifting & Economy
**Sprint 7 (Week 13-14) — "Show Love" | 60 SP**

| ID | Task | Assignee | Depends On | SP | Pri |
|----|------|----------|-----------|-----|-----|
| LOCA-120 | Economy Service: Wallet, Transaction (double-entry), GiftCatalog entities | @backend-1 | LOCA-4 | 5 | P0 |
| LOCA-121 | IAP validation: App Store Server API v2 + Google Play Developer API | @backend-1 | LOCA-120 | 8 | P0 |
| LOCA-122 | Gift catalog: 20 gifts, 4 tiers, coin prices, animation URLs | @backend-1 | LOCA-120 | 2 | P0 |
| LOCA-123 | Send gift: validate balance→deduct→transaction→SignalR broadcast | @backend-1 | LOCA-122, LOCA-61 | 5 | P0 |
| LOCA-124 | Gift in private chat + gift leaderboard (Redis) | @backend-2 | LOCA-123, LOCA-102 | 3 | P1 |
| LOCA-125 | Mobile: Coin shop (4 IAP packages via expo-in-app-purchases) | @mobile-1 | LOCA-121, LOCA-28 | 5 | P0 |
| LOCA-126 | Mobile: Gift picker bottom sheet (grid, cost, balance, send) | @mobile-1 | LOCA-125, LOCA-122 | 3 | P0 |
| LOCA-127 | Mobile: Gift button in chat input + send in public/private chat | @mobile-2 | LOCA-126, LOCA-123 | 3 | P0 |
| LOCA-128 | Mobile: Lottie animation system — 20 full-screen gift animations | @mobile-2 | LOCA-127 | 8 | P0 |
| LOCA-129 | IAP setup doc: App Store Connect + Google Play product IDs | @architect | LOCA-121 | 2 | P0 |
| LOCA-130 | Tests: purchase (valid/invalid), gift send, balance, concurrent safety | @qa-1 | LOCA-123 | 5 | P0 |
| LOCA-131 | Mobile tests: coin shop, gift picker, animation, balance | @qa-2 | LOCA-128 | 3 | P1 |
| LOCA-132 | E2E: buy coins→gift picker→send→animation→balance updated | @qa-2 | LOCA-128 | 3 | P1 |

---

### EPIC-8: Launch
**Sprint 8 (Week 15-16) — "Ship It" | 58 SP**

| ID | Task | Assignee | Depends On | SP | Pri |
|----|------|----------|-----------|-----|-----|
| LOCA-140 | Production: Hetzner VPS + Docker + Nginx + SSL | @architect | LOCA-3 | 5 | P0 |
| LOCA-141 | Prod DB: PostgreSQL backup (pg_dump→MinIO daily) + Redis persistence | @backend-1 | LOCA-140 | 3 | P0 |
| LOCA-142 | DNS: api.loca.az, admin.loca.az + Nginx + SSL certs | @architect | LOCA-140 | 2 | P0 |
| LOCA-143 | Staging env: separate Docker stack, staging DB + seed data | @backend-1 | LOCA-140 | 3 | P0 |
| LOCA-144 | EAS Build: dev+prod profiles, iOS+Android, code signing | @mobile-1 | LOCA-6 | 5 | P0 |
| LOCA-145 | App Store: screenshots, AZ/EN desc, privacy policy, 17+ | @mobile-1 | LOCA-144 | 3 | P0 |
| LOCA-146 | Google Play: screenshots, desc, content rating, data safety | @mobile-2 | LOCA-144 | 3 | P0 |
| LOCA-147 | Privacy Policy + ToS (AZ+EN, GDPR, 18+) | @architect | — | 3 | P0 |
| LOCA-148 | Admin web MVP: venue CRUD + QR display + check-in stats | @mobile-2 | LOCA-40 | 8 | P1 |
| LOCA-149 | PostHog analytics: user events + funnels | @backend-2 | LOCA-140 | 3 | P1 |
| LOCA-150 | Performance: API <200ms p95, startup <3s, SignalR <500ms | @backend-2 | ALL | 3 | P0 |
| LOCA-151 | Security audit: OWASP top 10, JWT, rate limits, validation | @qa-1 | ALL | 5 | P0 |
| LOCA-152 | Full regression: every endpoint + screen on staging | @qa-1 | LOCA-143 | 5 | P0 |
| LOCA-153 | Mobile E2E suite: auth→discover→checkin→chat→game→match→gift | @qa-2 | LOCA-143 | 5 | P0 |
| LOCA-154 | Bug fix buffer | ALL | LOCA-152 | 5 | P0 |
| LOCA-155 | HUMAN-ONLY: Visit 10+ Baku venues, install QR, train staff | — | — | — | P0 |

---

## Phase 2-3 Sprints (Week 17-28)

### Sprint 9 (Week 17-18): Vibe AI Radar | 55 SP
LOCA-200→209: AI Service + vibe scoring + matching + mobile Vibe Radar card

### Sprint 10 (Week 19-20): Vibe Bomb + Uno/Domino | 52 SP
LOCA-220→231: Vibe Bomb mechanic + Uno game + Domino game

### Sprint 11-12 (Week 21-24): AR Ghost Mode | 48 SP
LOCA-300→307: Ghost entity + expo-dev-client + AR rendering + ghost interaction

### Sprint 13 (Week 25-26): DJ + Playlist | 42 SP
LOCA-320→326: Spotify API + song request + DJ panel + mobile Music tab

### Sprint 14 (Week 27-28): Chain Party + Time Capsule | 44 SP
LOCA-340→364: Cross-venue chain + completion rewards + time capsule + scheduled unlock

---

## Capacity Summary

| Sprint | Weeks | SP | ≤65? |
|--------|-------|-----|------|
| S1 Foundation | 1-2 | 48 | ✅ |
| S2 Auth | 3-4 | 58 | ✅ |
| S3 Venue | 5-6 | 62 | ✅ |
| S4 Chat+People | 7-8 | 61 | ✅ |
| S5 Feed+Games | 9-10 | 63 | ✅ |
| S6 Matching | 11-12 | 56 | ✅ |
| S7 Gifting | 13-14 | 60 | ✅ |
| S8 Launch | 15-16 | 58 | ✅ |
| S9 AI | 17-18 | 55 | ✅ |
| S10 Bomb+Games | 19-20 | 52 | ✅ |
| S11-12 AR | 21-24 | 48 | ✅ |
| S13 DJ | 25-26 | 42 | ✅ |
| S14 Chain+Capsule | 27-28 | 44 | ✅ |

## Critical Path
```
LOCA-1 → 2 → 9 → 31 (API client gen = mobile can call backend)
LOCA-1 → 3 → 4 → 40 → 43 → 44 → 48 → 51 → 68 (check-in → social hub)
LOCA-10 → 61 → 67 (useSignalR) → 69 (chat screen)
LOCA-83 → 84 → 90 (game engine → Mafia UI)
LOCA-120 → 121 → 123 → 127 → 128 (economy → animations)
```
