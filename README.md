# Campus Equipment Borrowing System

ITSD 81 – Desktop Application Development

An Avalonia desktop application for browsing campus equipment, requesting borrowings, and recording returns. Entity Framework Core and SQLite provide persistent storage.

Repository: https://github.com/nestorbenitezx10-tech/EquipmentBorrowing

## Solution structure

| Project | Responsibility |
| --- | --- |
| src/EquipmentBorrowing.Domain | Student, Equipment, Borrowing, and status types. |
| src/EquipmentBorrowing.Application | Use cases and repository interfaces. |
| src/EquipmentBorrowing.Infrastructure | EF Core SQLite context, configurations, migrations, and repositories. |
| src/EquipmentBorrowing.Desktop | Avalonia UI, view models, and app startup. |
| tests/EquipmentBorrowing.Tests | Automated tests. |

## Build, test, and run

Run commands from the repository root with the .NET 8 SDK. Explicit project commands work on SDKs that do not support the .slnx solution format.

    dotnet restore src\EquipmentBorrowing.Desktop\EquipmentBorrowing.Desktop.csproj
    dotnet build src\EquipmentBorrowing.Desktop\EquipmentBorrowing.Desktop.csproj --configuration Release
    dotnet test tests\EquipmentBorrowing.Tests\EquipmentBorrowing.Tests.csproj --configuration Release
    dotnet run --project src\EquipmentBorrowing.Desktop\EquipmentBorrowing.Desktop.csproj --configuration Release

Start the desktop app from the repository root. It applies pending EF Core migrations and uses equipment_borrowing.db in the current working directory. If the Equipment table is empty, it seeds students STU-001, STU-002, STU-999 (inactive) and equipment EQ-101, EQ-102, EQ-103.

### Borrow equipment

1. Open Equipment.
2. Enter an active student ID, such as STU-001, and the borrowing duration.
3. Select an available equipment row.
4. Click Borrow selected equipment.
5. Open Active Borrowings to view the saved request.

### Return equipment

1. Open Active Borrowings.
2. Select the borrowing row and click Return selected borrowing.
3. Confirm the success message and that the equipment is available again.

To verify persistence, close and restart the app from the same directory. Check Active Borrowings and the SQLite Borrowings table.

## SQLite and EF Core

- Provider project: src/EquipmentBorrowing.Infrastructure/EquipmentBorrowing.Infrastructure.csproj
- DbContext: src/EquipmentBorrowing.Infrastructure/Persistence/EquipmentBorrowingDbContext.cs
- Entity configurations: src/EquipmentBorrowing.Infrastructure/Persistence/Configuration/
- Design-time factory: src/EquipmentBorrowing.Infrastructure/Persistence/DbContextFactory.cs
- Initial migration and model snapshot: src/EquipmentBorrowing.Infrastructure/Persistence/Migrations/
- Database-backed repositories: src/EquipmentBorrowing.Infrastructure/Repositories/
- SQL statements: docs/database-queries.sql
- Logical relational diagram: docs/database-schema.md

Tables include Students, Equipment, Borrowings, and EF Core's __EFMigrationsHistory. Borrowing status is an integer enum: Requested=0, Approved=1, Borrowed=2, Returned=3, Overdue=4. The current migration does not enforce foreign-key constraints between borrowing IDs and the other tables.

The SQL file includes an example UPDATE that marks EQ-101 unavailable. Run it only if that change is intended.

## Laboratory Activity 3 submission checklist

| # | Requirement | Artifact or evidence |
|---|---|---|
| 1 | Link or compressed copy of complete Git repository | This repository or a ZIP copy. |
| 2 | Complete .NET solution that builds successfully | EquipmentBorrowing.slnx; capture successful Release build output. |
| 3 | SQLite database using EF Core | Provider, DbContext, migrations, and repositories in Infrastructure. |
| 4 | Relational database diagram | docs/database-schema.md; logical relationships, no enforced foreign keys in current migration. |
| 5 | Required SQL statements | docs/database-queries.sql. |
| 6 | EF Core DbContext and entity configurations | DbContext and Persistence/Configuration/. |
| 7 | Initial EF Core migration | Persistence/Migrations/ with initial migration and model snapshot. |
| 8 | Database-backed repositories | EF repository implementations under Infrastructure/Repositories/. |
| 9 | Working Avalonia application with persistent data | Desktop project; capture borrow, restart persistence, and return workflows. |
| 10 | Updated README | This file. |
| 11 | Screenshot of database tables | Capture SQLite viewer showing Students, Equipment, and Borrowings. |
| 12 | Screenshot of stored Student, Equipment, and Borrowing data | Capture actual rows in the SQLite viewer. |
| 13 | Borrow, restart, and return screenshots | Capture successful app operations and matching database records. |
| 14 | Two inspected EF Core generated SQL queries | Capture actual EF Core SQL logging output; the SQL file alone is not proof. |
| 15 | Successful build and Git history | Capture successful build output and actual repository history. |

Capture genuine screenshots from the running application, SQLite viewer, build output, and EF Core logs. Do not claim evidence that has not been captured and verified. Do not publish a database containing personal or sensitive information.

## Architecture

Avalonia View → ViewModel → Application service → Repository interface → EF Core repository → EquipmentBorrowingDbContext → SQLite
