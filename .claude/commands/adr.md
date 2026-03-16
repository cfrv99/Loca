# /adr — Create an Architecture Decision Record

## Usage
`/adr {title}` — Document a technical decision

## Template
Create a new file at `docs/architecture/adr/ADR-{NNN}-{title}.md`:

```markdown
# ADR-{NNN}: {Title}

**Status:** Proposed | Accepted | Deprecated | Superseded by ADR-{X}
**Date:** {today}
**Deciders:** {team roles involved}

## Context
What is the issue that we're seeing that is motivating this decision?

## Decision
What is the change that we're proposing and/or doing?

## Consequences
### Positive
### Negative
### Risks

## Alternatives Considered
| Option | Pros | Cons | Why rejected |
```

## Important
- Always search existing ADRs before creating a new one
- Reference relevant LOCA task IDs
- Use `sequential-thinking` MCP for complex decisions
