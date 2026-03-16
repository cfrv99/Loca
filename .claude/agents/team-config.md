# Loca Agent Team Configuration

## Team Spawn Template
When using Claude Code Agent Teams, use this configuration to spawn the Loca development team:

```
Create an agent team "loca-dev" with these teammates:

1. **architect** — Software Architect
   - Focus: System design, API contracts, database schema, tech decisions, ADRs
   - Files: docs/architecture/*, services/*/Domain/*, infrastructure/*
   - Rules: Read .claude/rules/backend.md, always check EPIC-INDEX.md for context

2. **backend-1** — Senior Backend Developer (Identity, Venue, Economy)
   - Focus: Identity Service, Venue Service, Economy Service, Matching Service
   - Files: services/identity/*, services/venue/*, services/economy/*, services/matching/*
   - Rules: Read .claude/rules/backend.md, use context7 for EF Core + SignalR docs

3. **backend-2** — Senior Backend Developer (Social, Game, Notification, AI)
   - Focus: Social Service, Game Service, Notification Service, AI Service
   - Files: services/social/*, services/game/*, services/notification/*, services/ai/*
   - Rules: Read .claude/rules/backend.md, SignalR hubs are your primary domain

4. **mobile-1** — Senior Mobile Developer (Core + Navigation + Venue)
   - Focus: App shell, navigation, auth screens, venue discovery, check-in, gifting shop
   - Files: apps/mobile/app/*, apps/mobile/features/auth/*, apps/mobile/features/venue/*, apps/mobile/features/gifting/*
   - Rules: Read .claude/rules/mobile.md, use context7 for Expo + React Navigation docs

5. **mobile-2** — Senior Mobile Developer (Social + Games + Chat)
   - Focus: Chat screens, game UIs, matching UI, SignalR integration, animations
   - Files: apps/mobile/features/chat/*, apps/mobile/features/games/*, apps/mobile/features/matching/*
   - Rules: Read .claude/rules/mobile.md, Lottie animations, SignalR client hooks

6. **qa-1** — QA Engineer (Backend)
   - Focus: Unit tests, integration tests, API contract tests, load tests
   - Files: tests/*, services/*/Tests/*
   - Rules: Read .claude/rules/testing.md, every feature needs tests before PR

7. **qa-2** — QA Engineer (Mobile + E2E)
   - Focus: Mobile unit tests, Detox E2E, Playwright admin web tests
   - Files: apps/mobile/__tests__/*, tests/e2e/*
   - Rules: Read .claude/rules/testing.md, test critical user flows end-to-end
```

## Subagent Patterns (Single Session)

### Research subagent
```
Task({
  subagent_type: "research",
  prompt: "Research best practices for [topic]. use context7 for [library] docs. Return a summary with code examples."
})
```

### Code review subagent
```
Task({
  subagent_type: "review",
  prompt: "Review [file/directory] following .claude/rules/backend.md. Check for security, performance, conventions."
})
```

### Test writing subagent
```
Task({
  subagent_type: "testing",
  prompt: "Write tests for [feature] following .claude/rules/testing.md. Cover happy path + 3 edge cases."
})
```

## Recommended Team Configurations by Sprint

### Sprint 1 (Foundation)
- Lead: @architect
- Parallel: @backend-1 (DB + Redis), @mobile-1 (Expo init), @backend-2 (CI/CD)

### Sprint 4 (Social Hub)
- Lead: @architect
- Parallel: @backend-2 (SignalR chat), @mobile-2 (chat UI), @backend-1 (feed + people), @mobile-1 (hub shell)
- Coordination needed: SignalR contract between @backend-2 and @mobile-2

### Sprint 5 (Games)
- Lead: @backend-2 (game engine)
- Parallel: @mobile-2 (game UIs), @qa-1 (game state tests)
- Sequential: Game logic first → then UI → then tests

### Sprint 7 (Gifting)
- Lead: @backend-1 (economy)
- Parallel: @mobile-1 (coin shop), @mobile-2 (animations), @designer (Lottie assets)
- Critical path: IAP setup requires App Store/Google Play developer accounts ready
