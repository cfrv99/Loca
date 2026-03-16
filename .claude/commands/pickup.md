---
name: pickup
description: Pick up next available task from the board. Reads taskboard, finds your TODO tasks, starts the highest priority one.
---

# Pick Up Next Task

## Workflow
1. Read `docs/taskboard/BOARD.md`
2. FIRST check: am I blocking anyone? (`[BLOCKED] ... BLOCKED by LOCA-{id}` where that id is mine) → Fix that FIRST
3. THEN check: any `[IN_REVIEW]` tasks waiting for me? → Review those SECOND
4. THEN find my next `[TODO]` task (earliest LOCA-{id} = highest priority)
5. Update the board: move task from TODO section to IN_PROGRESS section, change `[TODO]` to `[IN_PROGRESS]`
6. Read the task details from `docs/epics/EPIC-INDEX.md`
7. Check dependencies: are all "Depends On" tasks in `[DONE]`? If not → set `[BLOCKED]` and pick next
8. Load the relevant skill (create-endpoint / create-screen / create-game)
9. Load the relevant golden example
10. Start implementation

## Usage
```
claude> /pickup @backend-1
```
This finds and starts the next task for @backend-1.

If no role specified, use the role that matches the current work context.
