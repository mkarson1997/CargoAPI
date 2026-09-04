# CargoAPI Portfolio Upgrade Plan

CargoAPI is already a strong backend case study. The next upgrades should make its engineering evidence easier to verify.

## Priority 1

- Add unit tests for carrier-selection boundary cases.
- Add integration tests for order creation and report generation.
- Upgrade from .NET 6 to a currently supported LTS target.
- Add structured logging and request correlation.

## Priority 2

- Add Docker-based SQL Server local development.
- Add authentication and authorization before any public deployment.
- Add explicit request/response DTO contracts where appropriate.
- Document representative error responses.

## Portfolio proof

The repository should make these qualities directly verifiable:

- domain rules outside controllers,
- persistence boundaries,
- background processing,
- validation,
- automated tests,
- reproducible environment,
- safe failure handling,
- CI and security documentation.