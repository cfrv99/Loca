# /review — Code review from QA/Architect perspective

## Usage
`/review {file_or_directory}` — Review code for quality, security, and convention compliance

## Checklist
1. **Architecture**: Does it follow Clean Architecture layer rules?
2. **Conventions**: Naming, file structure, patterns per `.claude/rules/`
3. **Security**: Input validation, SQL injection, XSS, auth checks
4. **Performance**: N+1 queries, missing indexes, unnecessary allocations
5. **Testing**: Are tests written? Do they cover edge cases?
6. **Error handling**: Result pattern used? No swallowed exceptions?
7. **SignalR** (if applicable): Group management, reconnection handling
8. **Mobile** (if applicable): Accessibility, offline handling, performance

## Output Format
For each finding: `[SEVERITY] file:line — description — suggested fix`
Severities: 🔴 CRITICAL | 🟡 WARNING | 🔵 INFO
