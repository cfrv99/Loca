# LOCA — Location-Based Social Discovery Platform

## Project Overview
Loca transforms physical venues (restaurants, bars, beach clubs, cafes) into live social hubs via QR + geofence verification. Users check in, join public chat, play interactive games, match with people, send gifts, and discover venues by real-time activity.

**Target Market:** Baku, Azerbaijan (first 6 months) → Turkey expansion
**Tech Stack:** ASP.NET Core 8 (Backend) + React Native Expo (Mobile) + PostgreSQL + Redis + SignalR
**Architecture:** Clean Architecture + CQRS + Microservices (domain-separated)

## Monorepo Structure
```
loca/
├── apps/
│   ├── mobile/          # React Native Expo app (iOS + Android)
│   ├── api/             # ASP.NET Core 8 Web API (main backend)
│   ├── admin-web/       # React admin panel for venue owners (Vite + React)
│   └── venue-tablet/    # QR display tablet app (React PWA)
├── packages/
│   ├── shared-types/    # TypeScript shared types/interfaces
│   ├── ui-kit/          # Shared React Native UI components
│   └── game-engine/     # Game logic (shared between mobile & backend)
├── services/
│   ├── identity/        # Auth microservice
│   ├── venue/           # Venue + QR + geofence
│   ├── social/          # Chat + feed + matching
│   ├── game/            # Game sessions + state
│   ├── economy/         # Coins + gifts + IAP
│   ├── ai/              # Vibe AI + recommendation (Python FastAPI)
│   ├── notification/    # Push + in-app notifications
│   └── analytics/       # Event tracking + B2B dashboards
├── infrastructure/
│   ├── docker/          # Docker configs per service
│   ├── k8s/             # Kubernetes manifests (future)
│   ├── terraform/       # AWS/Hetzner infrastructure
│   └── ci-cd/           # GitHub Actions workflows
├── docs/                # Architecture decisions, API specs, PRD
├── .claude/             # Claude Code configuration
└── tests/
    ├── integration/     # Cross-service integration tests
    ├── e2e/             # Detox (mobile) + Playwright (web) E2E tests
    └── load/            # k6 load testing scripts
```

## Build & Run Commands

### Backend (ASP.NET Core)
```bash
cd apps/api && dotnet restore && dotnet build
dotnet run --project apps/api  # Starts on https://localhost:5001
dotnet test tests/              # Run all tests
dotnet ef migrations add <Name> --project services/venue  # EF migration
```

### Mobile (React Native Expo)
```bash
cd apps/mobile && npm install
npx expo start                  # Dev server with QR for Expo Go
npx expo start --ios            # iOS simulator
npx expo start --android        # Android emulator
npx expo run:ios --device       # Real device build
npx eas build --platform ios    # Production build via EAS
```

### Admin Web
```bash
cd apps/admin-web && npm install && npm run dev  # Vite dev server
```

### Docker (Full Stack)
```bash
docker compose up -d            # All services + Postgres + Redis
docker compose logs -f api      # Follow API logs
```

## Code Conventions

### Backend (.NET)
- Clean Architecture layers: Domain → Application → Infrastructure → API
- CQRS: Commands/Queries via MediatR
- Use FluentValidation for all request validation
- Entity Framework Core with PostgreSQL + PostGIS
- All endpoints return `ApiResponse<T>` wrapper
- SignalR hubs in `/Hubs` folder, one per domain
- Use `Result<T>` pattern, never throw exceptions for business logic
- Naming: PascalCase for public, _camelCase for private fields

### Mobile (React Native Expo + TypeScript)
- Functional components only, no class components
- State: Zustand for global, React Query for server state
- Navigation: React Navigation v7 (native stack)
- Styling: NativeWind (Tailwind for RN)
- File naming: kebab-case for files, PascalCase for components
- Folder structure: feature-based (`/features/venue/`, `/features/chat/`)
- All API calls through generated TypeScript client (openapi-typescript-codegen)
- Use Expo modules where available (expo-camera, expo-location, expo-notifications)

### Testing
- Backend: xUnit + FluentAssertions + Testcontainers (integration)
- Mobile: Jest + React Native Testing Library
- E2E: Detox (mobile) + Playwright (admin web)
- Coverage target: 80% for services, 60% for UI

### Git Workflow
- Branch naming: `feature/LOCA-{ticket}-{short-desc}`, `fix/LOCA-{ticket}-{short-desc}`
- Commit format: `feat(venue): add QR rotation logic [LOCA-42]`
- PR must pass: build + lint + tests + 1 approval
- Main branch is protected, deploy from `release/*` branches

## Team Roles (Agent Definitions in .claude/agents/)
Each agent has a dedicated definition file with role, responsibilities, workflow, and review checklist:

