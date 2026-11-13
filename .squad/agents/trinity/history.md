# Project Context

- **Owner:** Cyrille NDOUMBE
- **Project:** MedEasy management platform
- **Stack:** .NET, Aspire, microservices, web frontend
- **Created:** 2026-07-14T01:20:55+0000

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

📌 Team update (2026-08-16T00:00:00Z): agenda-api startup abort is caused by Npgsql's default `GssEncryptionMode=Prefer` on the Paramore.Brighter outbox path (`dlopen` of missing `libgssapi_krb5.so.2`), not by the base image. Migrator is safe via `configureSettings` (`Program.cs` L18) + self-contained publish (`Build.cs` L372); the API only covers the EF data source (`Program.cs` L46), leaving `ServiceCollectionExtensions.cs` L143 exposed. Fix is a shared `Agenda.ServiceDefaults` normalization helper applied to API/EF, API/Brighter and Migrator. — decided by Squad (Coordinator), from Morpheus and Trinity

📌 Team update (2026-08-16T00:00:00Z): the GSS helper shipped as `WithGssDisabled()` in `Agenda.DataStores.Postgres/NpgsqlConnectionStringExtensions.cs`, **not** in `Agenda.ServiceDefaults` — that project is storage-agnostic, still `net9.0` while consumers are `net10.0`, and would force Npgsql on every future service. `Agenda.DataStores.Postgres` is already referenced by API and Migrator and already pulled Npgsql transitively via `Paramore.Brighter.Outbox.PostgreSql`, so no new dependency surface. Applied at the three entry points, redundant `ConfigureDataSource` blocks removed, `Npgsql` pinned to `10.0.3` in `Directory.Packages.props`. API + Migrator build clean. Remaining: image rebuild and tag bump in `apphost.mts`. — decided by Trinity

📌 Team update (2026-09-15T10:58:34+0000): Centralized RFC 7807 Problem Details responses across all current Physiotrack HTTP error paths, with tests and changelog coverage. Dozer independently approved the result after adding exact assertions for `about:blank`, query-bearing `instance` values, and the `Allow` header; all reported validations passed.

📌 Team update (2026-09-15T11:09:30+0000): The approved Physiotrack RFC 7807 work was committed as `e2967586010332bd6c4deacde1b669ee61ed2876` using Conventional Commit title `feat(api): add RFC 7807 problem details` with a non-empty English body, pushed on `feature/support-problem-details`, and submitted as non-draft PR #14 targeting `develop`: https://github.com/candoumbe/physiotrack/pull/14. The working tree was clean after the push.
### 2026-10-28: Physiotrack local revision development
- Added source-only make migrate-create NAME=... / migrate-delete backed by existing migrator CLI/config; no DB access, no autogeneration, initial revision protected, graph ambiguity and symbolic/outside paths refused. Alembic template is included by existing Hatchling wheel rules; no packaging metadata change needed.
- GNU Make lesson: command-line recursive NAME can be expanded during automatic environment export for early $(shell ...) metadata computations, before target-specific assignments take effect. For migrate-create, freeze $(value NAME) at the top of the Makefile before any shell computation, then pass the literal through a quoted environment variable using --message=... (also supports leading dashes). Actual Make tests cover shell/Make injection sentinels, apostrophes, whitespace and multiline messages.
- Validation: 166 migrator tests passed including disposable PostgreSQL/Docker and wheel; 9 API tests passed. Migrator Ruff lint/format and scoped git diff --check pass. API lint passes; API formatting has four pre-existing failing files, untouched. Root global diff check reports a pre-existing trailing blank line in .squad/decisions.md, untouched. Both branches remain feature/physiotrack-migrator; existing modifications preserved, no commit/push.
