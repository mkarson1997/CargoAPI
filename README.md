# CargoAPI

[![CI](https://github.com/mkarson1997/CargoAPI/actions/workflows/ci.yml/badge.svg)](https://github.com/mkarson1997/CargoAPI/actions/workflows/ci.yml)

A layered .NET 10 Web API that selects the most cost-effective cargo carrier for an order based on dimensional-weight rules, persists operational data in SQL Server, and generates recurring carrier-cost reports with Hangfire.

This repository is maintained as a backend engineering case study: business rules live outside controllers, persistence is isolated behind repositories, validation protects domain invariants, automated tests cover core pricing behavior, and the complete development stack can be started with Docker Compose.

## Engineering highlights

- .NET 10 LTS and Entity Framework Core 10
- N-tier architecture with API, Business, DataAccess and Entities projects
- Carrier selection based on configurable desi ranges and pricing rules
- SQL Server persistence with EF Core Code First migrations
- Hangfire recurring background jobs and dashboard
- Idempotent-style daily carrier report upsert flow
- Swagger/OpenAPI development interface
- Global exception handling with safe JSON error responses
- Liveness and database-readiness endpoints
- Optional migration-on-start behavior controlled by configuration
- Automated xUnit tests for core `OrderService` behavior
- Moq-based dependency isolation in business-layer tests
- GitHub Actions build + unit-test validation
- Multi-stage production-style Docker image
- Docker Compose development stack with SQL Server health gating
- Security, contribution, architecture and domain-rule documentation

## Architecture

```text
Client
  │
  ▼
CargoAPI.API
  │  HTTP / validation / middleware / Swagger / health
  ▼
CargoAPI.Business
  │  application and carrier-selection rules
  ▼
CargoAPI.DataAccess
  │  EF Core repositories / DbContext / migrations
  ▼
CargoAPI.Entities
     entities and DTOs

SQL Server ◄──── EF Core
Hangfire   ◄──── recurring reporting jobs
```

Repository layout:

```text
CargoAPI.sln
├── CargoAPI.API
├── CargoAPI.Business
├── CargoAPI.DataAccess
├── CargoAPI.Entities
├── CargoAPI.Tests
├── database/
├── docs/
├── Dockerfile
├── docker-compose.yml
├── .env.example
├── global.json
├── .github/workflows/ci.yml
├── SECURITY.md
└── CONTRIBUTING.md
```

See also:

- [Architecture notes](docs/ARCHITECTURE.md)
- [Domain rules](docs/DOMAIN_RULES.md)
- [Portfolio hardening roadmap](docs/PORTFOLIO_UPGRADE.md)

## Core business rule

When an order is created, the API receives `orderDesi` and evaluates active carrier configurations.

### Case 1: desi is inside a configured range

The system selects the lowest-priced eligible carrier configuration.

### Case 2: desi is above every configured range

The documented example applies the extra-desi calculation:

```text
finalPrice = carrierPrice + (carrierPlusDesiCost × difference)
```

Example:

```text
Order desi:           13
Configured max desi:  10
Base carrier price:   32 TRY
Extra desi price:      4 TRY

32 + (4 × 3) = 44 TRY
```

Behavior for orders below every configured range and gaps between ranges remains an explicit product/domain decision rather than being silently frozen by tests. See [issue #3](https://github.com/mkarson1997/CargoAPI/issues/3).

## Background reporting

Hangfire runs the `carrier-reports` recurring job every hour.

The job:

1. reads orders,
2. groups them by carrier and calendar date,
3. aggregates `OrderCarrierCost`,
4. updates an existing report or creates a new one.

This keeps reporting work outside the request/response path and demonstrates a background-processing workflow.

## API surface

### Carriers

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/Carriers` | List carriers |
| `POST` | `/api/Carriers` | Create a carrier |
| `PUT` | `/api/Carriers` | Update a carrier |
| `DELETE` | `/api/Carriers/{id}` | Delete a carrier |

### Carrier configurations

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/CarrierConfigurations` | List pricing configurations |
| `POST` | `/api/CarrierConfigurations` | Create a configuration |
| `PUT` | `/api/CarrierConfigurations` | Update a configuration |
| `DELETE` | `/api/CarrierConfigurations/{id}` | Delete a configuration |

### Orders

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/Orders` | List orders |
| `POST` | `/api/Orders` | Create an order and select its carrier |
| `DELETE` | `/api/Orders/{id}` | Delete an order |

### Reports

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/CarrierReports` | List carrier reports |
| `POST` | `/api/CarrierReports/generate` | Trigger report generation manually |

## Health endpoints

| Route | Meaning |
|---|---|
| `GET /health/live` | Process is running and HTTP is responsive |
| `GET /health/ready` | API can connect to SQL Server |

The readiness endpoint returns HTTP `503` when the database cannot be reached, making it suitable for container/orchestrator readiness checks.

## Automated tests

`CargoAPI.Tests` protects key `OrderService` behavior including:

- non-positive desi rejection without persistence,
- exact lower and upper range boundaries,
- cheapest-carrier selection when multiple ranges match,
- documented above-range extra-desi pricing,
- missing configuration failure without persistence.

Run the suite:

```bash
dotnet restore CargoAPI.sln
dotnet test CargoAPI.Tests/CargoAPI.Tests.csproj --configuration Release
```

The tests use mocks for repositories and logging so business behavior can be validated without SQL Server.

## Quick start: Docker Compose

### Requirements

- Docker Desktop or Docker Engine with Compose support

Create your local environment file:

```bash
cp .env.example .env
```

On Windows PowerShell:

```powershell
Copy-Item .env.example .env
```

Edit `.env` and replace the example SQL Server password with a strong local-only password.

Start the complete stack:

```bash
docker compose up --build
```

Compose waits for SQL Server to become healthy before starting the API. The API receives its connection string through environment variables and applies EF Core migrations because the Compose environment explicitly sets:

```text
Database__ApplyMigrations=true
```

Useful local URLs:

```text
Swagger:    http://localhost:8080/swagger
Hangfire:   http://localhost:8080/hangfire
Liveness:   http://localhost:8080/health/live
Readiness:  http://localhost:8080/health/ready
SQL Server: localhost,14333
```

Stop the stack:

```bash
docker compose down
```

Remove the local SQL Server volume as well:

```bash
docker compose down -v
```

## Quick start: local .NET SDK

### Requirements

- .NET 10 SDK
- SQL Server LocalDB, Express or another SQL Server instance
- `dotnet-ef` 10.x for migration commands

The repository includes `global.json` to keep local and CI SDK selection in the .NET 10 toolchain.

Install EF tooling if needed:

```bash
dotnet tool install --global dotnet-ef --version 10.*
```

The committed `appsettings.json` contains only a LocalDB development connection string and does not contain a database password. You can override it without editing tracked files:

```text
ConnectionStrings__DefaultConnection=<your connection string>
```

Apply migrations manually:

```bash
dotnet ef database update \
  --project CargoAPI.DataAccess \
  --startup-project CargoAPI.API
```

Run the API:

```bash
dotnet run --project CargoAPI.API
```

`Database:ApplyMigrations` is `false` by default. Automatic migration is opt-in and enabled by the local Docker Compose configuration only.

## Example workflow

Create a carrier:

```json
{
  "carrierName": "Yurtici Kargo",
  "carrierIsActive": true,
  "carrierPlusDesiCost": 4,
  "carrierConfigurationId": 0
}
```

Create a pricing configuration:

```json
{
  "carrierId": 1,
  "carrierMaxDesi": 10,
  "carrierMinDesi": 1,
  "carrierCost": 32.00
}
```

Create an order:

```json
{
  "orderDesi": 13
}
```

For the documented example above, the calculated carrier cost is `44 TRY`.

## Validation and failure handling

The application validates key business inputs such as:

- non-empty carrier names,
- positive order desi,
- valid min/max desi ranges,
- non-negative extra-desi cost,
- positive carrier configuration prices.

Unhandled failures pass through global exception middleware so the API returns a consistent response without exposing a server stack trace.

## CI

Every push and pull request to `main` uses the .NET 10 SDK and performs restore, Release build and unit tests:

```bash
dotnet restore CargoAPI.sln
dotnet build CargoAPI.sln --configuration Release --no-restore
dotnet test CargoAPI.Tests/CargoAPI.Tests.csproj --configuration Release --no-build
```

## Engineering roadmap

- Add API/database integration tests
- Resolve and test below-range/gap pricing semantics
- Move API contracts toward explicit request/response DTOs where needed
- Add authentication/authorization before internet-facing deployment
- Add structured logging and request correlation
- Add coverage reporting
- Add container image build validation to CI

## Security

This repository is a portfolio/reference project and should not be exposed to the public internet without authentication and deployment hardening. `.env` is ignored and should never be committed. See [SECURITY.md](SECURITY.md) for vulnerability reporting guidance.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Pull requests use the repository PR checklist and should include validation evidence.

## License

Project-authored source and documentation are available under the MIT License. Third-party packages and tooling retain their own licenses; see [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

---

Built and maintained by [Mahmoud Karzoun](https://github.com/mkarson1997).
