---
name: handoff
description: Hand off completed task to QA for review. Updates board, runs pre-handoff checks, notifies QA agent.
---

# Hand Off Task to QA

## Pre-Handoff Checks (MUST pass before handoff)
1. Run `/verify` — all checks must pass
2. Check `.claude/rules/definition-of-done.md` — all items checked
3. Ensure git commit exists with proper format

## Handoff Steps
1. Update `docs/taskboard/BOARD.md`: move task from IN_PROGRESS to IN_REVIEW
2. Write handoff note:
   ```
   ## LOCA-{id} Handoff
   **What was built:** {brief description}
   **Files changed:** {list of key files}
   **How to test:** {step-by-step manual test instructions}
   **Known limitations:** {any known issues or shortcuts}
   **Acceptance criteria:** {copy from EPIC-INDEX.md}
   ```
3. If backend task → QA is @qa-1
4. If mobile task → QA is @qa-2
5. Message: "@qa-{n} LOCA-{id} is ready for review. See handoff note above."

## Usage
```
claude> /handoff LOCA-42
```
