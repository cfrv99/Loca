---
name: verify
description: Full verification with MCP-powered checks. Run before completing any task or sprint.
---

# /verify — Full Project Verification

Run ALL checks. Fix failures before proceeding.

### 1. Backend Build & Tests
```bash
dotnet build --nologo
dotnet test --nologo --verbosity minimal
```

### 2. Mobile Type Check & Tests
```bash
cd apps/mobile && npx tsc --noEmit --pretty
cd apps/mobile && npx jest --passWithNoTests --silent
```

### 3. Database Verification (use postgres MCP)
- "Use postgres MCP: SELECT count(*) FROM venue.venues" — should be ≥15
- "Use postgres MCP: SELECT count(*) FROM identity.users" — should be ≥10
- "Use postgres MCP: verify all schemas exist (identity, venue, social, game, economy)"
- Verify no orphaned records, foreign key integrity

### 4. API Verification
- Swagger loads at /swagger with all endpoints documented
- Every controller method has XML comments
- Every endpoint returns ApiResponse<T>

### 5. Security (auto-checked by hooks, but double check)
- No hardcoded secrets in any code file
- No console.log without __DEV__ guard
- JWT extracted from claims, never from request body

### 6. Convention Check
- .cs files: PascalCase
- .ts/.tsx files: kebab-case
- All screens handle loading + error + empty states
- NativeWind classes only (no inline styles)
- Result<T> pattern (no thrown exceptions for business logic)

### 7. Save State (use memory MCP)
- Save current verification results to Memory MCP
- Include: what passed, what failed, what was fixed

### Output
```
✅ Backend: build PASS, tests {n}/{n} PASS
✅ Mobile: tsc PASS, jest {n}/{n} PASS
✅ Database: {n} venues, {n} users, schemas OK
✅ API: Swagger OK, all endpoints documented
✅ Security: no issues
✅ Conventions: all followed
💾 State saved to Memory MCP
```
