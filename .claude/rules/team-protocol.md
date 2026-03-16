# Team Communication & Handoff Protocol

## Task Lifecycle
```
TODO → [developer picks up] → IN_PROGRESS → [developer finishes] → IN_REVIEW → [QA reviews]
  → APPROVED → DONE
  → REJECTED → IN_PROGRESS (developer fixes, re-submits)
```

## Handoff Rules

### Backend → Mobile Handoff
When @backend-1 or @backend-2 finishes an API endpoint:
1. Update OpenAPI spec (Swagger auto-generates)
2. Message mobile dev: "API ready: `POST /api/v1/venues/checkin` — see Swagger for contract"
3. Mobile dev runs openapi-typescript-codegen to generate client types

### Backend → Backend Handoff  
When @backend-2 needs data from @backend-1's service:
1. Define interface contract first (don't depend on implementation)
2. Message: "I need `IVenueRepository.GetActiveUsersAsync(venueId)` — can you expose it?"
3. @backend-1 implements + tests + notifies when ready

### Mobile ↔ Mobile Handoff
When @mobile-1 creates shared components that @mobile-2 needs:
1. Put in `apps/mobile/shared/components/` with proper exports
2. Message: "Shared component ready: `<LoadingSkeleton variant='chat' />` in shared/components/"

### Developer → QA Handoff
1. Developer updates board to `[IN_REVIEW]`
2. Messages QA: "LOCA-{id} ready: {what was built}, {what to test}, {known limitations}"
3. QA reads the acceptance criteria from EPIC-INDEX.md
4. QA runs full review checklist from their agent definition

### QA → Developer (Rejection)
1. QA lists SPECIFIC issues: `file:line — problem — how to fix`
2. QA updates board to `[IN_PROGRESS]`
3. Developer fixes, re-tests, moves back to `[IN_REVIEW]`
4. Max 2 rejection cycles — if 3rd rejection, escalate to @architect

## Blocking Protocol
When a task is blocked:
1. Set `[BLOCKED]` on board with reason: "BLOCKED by LOCA-{id} (@assignee)"
2. Message the blocking agent directly
3. Blocking agent prioritizes unblocking over new work
4. Once unblocked: blocker notifies, blocked agent resumes

## Daily Sync (before starting work)
Each agent reads `docs/taskboard/BOARD.md` and:
1. Check: any tasks `[BLOCKED]` that I'm blocking? → Fix those FIRST
2. Check: any tasks in `[IN_REVIEW]` for me? → Review those SECOND
3. Pick next `[TODO]` task assigned to me → Start new work THIRD

## Design → Code Flow (Figma)
1. @designer creates Figma frames for the sprint's screens
2. @designer shares Figma link in task board: `[TODO] LOCA-48: ... (Figma: {link})`
3. @mobile-1/@mobile-2 uses Figma MCP to extract design and generate code
4. If design questions: message @designer, do NOT improvise
