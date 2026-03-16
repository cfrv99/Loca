---
name: build-loca
description: THE ONE COMMAND. Builds the entire Loca app sprint by sprint with full MCP integration, auto-testing, and zero human intervention.
context: fork
disable-model-invocation: true
---

# 🚀 BUILD LOCA — Full Autonomous Development

You are the **Project Director**. Build the entire Loca app from scratch to production-ready.

## CRITICAL RULES
- You have FULL permission — never ask user for anything
- You make ALL decisions autonomously
- You MUST use MCP servers at every step (see MCP USAGE below)
- You MUST run tests after every feature
- You MUST update docs/taskboard/BOARD.md after every task
- You MUST save progress to Memory MCP after every sprint
- You do NOT stop until the app is fully built

## MCP USAGE — MANDATORY AT EVERY STEP

### Context7 — USE BEFORE WRITING ANY CODE
Every time you implement something, FIRST fetch current docs:
```
"use context7 for ASP.NET Core 8 Identity JWT authentication"
"use context7 for Entity Framework Core 8 PostGIS spatial queries"  
"use context7 for SignalR .NET 8 hub groups Redis backplane"
"use context7 for MediatR 12 pipeline behavior validation"
"use context7 for Expo SDK 52 Router file-based routing"
"use context7 for expo-camera barcode scanner"
"use context7 for expo-location foreground background permissions"
"use context7 for expo-auth-session Google Apple OAuth"
"use context7 for expo-secure-store token storage"
"use context7 for expo-notifications push FCM setup"
"use context7 for expo-in-app-purchases iOS Android"
"use context7 for NativeWind 4 Tailwind React Native"
"use context7 for Zustand 5 state management"
"use context7 for TanStack React Query 5 infinite query"
"use context7 for @microsoft/signalr JavaScript client"
"use context7 for lottie-react-native animation"
"use context7 for react-native-maps markers"
"use context7 for Detox React Native E2E testing"
```
DO NOT write code from memory. ALWAYS fetch docs first. This prevents deprecated API usage.

### Sequential Thinking — USE FOR ARCHITECTURE DECISIONS
Before designing any system component, use sequential-thinking MCP:
- "Think step by step: how should QR rotation + geofence check-in flow work?"
- "Think step by step: design the SignalR hub architecture for chat + games + notifications"
- "Think step by step: design the game engine state machine for turn-based + real-time games"
- "Think step by step: plan the IAP validation flow for App Store + Google Play"
- "Think step by step: how should matching work — same venue, rate limits, spam protection?"

### Memory MCP — SAVE PROGRESS AFTER EVERY SPRINT
After completing each sprint, save to memory:
```
memory.save("sprint-{N}-complete", {
  tasks_done: [...],
  architecture_decisions: [...],
  patterns_established: [...],
  api_contracts: [...],
  known_issues: [...],
  next_sprint_prereqs: [...]
})
```
Before starting each new sprint, LOAD previous sprint memory:
```
memory.load("sprint-{N-1}-complete")
```
This prevents context loss across compaction/sessions.

### Figma MCP — USE FOR MOBILE UI (when Figma links available)
If any task has a Figma link, use figma MCP to extract design context:
```
"Use figma MCP to get design context for this frame: {link}
Generate React Native component with NativeWind matching the design exactly."
```
If no Figma link: follow .claude/rules/design-system.md tokens exactly.

### PostgreSQL MCP — USE TO VERIFY DATABASE
After creating migrations/seed data:
```
"Use postgres MCP to verify: SELECT count(*) FROM venue.venues"
"Use postgres MCP to verify: SELECT ST_AsText(location) FROM venue.venues LIMIT 1"
"Use postgres MCP to verify: SELECT * FROM identity.users LIMIT 5"
```

### Playwright MCP — USE FOR ADMIN WEB E2E (Sprint 8)
When building admin web, use playwright MCP to test:
```
"Use playwright to navigate to http://localhost:3000, verify venue list renders, add a venue, check QR displays"
```

---

## EXECUTION — SPRINT BY SPRINT

### PHASE 0: READ ALL DOCS + SETUP

**Step 0.1 — Read ALL documentation FIRST (before writing any code):**
1. `docs/prd/feature-specs.md` — COMPLETE feature specification (what to build)
2. `docs/architecture/api-specification.md` — ALL REST endpoints with request/response
3. `docs/architecture/signalr-contracts.md` — ALL real-time hub methods + payloads
4. `docs/architecture/database-schema.md` — ALL database tables (30+ tables)
5. `docs/architecture/system-overview.md` — Service architecture + boundaries
6. `docs/content/azerbaijani-content.md` — UI strings, game content, notification templates
7. `docs/golden-examples/` — 4 reference implementations to match patterns

**Step 0.2 — Scaffold monorepo:**
```bash
mkdir -p apps/{api,mobile,admin-web,venue-tablet}
mkdir -p packages/{domain,application,infrastructure}
mkdir -p services/{identity,venue,social,game,economy,notification}
mkdir -p infrastructure/{docker,ci-cd}
mkdir -p tests/{unit,integration,e2e,load}
git init && git checkout -b main
```

### FOR EACH SPRINT (1 through 8):

**Step 1: Load Context**
- Read `docs/epics/EPIC-INDEX.md` for this sprint's tasks
- Read `docs/prd/feature-specs.md` sections relevant to this sprint
- Load memory from previous sprint: `memory.load("sprint-{N-1}-complete")`
- Update `docs/taskboard/BOARD.md` with this sprint's TODO items

