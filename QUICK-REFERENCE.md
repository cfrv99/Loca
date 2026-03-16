# Loca — Quick Reference Card

## Slash Commands
| Command | Nə edir |
|---------|---------|
| `/feature LOCA-42` | EPIC-INDEX-dən task-ı tapır, role assign edir, implement edir |
| `/sprint-plan 3` | Sprint 3-ün task listini, dependency-ləri, risk-ləri göstərir |
| `/review services/venue/` | Code review — security, performance, conventions check |
| `/adr "Expo vs RN CLI"` | Architecture Decision Record yaradır docs/architecture/adr/ |

## Agent Team Roles
| Role | Məsuliyyət | Fokus Faylları |
|------|-----------|----------------|
| @architect | System design, API contracts, ADRs | docs/architecture/*, infrastructure/* |
| @backend-1 | Identity, Venue, Economy, Matching services | services/identity,venue,economy/* |
| @backend-2 | Social, Game, Notification, AI services | services/social,game,notification/* |
| @mobile-1 | Core UI, navigation, venue, check-in, gifting | apps/mobile/features/auth,venue,gifting/* |
| @mobile-2 | Chat, games, matching UI, animations | apps/mobile/features/chat,games,matching/* |
| @designer | Wireframes, design tokens, Lottie animations | .claude/rules/design-system.md |
| @pm | Sprint planning, priorities, stakeholder updates | docs/epics/* |
| @qa-1 | Backend tests, API contracts, load tests | tests/* |
| @qa-2 | Mobile tests, E2E, device matrix | apps/mobile/__tests__/*, tests/e2e/* |
| @ba | Requirements, acceptance criteria, content | docs/epics/*, content databases |

## Sprint Overview
| Sprint | Həftə | EPIC | Core Deliverable |
|--------|-------|------|-----------------|
| S1 | 1-2 | Foundation | Monorepo, Docker, CI/CD, design tokens |
| S2 | 3-4 | Auth | Google/Apple login, onboarding, profiles |
| S3 | 5-6 | Venue | Discovery, QR scan, geofence check-in |
| S4 | 7-8 | Social Hub | Public chat, feed, people browser |
| S5 | 9-10 | Games | Mafia, Truth or Dare, game engine |
| S6 | 10-11 | Matching | Match requests, private chat |
| S7 | 11-12 | Gifting | Coins, IAP, gift animations |
| S8 | 12-13 | Launch | App Store, production, 10 venues |
| S9-11 | 14-20 | AI + Engagement | Vibe Radar, Vibe Bomb, Uno/Domino |
| S12-15 | 21-30 | Differentiation | AR Ghost, DJ/Playlist, Chain Party |

## MCP Quick Commands
```bash
# Library docs
"use context7 for Expo Camera docs"
"use context7 for SignalR .NET 8 docs"
"use context7 for React Navigation v7 docs"

# Architecture thinking
"use sequential-thinking to design the QR rotation system"

# Database exploration
"show me all tables in the venue schema"
```

## Git Conventions
- Branch: `feature/LOCA-42-qr-scanner`
- Commit: `feat(venue): add QR rotation logic [LOCA-42]`
- PR: title = commit message, body = task acceptance criteria
