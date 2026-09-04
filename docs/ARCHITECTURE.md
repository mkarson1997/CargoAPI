# CargoAPI Architecture

CargoAPI is organized as a layered backend case study. The goal is to keep HTTP concerns, business rules, persistence and data models separated enough that each layer can be reviewed and tested independently.

## Layer map

```text
HTTP client
    │
    ▼
CargoAPI.API
    │  controllers / middleware / OpenAPI / DI
    ▼
CargoAPI.Business
    │  application services / validation / pricing decisions
    ▼
CargoAPI.DataAccess
    │  repositories / EF Core / DbContext / migrations
    ▼
CargoAPI.Entities
       entities and shared data structures

SQL Server ◄──────── EF Core
Hangfire   ◄──────── background reporting workflow
```

## Dependency direction

The intended dependency direction is downward through the layers:

```text
API → Business → DataAccess → Entities
```

Controllers should not own carrier-pricing rules, and business services should not embed SQL queries directly.

## Order creation sequence

```text
POST order
   │
   ▼
OrderService.AddAsync(orderDesi)
   │
   ├── reject invalid desi
   │
   ├── query matching carrier configurations
   │       │
   │       ├── matches exist → choose lowest CarrierCost
   │       │
   │       └── no match → query closest configuration
   │                         │
   │                         └── apply documented extra-desi formula
   │
   ▼
Create Order entity
   │
   ▼
Generic repository AddAsync + SaveAsync
   │
   ▼
SQL Server
```

The public unit-test project mocks repository dependencies so `OrderService` behavior can be checked without requiring SQL Server.

## Background reporting sequence

```text
Hangfire schedule
   │
   ▼
Carrier report service
   │
   ├── read orders
   ├── group by carrier + date
   ├── aggregate carrier cost
   └── update/create daily report
```

This keeps recurring reporting out of the request/response path.

## Current boundaries

### API layer

Responsible for transport-level concerns such as HTTP endpoints, middleware, dependency injection and Swagger/OpenAPI exposure.

### Business layer

Responsible for application rules such as validating desi input, selecting a carrier and coordinating report generation.

### DataAccess layer

Responsible for EF Core database access and repository implementations. Carrier-configuration queries live here rather than inside controllers.

### Entities layer

Contains the persisted domain/data structures shared by the layers.

## Testability

`CargoAPI.Tests` currently covers the central order-pricing path with xUnit and Moq. The next testing boundary is HTTP/database integration so that routing, EF Core behavior and migrations are validated together.

## Known architectural debt

- .NET 6 is out of support and is tracked for upgrade.
- Authentication/authorization must be added before internet-facing deployment.
- Below-range and gap pricing semantics need an explicit domain decision before changing behavior.
- Structured request correlation and richer observability are still roadmap items.
- Containerized SQL Server development is still a tracked improvement.

## Review entry points

For a quick engineering review, read in this order:

1. `CargoAPI.Business/Services/OrderService.cs`
2. `CargoAPI.DataAccess/Repositories/CarrierConfigurationRepository.cs`
3. `CargoAPI.Tests/OrderServiceTests.cs`
4. `.github/workflows/ci.yml`
5. `SECURITY.md`
6. `docs/DOMAIN_RULES.md`