**Step 2: Architecture First (if needed)**
- Use `sequential-thinking` MCP to plan the sprint's architecture
- Read `docs/architecture/api-specification.md` for endpoint contracts — implement EXACTLY as specified
- Read `docs/architecture/signalr-contracts.md` if sprint has real-time features
- @architect tasks first: verify DB schema matches `docs/architecture/database-schema.md`

**Step 3: Backend Implementation**
For each backend task:
1. Read `docs/prd/feature-specs.md` for the feature's business rules and edge cases
2. Read `docs/architecture/api-specification.md` for the exact endpoint contract
3. Use `context7` to fetch latest library docs for the feature
4. If SignalR feature: read `docs/architecture/signalr-contracts.md` + use skill `create-signalr-hub`
5. If game feature: read `.claude/skills/create-game/SKILL.md` for game-specific rules
6. For REST endpoints: read `.claude/skills/create-endpoint/SKILL.md` — follow step by step
7. Match golden example: `docs/golden-examples/backend/checkin-endpoint.md` (REST) or `signalr-hub.md` (real-time)
8. Implement: Domain entity → Validator → Handler → Controller → EF Config
9. ALL user-facing strings in Azerbaijani (read `docs/content/azerbaijani-content.md`)
10. Run `dotnet build` — must pass
11. Write tests (minimum 3 per feature)
12. Run `dotnet test` — must pass
13. Use `postgres` MCP to verify data in DB
14. Update taskboard: `[IN_PROGRESS]` → `[IN_REVIEW]`

**Step 4: Mobile Implementation**
For each mobile task:
1. Read `docs/prd/feature-specs.md` for exact UI behavior, states, rules
2. Use `context7` to fetch latest Expo/RN library docs
3. Read `.claude/skills/create-screen/SKILL.md` — follow step by step
4. If chat/real-time: match `docs/golden-examples/mobile/chat-screen.md` pattern
5. If discovery/list: match `docs/golden-examples/mobile/discover-screen.md` pattern
6. If Figma link: use `figma` MCP to extract design
7. ALL UI strings in Azerbaijani (from `docs/content/azerbaijani-content.md`)
8. Implement: Screen → Hooks → Components (loading/error/empty states MANDATORY)
9. Run `npx tsc --noEmit` — must pass
10. Write tests (minimum 2 per screen)
11. Run `npx jest` — must pass
12. Update taskboard

**Step 5: QA Review**
For each `[IN_REVIEW]` task:
1. Read `.claude/skills/qa-review/SKILL.md`
2. Run full QA checklist from `.claude/agents/qa-1.md` or `qa-2.md`
3. If PASS → `[DONE]`
4. If FAIL → fix issues, re-test, then `[DONE]`

**Step 6: Sprint Completion**
1. Run `/verify` — all checks must pass
2. Git commit: `feat(sprint-{N}): complete sprint {N} — {summary} [LOCA-{range}]`
3. Save to Memory MCP: all decisions, patterns, APIs, known issues
4. Update taskboard: move all to DONE, add Sprint {N+1} TODOs
5. Report to user:
```
Sprint {N} COMPLETE ✅
- Tasks: {count} done
- Tests: {pass}/{total}  
- MCP Usage: context7 ({count} calls), sequential-thinking ({count}), memory (saved)
- Known issues: {list or "none"}
- Starting Sprint {N+1}...
```

---

## SPRINT-SPECIFIC MCP USAGE

### Sprint 1 (Foundation)
- context7: ASP.NET Core 8 project setup, EF Core 8 PostGIS, Expo SDK 52, NativeWind 4
- sequential-thinking: monorepo structure, Clean Architecture layers, SignalR scaling

### Sprint 2 (Auth)
- context7: ASP.NET Identity, JWT Bearer, Google.Apis.Auth, Apple Sign-In, expo-auth-session, expo-secure-store
- sequential-thinking: token refresh rotation strategy, OAuth flow design

### Sprint 3 (Venue + QR)
- context7: PostGIS ST_DWithin, TOTP algorithm, expo-camera barcode, react-native-maps, expo-location
- sequential-thinking: QR rotation + geofence verification flow, anti-spoofing strategy
- postgres: verify venue seed data, test spatial queries

### Sprint 4 (Chat + People)
- context7: SignalR hubs + groups + Redis backplane, @microsoft/signalr JS client, expo-notifications
- sequential-thinking: chat architecture (persistence + cache + real-time), anonymous mode design
- postgres: verify message persistence

### Sprint 5 (Feed + Games)
- context7: SignalR for game state, Reanimated 3 animations
- sequential-thinking: game engine state machine design, Mafia phase transitions, fog of war

### Sprint 6 (Matching)
- context7: SignalR private messaging, push notifications
- sequential-thinking: match request lifecycle, spam protection, rate limiting

### Sprint 7 (Gifting)
- context7: expo-in-app-purchases, lottie-react-native, App Store Server API v2
- sequential-thinking: IAP receipt validation flow, double-entry ledger, concurrent safety
- postgres: verify transaction integrity

### Sprint 8 (Launch)
- context7: EAS Build, Nginx config, Let's Encrypt
- playwright: admin web E2E testing
- sequential-thinking: production architecture, scaling plan, security audit
- postgres: production DB verification

---

## DECISION AUTHORITY
- Make ALL technical decisions. If unsure → simpler option.
- If blocked → reorder to maximize parallel work.
- If library doesn't work → find alternative, don't ask.
- If tests fail → fix them, don't skip.
- ALWAYS use context7 before coding. ALWAYS.
- Commit after EVERY task.

BEGIN NOW. Start with Phase 0, then Sprint 1.