- **@architect** → `.claude/agents/architect.md` — System design, API contracts, ADRs, cross-service reviews
- **@backend-1** → `.claude/agents/backend-1.md` — Identity, Venue, Economy, Matching services
- **@backend-2** → `.claude/agents/backend-2.md` — Social, Game, Notification, AI services (SignalR expert)
- **@mobile-1** → `.claude/agents/mobile-1.md` — Auth, venue discovery, check-in, gifting (Figma-to-code lead)
- **@mobile-2** → `.claude/agents/mobile-2.md` — Chat, games, matching UI, Lottie animations (SignalR client expert)
- **@qa-1** → `.claude/agents/qa-1.md` — Backend QA gatekeeper (code review + integration tests + load tests)
- **@qa-2** → `.claude/agents/qa-2.md` — Mobile QA gatekeeper (UI review + E2E + accessibility + design compliance)

## Task Board Workflow
All agents use `docs/taskboard/BOARD.md` as the single source of truth:
- `/pickup @role` — Pick next task from board
- `/handoff LOCA-{id}` — Hand off to QA after implementing
- `/verify` — Self-test everything before handoff
- QA reviews → APPROVED → DONE, or REJECTED → back to IN_PROGRESS

## Team Communication Protocol
See `.claude/rules/team-protocol.md` for full handoff rules:
- Backend→Mobile: publish API contract first, then mobile builds UI
- Developer→QA: handoff note with what was built, how to test, acceptance criteria
- Blocked tasks: notify blocker immediately, blocker prioritizes unblocking
- Figma→Code: use Figma MCP to extract design, then generate pixel-perfect RN components

## 🔌 MCP ENFORCEMENT — MUST USE AT EVERY STEP
Claude Code has 7 MCP servers. Using them is NOT optional.

### BEFORE writing ANY code:
```
"use context7 for {library name} {specific feature}"
```
This fetches current docs. Without this, you WILL use deprecated APIs.

### BEFORE any architecture decision:
```
Use sequential-thinking MCP to plan: {description}
```

### AFTER every sprint:
```
Save to Memory MCP: sprint-{N} decisions, patterns, APIs, issues
```

### AFTER DB changes:
```
Use postgres MCP: verify data with SELECT queries
```

### For mobile UI with Figma:
```
Use figma MCP to get design context for: {frame link}
```

### For admin web testing:
```
Use playwright MCP to E2E test: {url}
```

## Important Constraints
- NEVER use localStorage/sessionStorage in any web artifacts
- All dates in UTC, display in user's timezone
- All monetary amounts in qəpik (AZN cents) internally, display as AZN
- Geofence radius: configurable per venue (default 150m)
- QR codes rotate every 60 seconds (venue tablet generates, backend validates)
- Max concurrent SignalR connections per venue: 500
- Rate limits: 100 req/min per user (API), 30 msg/min per user (chat)
- GDPR-compliant: user data deletion within 30 days of request
- Minimum age: 18+ (verified at registration)

## Quality Enforcement
Claude Code is configured with automatic quality gates:

1. **Stop Hook**: Before stopping, a subagent auto-verifies build, types, and tests pass
2. **PreToolUse Hooks**: Blocks commits with wrong format, blocks writes with hardcoded secrets
3. **PostToolUse Hooks**: Auto-runs `dotnet build` after .cs edits, `tsc --noEmit` after .ts edits
4. **Skills**: Use `/feature`, then Claude reads the relevant skill (create-endpoint, create-screen, create-game) for step-by-step quality pattern
5. **Definition of Done**: `.claude/rules/definition-of-done.md` is auto-loaded — every task must pass ALL checklist items
6. **Anti-Patterns**: `.claude/rules/anti-patterns.md` lists 30+ things to NEVER do
7. **Golden Examples**: `docs/golden-examples/` contains reference implementations to match

## 🚀 HOW TO BUILD THIS APP (ONE COMMAND)

```bash
claude --dangerously-skip-permissions
> /build-loca
```

This spawns the full Agent Team, orchestrates 8 sprints, and builds the entire app autonomously.
You do NOT need to intervene. Go drink çay. Come back to a finished app.

### What happens:
1. Project Director initializes monorepo + Docker + Git
2. Spawns 7 agents: architect, backend-1, backend-2, mobile-1, mobile-2, qa-1, qa-2
3. Agents work through Sprint 1→8, communicating via task board + messages
4. Each sprint: code → auto-test → QA review → commit → next sprint
5. Hooks auto-verify: build passes, no secrets leaked, naming conventions followed
6. At end: full app with tests, ready for `eas build` and App Store submission

### Permissions: FULL AUTONOMY
- `Bash(*)` — all shell commands allowed
- `Write(*)`, `Edit(*)`, `Read(*)` — all file operations allowed
- `Agent(*)` — agent spawning allowed
- `mcp__*` — all MCP tools allowed
- Hooks still enforce: security scan, build check, test check
- Only denied: `rm -rf /`, `rm -rf ~`, `sudo rm -rf *`

@docs/prd/feature-specs.md
@docs/architecture/system-overview.md
@docs/architecture/api-specification.md
@docs/architecture/signalr-contracts.md
@docs/architecture/database-schema.md
@docs/epics/EPIC-INDEX.md
@docs/content/azerbaijani-content.md
@docs/golden-examples/backend/checkin-endpoint.md
@docs/golden-examples/backend/signalr-hub.md
@docs/golden-examples/mobile/discover-screen.md
@docs/golden-examples/mobile/chat-screen.md
