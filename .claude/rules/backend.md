# Backend Development Rules

## Architecture
- Clean Architecture: Domain has ZERO dependencies on other layers
- CQRS via MediatR: Commands mutate state, Queries read state, never mix
- Each service is a separate ASP.NET Core project with its own DbContext
- Services communicate via: direct method call (in-process) or message bus (RabbitMQ for async)
- API versioning via URL path: `/api/v1/venues`

## Database
- PostgreSQL 16 with PostGIS extension for spatial queries
- Use Fluent API for EF Core configuration, NEVER data annotations on domain entities
- All tables must have: `Id` (Guid), `CreatedAt`, `UpdatedAt` (UTC timestamps)
- Soft delete via `IsDeleted` + `DeletedAt` columns, NEVER hard delete user data
- Indexes: always add for foreign keys, frequently queried columns, and spatial columns (GIST)
- Migrations: one migration per feature, descriptive names like `AddVenueGeofenceRadius`

## API Design
- RESTful endpoints, resource-based URLs
- Response wrapper: `{ success: bool, data: T?, error: { code: string, message: string }? }`
- Pagination: cursor-based for feeds/lists, offset for admin panels
- Authentication: JWT Bearer tokens, refresh token rotation
- Rate limiting: per-user, per-endpoint configurable via middleware
- All endpoints documented with XML comments → Swagger/OpenAPI auto-generated

## SignalR
- One hub per domain: `VenueChatHub`, `GameHub`, `NotificationHub`, `MatchingHub`
- Hub methods: PascalCase (C# convention)
- Client methods: camelCase (JavaScript convention)
- Always use groups for venue-scoped messaging: `venue_{venueId}`
- Reconnection: client retries with exponential backoff, server tracks user state in Redis

## Error Handling
- Use `Result<T>` / `Result<T, Error>` pattern from domain layer
- Controllers map Result to appropriate HTTP status codes
- Never expose stack traces in production responses
- Log all errors with correlation ID for tracing

## Security
- Input validation via FluentValidation on every command/query
- Parameterized queries only (EF Core handles this)
- CORS: only allow registered mobile app origins + admin web origin
- Anti-forgery tokens for admin web
- Rate limit all public endpoints
