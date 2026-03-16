---
name: feature
description: Implement a specific task from EPIC-INDEX with full MCP integration
---

# /feature LOCA-{id} — Implement a Task

## Workflow (EVERY step mandatory)

### 1. Load Task
- Read `docs/epics/EPIC-INDEX.md`, find LOCA-{id}
- Check dependencies — are all "Depends On" tasks DONE in taskboard?
- If blocked → report and pick next available task
- Update `docs/taskboard/BOARD.md`: set `[IN_PROGRESS]`

### 2. Architecture (use sequential-thinking MCP)
- "Think step by step: what is the best approach for {task description}?"
- Consider: existing patterns, dependencies, edge cases

### 3. Fetch Docs (use context7 MCP — MANDATORY)
- "use context7 for {relevant library}" — for EVERY library you'll use
- Examples:
  - Backend: "use context7 for EF Core 8 {specific feature}"
  - Mobile: "use context7 for Expo {specific module}"
  - SignalR: "use context7 for SignalR .NET 8 {specific feature}"
- DO NOT code from memory. ALWAYS fetch fresh docs.

### 4. Implement
- Read the relevant skill: create-endpoint / create-screen / create-game
- Read the golden example for the task type
- Follow the skill step-by-step, match the golden example pattern
- If Figma link in task → use figma MCP to extract design

### 5. Verify
- Backend: `dotnet build` + `dotnet test`
- Mobile: `npx tsc --noEmit` + `npx jest`
- Database: use postgres MCP to verify data
- Check `.claude/rules/definition-of-done.md`
- Check `.claude/rules/anti-patterns.md`

### 6. Complete
- Git commit: `feat(scope): description [LOCA-{id}]`
- Update taskboard: `[IN_REVIEW]`
- Save key decisions to Memory MCP
