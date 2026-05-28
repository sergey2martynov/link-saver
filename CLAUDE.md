# MyMemory

A microservices app for saving and managing bookmarked links. Users add a URL — AI auto-generates a title. Tags are first-class entities with color and optional date range.

## Stack

| Layer | Tech |
|---|---|
| Backend | .NET 10, ASP.NET Core, Clean Architecture |
| DB | Dapper + PostgreSQL, FluentMigrator |
| Messaging | Apache Kafka (KRaft, no Zookeeper) |
| AI | Claude Haiku API (`claude-haiku-4-5-20251001`) |
| Realtime | SignalR (WebSocket push to browser) |
| Gateway | YARP Reverse Proxy, JWT Bearer |
| Frontend | Vue 3 + Vite + Pinia + TypeScript |
| Infra | Kubernetes (Docker Desktop), NodePort |

## Services

| Service | NodePort | Purpose |
|---|---|---|
| Gateway | 30080 | YARP proxy, JWT validation, UserIdHeaderTransform |
| UserService | 30081 | Registration, login, JWT, internal users API |
| LinkService | 30082 | Link CRUD, tags, SignalR hub, Kafka consumers |
| NamingService | 30083 | Consumes `links.created`, calls Claude Haiku, publishes `links.named` |

Frontend: `http://localhost:5173` (Vite dev server, proxies `/api` and `/hubs` to 30080).

## Service Architecture

Every service follows Clean Architecture: `Domain → Application → Infrastructure → Web`

```
ServiceName/
  src/
    ServiceName.Domain/        # Entities, no dependencies
    ServiceName.Application/   # DTOs, Interfaces, Services, Events, Exceptions
    ServiceName.Infrastructure/ # Repositories (Dapper), Kafka, Migrations, DI
    ServiceName.Web/           # Controllers, Program.cs, Middleware
  k8s/
    deployment.yaml            # NodePort + env from secrets
    secret.yaml                # gitignored if it contains real secrets
  Dockerfile
  redeploy.ps1
```

## Kafka Topics

| Topic | Producer | Consumer |
|---|---|---|
| `links.created` | LinkService | NamingService |
| `links.named` | NamingService | LinkService |
| `links.deleted` | LinkService | — |

Topics are auto-created (`KAFKA_AUTO_CREATE_TOPICS_ENABLE=true` in `infra/k8s/kafka.yaml`).

## LinkService Database

Tables: `links`, `tags`, `link_tags` (many-to-many junction), `link_domains`.

Tags have `name`, `color` (hex), `start_date?`, `end_date?`, `user_id`.  
`link_tags` has CASCADE DELETE on both FKs.  
Migrations run automatically on startup via `MigrateUp()`.

## SignalR Auth Flow

- Hub endpoint: `LinkService` → `/hubs/links`
- Gateway proxies `/hubs` via YARP
- JWT is sent as `?access_token=` query param — browsers can't send custom headers during WebSocket handshake
- Gateway reads the token in the `OnMessageReceived` event, YARP adds `X-User-Id` header downstream
- Hub joins connection to a group by `userId` — pushes only to the right user
- `LinkNamed` event updates the link title in real time without page reload

## Secrets & Running

```powershell
# One-time setup (persists across reboots):
setx ANTHROPIC_API_KEY "sk-ant-..."

# First run (provisions Postgres):
.\dev.ps1 -InitInfra

# Normal run (rebuild + redeploy all services + start frontend):
.\dev.ps1

# Specific services only:
.\dev.ps1 -Only gateway,linkservice

# Frontend only (backend already running):
.\dev.ps1 -SkipBack
# or just:
cd frontend && npm run dev
```

`dev.ps1` auto-creates the `namingservice-secrets` k8s secret from `$env:ANTHROPIC_API_KEY`.  
`NamingService/k8s/secret.yaml` is **gitignored**.

## Frontend Structure

```
frontend/src/
  api/
    client.ts       # fetch wrapper with JWT from auth store
    links.ts        # link CRUD + dismissSuggestions
    tags.ts         # tag CRUD
    types.ts        # LinkDto, TagDto, CreateLinkDto, UpdateLinkDto, ...
  stores/
    auth.ts         # Pinia: token, login, logout
  composables/
    useLinksHub.ts  # SignalR connection, onLinkNamed callback
  views/
    LinksView.vue   # Main page: links list + tag library
    LoginView.vue
    RegisterView.vue
```

## Key Design Decisions

- **Tags as a separate table**, not `text[]` — needed for color, date ranges, future metadata
- **SignalR JWT via query param** — browsers can't send custom headers during WS handshake
- **BackgroundLinkEventPublisher** — Kafka publish is fire-and-forget, doesn't block the HTTP response
- **NamingService skips links with non-empty title** — won't overwrite manually set titles
- **`KAFKA_AUTO_CREATE_TOPICS_ENABLE=true`** — no manual topic setup needed

## New Service Checklist

1. Create folder structure following the template above
2. Add `Dockerfile` and `k8s/deployment.yaml`
3. Add to `$allServices` in `dev.ps1`
4. Add route in `Gateway/src/Gateway.Web/appsettings.json`
5. Add projects to `MyMemory.sln`
