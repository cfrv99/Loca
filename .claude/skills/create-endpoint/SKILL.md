---
name: create-endpoint
description: Use when creating a new REST API endpoint. Generates all Clean Architecture layers (Domain → Application → Infrastructure → API) with validation, tests, and Swagger docs.
---

# Create Endpoint Skill

When creating a new API endpoint, follow this EXACT order. Do NOT skip any step.

## Step 1: Domain Entity (if new)
- Create in `packages/domain/{ServiceName}/`
- Extend `BaseEntity` or `SoftDeletableEntity`
- Add domain logic methods (validation, business rules)
- ZERO dependencies on other layers

## Step 2: Application Layer
Create these files in `services/{service}/Application/`:

### Command or Query (MediatR)
```csharp
// Commands mutate state, Queries read state. NEVER mix.
public record CreateVenueCommand(string Name, string Address, ...) : IRequest<Result<VenueDto>>;
```

### Validator (FluentValidation)
```csharp
public class CreateVenueCommandValidator : AbstractValidator<CreateVenueCommand>
{
    public CreateVenueCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        // EVERY field must have validation
    }
}
```

### Handler
```csharp
public class CreateVenueHandler : IRequestHandler<CreateVenueCommand, Result<VenueDto>>
{
    // Use Result<T> pattern, never throw for business logic
}
```

### DTO
```csharp
public record VenueDto(Guid Id, string Name, string Address, ...);
```

## Step 3: Infrastructure (EF Core)
- Add entity configuration in `services/{service}/Infrastructure/Persistence/`
- Use Fluent API, NEVER data annotations
- Add migration: `dotnet ef migrations add {Name} --project services/{service}`

## Step 4: API Controller
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class VenuesController : ControllerBase
{
    /// <summary>
    /// Creates a new venue
    /// </summary>
    /// <response code="201">Venue created</response>
    /// <response code="400">Validation error</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<VenueDto>), 201)]
    public async Task<IActionResult> Create([FromBody] CreateVenueCommand cmd)
    {
        var result = await _mediator.Send(cmd);
        return result.Match(
            v => CreatedAtAction(nameof(GetById), new { id = v.Id }, ApiResponse<VenueDto>.Ok(v)),
            e => BadRequest(ApiResponse<VenueDto>.Fail(e.Code, e.Message))
        );
    }
}
```

## Step 5: Tests (MANDATORY)
Create in `tests/unit/` and `tests/integration/`:
- 1 validator test (happy path + 2 edge cases)
- 1 handler test (happy path + error case)
- 1 integration test (full HTTP request → DB → response)

## Step 6: Verify
```bash
dotnet build
dotnet test
# Open https://localhost:5001/swagger and verify endpoint appears
```

## Checklist Before Done
- [ ] Domain entity has no external dependencies
- [ ] Every input field has FluentValidation rule
- [ ] Handler uses Result<T>, no thrown exceptions
- [ ] Controller returns ApiResponse<T> wrapper
- [ ] XML comments on controller method (for Swagger)
- [ ] At least 3 tests written
- [ ] Migration created and tested
- [ ] `dotnet build` passes
- [ ] `dotnet test` passes
