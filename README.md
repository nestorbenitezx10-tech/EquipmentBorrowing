# Campus Equipment Borrowing System

ITSD 81 — Desktop Application Development

An Avalonia desktop application for managing students, equipment, borrowing
requests, and returns. The Activity 4 baseline used Entity Framework Core with
SQLite. Activity 5 adds an ASP.NET Core API backed by PostgreSQL; the desktop
now accesses application data through HTTP.

## Contributor

- NESTOR S. BENITEZ — Student ID: 2024302407

## Active project location

The active Activity 4 projects, Activity 5 API, and tests are in the
`EquipmentBorrowing\` subdirectory and are included in the repository-root
solution:

- `EquipmentBorrowing\src\EquipmentBorrowing.Domain`
- `EquipmentBorrowing\src\EquipmentBorrowing.Application`
- `EquipmentBorrowing\src\EquipmentBorrowing.Infrastructure`
- `EquipmentBorrowing\src\EquipmentBorrowing.Desktop`
- `EquipmentBorrowing\tests\EquipmentBorrowing.Tests`

## Requirements

- .NET 8 SDK/runtime for the active desktop and API projects
- PostgreSQL for the Activity 5 API

## Record ID format

New PostgreSQL records use readable primary IDs: students use `ST-01122`-style
IDs, equipment uses its equipment code (for example `EQ-102`), and borrowings
use `BR-00001`-style IDs. The student's school number remains a separate value,
such as `2024302407`. Existing records keep their original IDs so borrowing
history and foreign-key links remain intact.

## Build, test, and run

Run these commands from the repository root:

```powershell
& "$env:ProgramFiles\dotnet\dotnet.exe" restore
& "$env:ProgramFiles\dotnet\dotnet.exe" build EquipmentBorrowing\src\EquipmentBorrowing.Api\EquipmentBorrowing.Api.csproj
```

The Activity 4 SQLite migration files remain in the retained infrastructure
project. The API applies the PostgreSQL baseline in
`EquipmentBorrowing\src\EquipmentBorrowing.Api\Migrations` at startup.

## Verification scope

The current automated suite covers two legacy borrowing-service cases and a
SQLite migration test that verifies data conversion and database integrity
constraints. It is not a full automated implementation of A01–A19. The
scenario-by-scenario status, including manual evidence and untested cases, is
maintained in [docs/lab4-verification.md](docs/lab4-verification.md).

## Architecture

```text
Avalonia View → ViewModel → HTTP client → ASP.NET Core API
                                  → application services → EF Core → PostgreSQL
```

The Domain project contains the borrowing model and rules. The API owns
business coordination and PostgreSQL persistence. Avalonia uses asynchronous
HTTP repositories and does not open a database connection.

## Laboratory Activity 5 API

The Activity 5 API host is `EquipmentBorrowing\src\EquipmentBorrowing.Api`. It serves the versioned endpoints independently of the desktop and publishes its OpenAPI document at `http://localhost:5090/openapi/v1.json`. The desktop reads `EQUIPMENTBORROWING_API_URL` (default `http://localhost:5090/`) and uses HTTP repositories; it does not open the database.

Use PostgreSQL for Activity 5. Create a **new disposable database** in pgAdmin for acceptance work; do not run the reset script against the existing `campus_equipment` database. From the repository root, start the API with the secure password prompt:

```powershell
& .\scripts\start-api.ps1
```

For the T08 reachable-server/database-error check only, stop the normal API and start the controlled missing-database scenario with `& .\scripts\start-api.ps1 -SimulateT08DatabaseError`. It uses a deliberately nonexistent database name; run the normal command again afterward to restore the application.

The API applies the idempotent PostgreSQL baseline script at startup. In a second terminal, start the desktop against the same port:

```powershell
& .\scripts\start-desktop.ps1
```

To reset the controlled T01–T08 data, open `scripts\lab5-reset-disposable.sql` in pgAdmin and run it only against the dedicated disposable Activity 5 database. It deletes the application rows before inserting the test dataset.

Implementation mapping is in [docs/lab5-traceability.md](docs/lab5-traceability.md). See [docs/lab5-acceptance-record.md](docs/lab5-acceptance-record.md) for execution evidence: T01–T06 are recorded as passing; T07 data loading is verified but live refresh remains pending, and T08 still needs desktop error/recovery verification.
