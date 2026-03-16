# /sprint-plan — Plan the next sprint

## Usage
`/sprint-plan {sprint_number}` — Generate a sprint plan from EPIC-INDEX

## Workflow
1. Read `docs/epics/EPIC-INDEX.md` and find the sprint
2. List all tasks for the sprint with assignees and story points
3. Check dependency graph — flag any blocked tasks
4. Calculate total story points vs team capacity (65 SP/sprint)
5. Identify risks and suggest mitigation
6. Output a formatted sprint plan with:
   - Sprint goal
   - Task breakdown by assignee
   - Dependency map
   - Definition of done
   - Sprint review agenda items
