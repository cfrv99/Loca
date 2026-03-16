---
name: qa-review
description: Use when reviewing a task that is in IN_REVIEW status. Runs comprehensive quality checks, tests, and produces a pass/fail verdict.
---

# QA Review Skill

## Step 1: Read Context
- Read handoff note from the developer
- Read acceptance criteria from `docs/epics/EPIC-INDEX.md`
- Identify if this is backend or mobile task

## Step 2: Automated Checks
### For Backend
```bash
dotnet build --nologo                              # Must pass
dotnet test --nologo --verbosity minimal            # Must pass
# Check test count — if < 3 tests for this feature, REJECT
```

### For Mobile
```bash
cd apps/mobile
npx tsc --noEmit                                    # Must pass
npx jest --silent                                    # Must pass
npx eslint . --ext .ts,.tsx --quiet                 # No errors
```

## Step 3: Manual Code Review
Read every file that was changed and check against:
- Agent-specific review checklist (qa-1.md or qa-2.md)
- `docs/golden-examples/` — does code match the reference pattern?
- `.claude/rules/anti-patterns.md` — any violations?

## Step 4: Acceptance Criteria
Read the task's acceptance criteria from EPIC-INDEX.md.
For each criterion: mentally trace the code path to verify it's implemented.

## Step 5: Verdict

### APPROVED
```
✅ LOCA-{id} APPROVED
- Build: ✅ pass
- Tests: ✅ {n} tests pass
- Code quality: ✅ follows patterns
- Security: ✅ no issues
- Acceptance criteria: ✅ all met
```
Update board → `[DONE]`

### REJECTED
```
❌ LOCA-{id} REJECTED
Issues found:
1. 🔴 {file}:{line} — {critical issue} — Fix: {suggestion}
2. 🟡 {file}:{line} — {warning} — Fix: {suggestion}
3. 🔵 {file}:{line} — {suggestion} — Nice to have
```
Update board → `[IN_PROGRESS]`, message developer with issues.
