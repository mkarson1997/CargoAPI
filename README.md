# CargoAPI

[![CI](https://github.com/mkarson1997/CargoAPI/actions/workflows/ci.yml/badge.svg)](https://github.com/mkarson1997/CargoAPI/actions/workflows/ci.yml)

A layered .NET Web API that selects the most cost-effective cargo carrier for an order based on dimensional-weight rules, persists operational data, and generates recurring daily carrier-cost reports in the background.

This repository is maintained as a backend engineering case study: business rules live outside controllers, persistence is isolated behind repositories, validation protects domain invariants, Hangfire handles recurring work, and automated tests protect core pricing behavior.

> **Runtime note:** the current code targets .NET 6. A tracked portfolio-hardening issue covers migration to a supported LTS runtime and containerized local development.

## Engineering highlights

- N-tier architecture with API, Business, DataAccess and Entities projects
- Carrier selection based on configurable desi ranges and pricing rules
- Entity Framework Core with SQL Server and Code First migrations
- Hangfire recurring background jobs and dashboard
- Idempotent-style daily carrier report upsert flow
- Swagger/OpenAPI development interface
- Global exception handling with safe JSON error responses
- Input validation for names, dimensions and pricing values
- Automated xUnit tests for core `OrderService` behavior
- Moq-based dependency isolation in business-layer tests
- GitHub Actions build + unit-test validation
- Security, contribution and pull-request documentation

## Architecture

```text
Client
  │
  ▼
CargoAPI.API
  │  HTTP / validation / middleware / Swagger
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
├── .github/workflows/ci.yml
├── SECURITY.md
└── CONTRIBUTING.md
```

## Core business rule

When an order is created, the API receives `orderDesi` and evaluates active carrier configurations.

### Case 1: desi is inside a configured range

The system selects the lowest-priced eligible carrier configuration.

### Case 2: desi is outside every configured range

The current implementation chooses the configuration whose `CarrierMaxDesi` is closest and applies the extra-desi calculation:

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

The behavior for orders below every configured range and for gaps between ranges is intentionally tracked as an explicit domain-rule decision before changing production behavior.

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

## Automated tests

The `CargoAPI.Tests` project currently protects the most important `OrderService` behaviors:

- non-positive desi is rejected without persistence,
- the cheapest carrier is selected when multiple ranges match,
- extra-desi pricing is calculated for the documented above-range case,
- no order is persisted when no carrier configuration exists.

Run the suite:

```bash
dotnet restore CargoAPI.Tests/CargoAPI.Tests.csproj
dotnet test CargoAPI.Tests/CargoAPI.Tests.csproj --configuration Release
```

The tests use mocks for repositories and logging so the core business behavior can be validated without SQL Server.

## Quick start

### Requirements

- .NET 6 SDK for the current repository state
- SQL Server LocalDB, Express or full SQL Server
- `dotnet-ef`

Install EF tooling:

```bash
dotnet tool install --global dotnet-ef
```

### Configure the database

Set `DefaultConnection` in `CargoAPI.API/appsettings.json` for your local environment. Do not commit production credentials.

Example LocalDB configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=CargoDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Apply migrations:

```bash
dotnet ef database update \
  --project CargoAPI.DataAccess \
  --startup-project CargoAPI.API
```

Or use the idempotent SQL bootstrap script:

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -i database/CargoDb_Create.sql
```

Run the API:

```bash
dotnet run --project CargoAPI.API
```

Development interfaces:

```text
Swagger:  http://localhost:5246/swagger
Hangfire: http://localhost:5246/hangfire
```

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

Every push and pull request to `main` performs restore, Release build and unit tests.

```bash
dotnet restore CargoAPI.sln
dotnet restore CargoAPI.Tests/CargoAPI.Tests.csproj
dotnet build CargoAPI.sln --configuration Release --no-restore
dotnet test CargoAPI.Tests/CargoAPI.Tests.csproj --configuration Release --no-restore
```

## Engineering roadmap

- Upgrade the runtime to a currently supported LTS target
- Add integration tests for order and report endpoints
- Add Docker-based SQL Server development environment
- Resolve and test below-range/gap pricing semantics
- Move API contracts toward explicit request/response DTOs where needed
- Add authentication/authorization before internet-facing deployment
- Add structured logging and request correlation
- Add test coverage reporting

## Security

This repository is a portfolio/reference project and should not be exposed to the public internet without deployment hardening and authentication. See [SECURITY.md](SECURITY.md) for vulnerability reporting guidance.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Pull requests use the repository PR checklist and should include validation evidence.

---

Built and maintained by [Mahmoud Karzoun](https://github.com/mkarson1997).
