# Loca — Claude Code Setup Guide

## Quraşdırma (bir dəfəlik, 5 dəqiqə)

### 1. Bu zip-i extract et
```bash
mkdir loca && cd loca
# zip-in içindəkiləri bura extract et
```

### 2. MCP Serverləri qoş
```bash
claude mcp add context7 -- npx -y @upstash/context7-mcp@latest
claude mcp add github -e GITHUB_PERSONAL_ACCESS_TOKEN=ghp_YOUR_TOKEN -- npx -y @modelcontextprotocol/server-github
claude mcp add sequential-thinking -- npx -y @modelcontextprotocol/server-sequential-thinking
claude mcp add memory -- npx -y @modelcontextprotocol/server-memory
claude mcp add playwright -- npx -y @anthropic/mcp-playwright
claude mcp add --transport http figma https://mcp.figma.com/mcp
```
PostgreSQL MCP docker-compose up-dan sonra əlavə olunacaq.

### 3. APP-I YARAT — TEK COMMAND
```bash
claude --dangerously-skip-permissions
```
Claude Code açılacaq. Sonra yaz:
```
/build-loca
```

**Bu qədər. Geridə otur.** Claude Code:
- Monorepo yaradacaq
- Docker-ları qaldıracaq
- 7 agent spawn edəcək (architect, 2 backend, 2 mobile, 2 QA)
- 8 sprint-i ardıcıl işləyəcək
- Hər sprint sonunda status yazacaq
- Heç nə soruşmayacaq
- Hər kodu auto-test edəcək
- Sonda tam işlək app hazır olacaq

### Token İstehlakı Xəbərdarlığı
Bu proses çox token istifadə edəcək. Opus 4.6 + Agent Teams + 8 sprint = təxminən:
- ~2-5M input tokens, ~500K-1M output tokens per sprint
- Cəmi: ~20-40M tokens (bütün app üçün)
- Claude Max plan tövsiyə olunur
- `--max-budget-usd 100` əlavə edə bilərsən limit üçün

---

## 4. Fayl Strukturu İzahı

```
loca/
├── CLAUDE.md                    # 🧠 ANA BEYIN — project overview, tech stack,
│                                #    monorepo structure, conventions, team roles,
│                                #    constraints. Claude hər session-da bunu oxuyur.
│
├── .claude/
│   ├── mcp.json                 # 🔌 MCP server configs (Context7, GitHub, etc.)
│   ├── settings.json            # ⚙️ Model, permissions, agent teams flag
│   │
│   ├── rules/                   # 📏 Auto-loaded rules (hər session-da active)
│   │   ├── backend.md           #    ASP.NET Core + EF Core + SignalR conventions
│   │   ├── mobile.md            #    React Native Expo + NativeWind conventions
│   │   ├── testing.md           #    Test strategy (xUnit, Jest, Detox, k6)
│   │   └── design-system.md    #    Colors, typography, spacing, components
│   │
│   ├── commands/                # ⚡ Slash commands (manual trigger)
│   │   ├── feature.md           #    /feature LOCA-{id} — implement a task
│   │   ├── sprint-plan.md       #    /sprint-plan {n} — generate sprint plan
│   │   ├── review.md            #    /review {path} — code review checklist
│   │   └── adr.md               #    /adr {title} — architecture decision record
│   │
│   ├── agents/                  # 🤖 Agent team definitions
│   │   └── team-config.md       #    10 role definitions, sprint-specific configs
│   │
│   └── skills/                  # 🎯 Custom skills (boş — lazım olduqca əlavə et)
│
└── docs/
    ├── epics/
    │   └── EPIC-INDEX.md        # 📋 MASTER PLAN — 15 EPICs, 160+ tasks,
    │                            #    dependencies, assignees, story points
    └── architecture/
        ├── system-overview.md   # 🏗️ C4 architecture, service boundaries
        ├── database-schema.md   # 🗄️ Full PostgreSQL schema (5 schemas)
        └── adr/                 # 📝 Architecture Decision Records (boş — /adr ilə doldur)
```

---

## 5. Quality Enforcement Mexanizmləri

### Hooks (Avtomatik)
`.claude/settings.json`-da 4 hook konfiqurasiya olunub:

**Stop Hook** — Claude hər dəfə dayanmaq istəyəndə avtomatik subagent işə düşür:
- Backend kod yazdısa → `dotnet build` yoxlayır
- Mobile kod yazdısa → `npx tsc --noEmit` yoxlayır
- Yeni fayl yaratdısa → naming convention yoxlayır
- Feature yazdısa → test yazılıb-yazılmadığını yoxlayır
- HƏR ŞEY keçirsə → dayanır. Biri fail edərsə → əvvəlcə fix edir.

**Security Hook** — Hər fayl write/edit-dən əvvəl `.claude/hooks/security-check.py` işə düşür:
- Hardcoded password, API key, secret axtarır
- Tapsa → BLOCK edir, commit olmur

**Build Hooks** — .cs faylı edit edəndə avtomatik `dotnet build`, .ts faylı edit edəndə `tsc --noEmit`

**Commit Hook** — `git commit` əvvəli commit mesaj formatını yoxlayır

### Skills (Step-by-Step Guides)
3 custom skill var — Claude bunları oxuyub addım-addım izləyir:
- `/skill create-endpoint` — Backend endpoint yaratma (Domain → Application → Infrastructure → API → Tests)
- `/skill create-screen` — Mobile ekran yaratma (Screen → Hooks → Components → Tests)
- `/skill create-game` — Oyun implement etmə (Config → State Machine → SignalR → UI → Tests)

### Golden Examples (Reference Code)
`docs/golden-examples/` qovluğunda ideal kod nümunələri var:
- `backend/checkin-endpoint.md` — Full check-in endpoint (controller + handler + validator + test)
- `mobile/discover-screen.md` — Full discover ekranı (screen + hook + component + test)

Claude bu nümunələri oxuyub EYNI pattern ilə yeni kod yazır.

### Self-Verification
```
claude> /verify
```
Build, tests, types, lint, security, conventions — hamısını yoxlayır. Fail varsa fix edir.

## 6. Vacib Qeydlər

### CLAUDE.md haqqında
- Claude hər session-da avtomatik oxuyur
- Qısa saxla — 200-dən çox instruction olmasın
- `@docs/architecture/system-overview.md` kimi @import-lar lazy-load olunur

### Rules haqqında
- `.claude/rules/` içindəki hər .md avtomatik yüklənir
- Backend task-da mobile.md da yüklənir — bu normaldır, Claude lazım olanı seçir
- Çox uzun rules context window-u doldurar — hər rule faylı max 100 sətir

### Commands haqqında
- `/feature`, `/review`, `/sprint-plan`, `/adr` — manual trigger olunur
- Claude özü də oxuya bilər, amma slash command daha sürətlidir

### Agent Teams haqqında
- Experimental feature — Opus 4.6 model lazımdır
- Token istehlakı yüksəkdir — sadəcə paralel iş tələb edən sprint-lərdə istifadə et
- Hər agent öz context window-unda işləyir — böyük kontekst problemi yoxdur

### MCP Serverləri haqqında
- Context7 ən vacibidir — library docs üçün "use context7" yaz prompt-da
- 20k+ token istifadə edən MCP-lər performance-ı azaldır — ehtiyatlı ol
- Tool Search aktiv et ki MCP-lər lazy-load olunsun
