# Testing Strategy

## Backend Testing
- **Unit tests** (xUnit + FluentAssertions): Domain logic, validators, handlers
- **Integration tests** (Testcontainers): API endpoints with real PostgreSQL + Redis
- **Contract tests**: Ensure API responses match OpenAPI spec
- Run: `dotnet test` from repo root (discovers all test projects)

## Mobile Testing
- **Unit tests** (Jest + RNTL): Component rendering, hooks, store logic
- **Snapshot tests**: Key screens for regression detection
- **E2E** (Detox): Critical user flows (login → discover → check-in → chat → match)
- Run: `cd apps/mobile && npm test` (unit), `npm run e2e` (Detox)

## Admin Web Testing
- **E2E** (Playwright): Venue onboarding, analytics dashboard, event creation
- Run: `cd apps/admin-web && npx playwright test`

## Load Testing
- **k6**: SignalR concurrent connections, API endpoint throughput
- Targets: 500 concurrent users per venue, 100 req/s API, <200ms p95 latency
- Run: `k6 run tests/load/venue-checkin.js`

## QA Process
1. Developer writes unit + integration tests with the feature
2. PR triggers CI: build → lint → test → coverage report
3. QA creates E2E test for the user story acceptance criteria
4. QA performs manual exploratory testing on staging
5. Load testing before each phase release

## Test Naming Convention
- `Should_{ExpectedBehavior}_When_{Condition}`
- Example: `Should_ReturnUnauthorized_When_TokenExpired`
