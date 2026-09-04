# Contributing

Contributions that improve correctness, maintainability, security, documentation or test coverage are welcome.

## Development workflow

1. Fork the repository or create a feature branch.
2. Restore dependencies with `dotnet restore CargoAPI.sln`.
3. Build with `dotnet build CargoAPI.sln --configuration Release`.
4. Keep API, business, data-access and entity responsibilities separated.
5. Avoid committing connection strings, credentials, generated database files or local IDE state.
6. Open a focused pull request that explains the problem, the solution and how it was validated.

## Pull request checklist

- [ ] Build succeeds locally.
- [ ] New behavior is documented.
- [ ] Input validation and error paths were considered.
- [ ] No secrets or personal data are included.
- [ ] Database changes include an appropriate migration or script update.
- [ ] Background-job changes are idempotent where practical.

## Commit style

Prefer small, descriptive commits such as:

```text
feat: add carrier pricing validation
fix: prevent duplicate daily carrier reports
docs: clarify local SQL Server setup
```
