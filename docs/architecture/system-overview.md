# Loca System Architecture Overview

## C4 Level 1: System Context
```
[Mobile App] → [API Gateway] → [Microservices]
[Admin Web]  → [API Gateway] → [Microservices]
[Venue Tablet] → [API Gateway] → [Venue Service]

Microservices:
├── Identity Service (ASP.NET Core)
├── Venue Service (ASP.NET Core + PostGIS)
├── Social Service (ASP.NET Core + SignalR)
├── Game Service (ASP.NET Core + SignalR)
├── Economy Service (ASP.NET Core)
├── AI Service (Python FastAPI) — Phase 2
├── Notification Service (ASP.NET Core)
└── Analytics Service (ASP.NET Core)

Data Stores:
├── PostgreSQL 16 + PostGIS (primary)
├── Redis 7 (cache, real-time counters, SignalR backplane, leaderboards)
├── S3/MinIO (media files)
└── RabbitMQ (async messaging between services) — Phase 2+
```

## C4 Level 2: Key Service Boundaries

### Identity Service
- Owns: Users, Profiles, OAuth tokens, Sessions
- Exposes: JWT token validation middleware (shared by all services)
- DB Schema: `identity.*` (users, user_profiles, user_interests, refresh_tokens)

### Venue Service
- Owns: Venues, QR codes, Check-ins, Geofences
- Exposes: Venue CRUD, check-in validation, real-time counters
- DB Schema: `venue.*` (venues, qr_codes, checkins, venue_settings)
- Special: PostGIS spatial queries for nearby search + geofence validation

### Social Service
- Owns: Chat rooms, Messages, Feed posts, Comments, Likes
- Exposes: VenueChatHub (SignalR), Feed CRUD, People browser
- DB Schema: `social.*` (chat_rooms, messages, posts, comments, likes)
- Special: SignalR hub with Redis backplane for horizontal scaling

### Game Service
- Owns: Game types, Game sessions, Player states, Leaderboards
- Exposes: GameHub (SignalR), Game CRUD
- DB Schema: `game.*` (game_types, game_sessions, game_players, game_states)
- Special: Server-authoritative state machine, turn-based + real-time support

### Economy Service
- Owns: Wallets, Transactions, Gifts, IAP receipts
- Exposes: Coin purchase, Gift send, Balance check
- DB Schema: `economy.*` (wallets, transactions, gifts, gift_catalog, iap_receipts)
- Special: Atomic transactions, double-entry ledger for audit trail

### Matching Service (part of Social Service initially)
- Owns: Match requests, Conversations, Vibe Bombs
- Exposes: Match request flow, Private chat
- DB Schema: `matching.*` (match_requests, conversations, vibe_bombs)

### AI Service (Phase 2)
- Owns: Vibe scores, User embeddings, Match predictions
- Exposes: REST API consumed by Venue + Matching services
- Stack: Python FastAPI, scikit-learn → PyTorch
- DB: Reads from PostgreSQL, writes to Redis (cached scores)

### Notification Service
- Owns: Push tokens, Notification templates, Delivery logs
- Exposes: Send notification endpoint (internal only)
- Integrations: Firebase Cloud Messaging (Android), APNs (iOS)

## Communication Patterns
- **Synchronous**: HTTP REST between services (via API Gateway)
- **Real-time**: SignalR WebSocket (client ↔ Social/Game services)
- **Async** (Phase 2+): RabbitMQ for event-driven (e.g., "user checked in" → update AI model)

## Scaling Strategy
- Phase 1: Single server (Hetzner CPX41 or AWS t3.large) + Docker Compose
- Phase 2: Separate Social/Game service for horizontal scaling (SignalR is the bottleneck)
- Phase 3: ECS Fargate or k8s for auto-scaling, Redis Cluster, read replicas for PostgreSQL
