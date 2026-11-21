# Campus Equipment Borrowing System

ITSD 81 — Desktop Application Development, Laboratory Activity 4

An Avalonia desktop application for laboratory staff to manage students and
equipment, check out and return items, coordinate multi-item checkouts, review
history, and search borrowing records. PostgreSQL is the operational database.

## Requirements

- .NET 8 SDK
- PostgreSQL (local installation is sufficient)
- `dotnet-ef` 8.0.11 for migration commands

## Configure PostgreSQL

Create an application role and the application and test databases using
pgAdmin or `psql`:

```sql
CREATE ROLE campus_app WITH LOGIN PASSWORD 'choose-a-local-password';
CREATE DATABASE campus_equipment OWNER campus_app;
CREATE DATABASE campus_equipment_test OWNER campus_app;
```

Copy `src/EquipmentBorrowing.Desktop/appsettings.json` to
`src/EquipmentBorrowing.Desktop/appsettings.Local.json` and set
`ConnectionStrings:CampusDb` to the local connection. The local file is
gitignored; never commit a database password. Alternatively set
`ConnectionStrings__CampusDb` in the process environment.

## Build, test, migrate, and run

Run these commands from the directory containing `EquipmentBorrowing.slnx`:

```powershell
dotnet restore
dotnet build src\EquipmentBorrowing.Desktop\EquipmentBorrowing.Desktop.csproj
dotnet test tests\EquipmentBorrowing.Tests\EquipmentBorrowing.Tests.csproj
dotnet tool install --global dotnet-ef --version 8.0.11
```

If `dotnet-ef` is already installed at another version, use
`dotnet tool update --global dotnet-ef --version 8.0.11` instead.

The migration `InitialPostgresCreate` creates the core tables and
`EnforceIdentifierIntegrity` adds required-text and normalized-identifier
constraints. This sets the environment variable only for the current
PowerShell session; replace the sample password locally:

```powershell
$env:ConnectionStrings__CampusDb = "Host=localhost;Port=5432;Database=campus_equipment;Username=campus_app;Password=YOUR_LOCAL_PASSWORD"
dotnet ef database update --project src\EquipmentBorrowing.Infrastructure\EquipmentBorrowing.Infrastructure.csproj
dotnet run --project src\EquipmentBorrowing.Desktop\EquipmentBorrowing.Desktop.csproj
```

The desktop app also applies migrations on startup. It seeds sample records
only when both student and equipment tables are empty; it does not reseed or
overwrite an existing database on every launch.

For readable SQL command logging in the terminal, set
`EQUIPMENTBORROWING_LOG_SQL=true` before launching. SQL logging is off by
default to keep normal startup output concise.

## Workflows

- **Equipment:** view all or available equipment, register and edit descriptive
  fields, view an item's active and returned history, or check out one item.
- **Multi-item checkout:** select an eligible student, at least two available
  items, and one expected return date. The transaction is atomic.
- **Students:** search, register, update, permit/restrict borrowing, and inspect
  each student's full history.
- **Borrowings:** view active/all/returned records, search by student name,
  student number, or equipment name, sort by checkout/expected-return date,
  clear criteria, and return an active item.

Seed examples include students `STU-001`, `STU-002`, and restricted `STU-999`,
and equipment `EQ-101` through `EQ-103`.

## Architecture and integrity

```text
Avalonia View → MVVM ViewModel → Application service
              → repository interface → EF Core → PostgreSQL
```

Views/ViewModels do not access the database or run SQL. Repositories use
`IDbContextFactory` to create short-lived contexts for asynchronous operations.
Checkout revalidates the student, current active count, equipment references,
availability, and dates inside a serializable transaction. The per-student
active-loan limit is three. PostgreSQL foreign keys, check constraints,
normalized unique indexes, and a partial unique index prevent invalid
relationships and concurrent duplicate active loans. A known write failure
rolls back; a commit with an unknown outcome is reported as unknown and is not
automatically retried.

See [docs/lab4-use-cases.md](docs/lab4-use-cases.md) for the updated diagram,
catalog, fourteen required use cases, and traceability. See
[docs/lab4-verification.md](docs/lab4-verification.md) for reproducible checks
and the acceptance-evidence checklist. PostgreSQL schema and inspection
queries are in [docs/database-schema.md](docs/database-schema.md) and
[docs/database-queries.sql](docs/database-queries.sql).

## Submission note

Each student submits individually. Add both partners' full names and student
IDs to the submission comment (or include your own if working alone); these
personal details are intentionally not invented or stored in this repository.

## Exporting a submission copy

After committing the final changes, run the following in PowerShell from the
repository root to create a source ZIP, a Git bundle containing repository
history, and a readable commit list in the parent folder:

```powershell
$out = Split-Path -Parent (Get-Location)
git archive --format=zip "--output=$(Join-Path $out 'EquipmentBorrowing-source.zip')" HEAD
git bundle create (Join-Path $out 'EquipmentBorrowing-history.bundle') --all
git log --graph --decorate --oneline --all |
    Set-Content -Encoding utf8 (Join-Path $out 'EquipmentBorrowing-history.txt')
git bundle verify (Join-Path $out 'EquipmentBorrowing-history.bundle')
```

`git archive` exports tracked files from `HEAD`, so ignored local configuration
such as `appsettings.Local.json` is not included. Do not ZIP the entire working
folder or submit database credentials. Keep the ZIP, bundle, and history list
with the individual course submission rather than committing them to this
repository.
