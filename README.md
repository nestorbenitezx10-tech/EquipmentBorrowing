# Campus Equipment Borrowing System

**Course:** ITSD 81 — Desktop Application Development  
**Project:** Campus Equipment Borrowing System  
**Contributor:** NESTOR S. BENITEZ — Student ID `2024302407`

This repository documents the project’s development from Laboratory Activity 1 through Laboratory Activity 5. It grows from a layered borrowing workflow into an Avalonia desktop application, adds database persistence and expanded equipment desk workflows, then connects the desktop client to a PostgreSQL-backed API. The activity descriptions below are specific to this repository; later activities may differ from examples in other students’ projects.

## Project progression

### Laboratory Activity 1 — Application structure and domain

Established the borrowing system’s layered foundation: Domain, Application, Infrastructure, tests, and a console demonstration. The activity modeled students, equipment, and borrowings; defined repository interfaces and application services; and kept business rules independent of storage and user-interface technologies.

The intended dependency direction is inward: Application depends on Domain; Infrastructure implements Application’s repository interfaces; and the demonstration and tests use those layers. The initial repositories use in-memory collections.

### Laboratory Activity 2 — Avalonia UI and MVVM

Added an Avalonia desktop interface using Views, ViewModels, bindings, and commands. The UI calls application services for borrowing and returning equipment, while dependency setup is centralized in the application startup code.

The Views handle presentation, ViewModels expose screen state and commands, and application services coordinate borrowing rules. This keeps UI controls out of the domain and application layers.

### Laboratory Activity 3 — SQLite and Entity Framework Core

Replaced the in-memory repositories with Entity Framework Core and SQLite persistence. The domain and application layers continue to use repository abstractions, while the Infrastructure layer handles database access and migrations.

The database stores students, equipment, and borrowing records with relationships and integrity rules. Repository implementations translate application requests into EF Core queries and updates; the UI does not query the database directly.

### Laboratory Activity 4 — Equipment desk workflows

Expanded the desktop application with equipment and student management, eligibility controls, single and multi-item checkout, returns, borrowing histories, and borrowing search/filter/sort. This activity’s retained application uses SQLite. Use cases, schema notes, screenshots, and verification status are available in:

- [Lab 4 use cases](docs/lab4-use-cases.md)
- [Lab 4 verification record](docs/lab4-verification.md)
- [Lab 4 screenshots](docs/screenshots/New%20folder/)

The verification record distinguishes checks with evidence from items that still need retesting. It should be used as the source of truth for Lab 4 acceptance status.

### Laboratory Activity 5 — PostgreSQL API integration

Added an ASP.NET Core API backed by PostgreSQL and connected the Avalonia desktop client through HTTP. The API provides versioned endpoints for equipment, students, and borrowings; the desktop uses HTTP repositories and does not connect directly to the database. The API applies its PostgreSQL schema at startup.

Lab 5 extends the Lab 4 workflows through an API boundary. The API and desktop can be started separately, and the traceability and acceptance documents describe endpoint coverage and observed test status.

- [Lab 5 API traceability](docs/lab5-traceability.md)
- [Lab 5 acceptance record](docs/lab5-acceptance-record.md)
- [Lab 5 screenshots](docs/screenshots/lab5/)

## Current architecture

```text
Avalonia Views → ViewModels → HTTP repositories → ASP.NET Core API
                                                  → Application services
                                                  → EF Core → PostgreSQL
```

The repository also retains the SQLite-based desktop and Infrastructure implementation used for Lab 4. The Lab 5 API is a separate host; its desktop client communicates with it over HTTP.

## Requirements

- .NET 8 SDK/runtime
- PostgreSQL for the Lab 5 API

## Build and run

Run commands from the repository root. To restore and build the solution:

```powershell
dotnet restore
dotnet build EquipmentBorrowing.slnx
```

To run the SQLite desktop application:

```powershell
dotnet run --project src/EquipmentBorrowing.Desktop/EquipmentBorrowing.Desktop.csproj
```

To run the Lab 5 PostgreSQL API and desktop client, use the included scripts:

```powershell
.\scripts\start-api.ps1
```

In a second terminal:

```powershell
.\scripts\start-desktop.ps1
```

The API defaults to `http://localhost:5090/`. It reads its PostgreSQL connection settings from the environment and prompts securely for the database password when started with the script. For disposable Lab 5 acceptance data and the controlled database reset procedure, see the [Lab 5 acceptance record](docs/lab5-acceptance-record.md) before running any SQL scripts.

## Record IDs

In the Lab 5 PostgreSQL API, new student records use IDs such as `ST-01122`, equipment uses its equipment code such as `EQ-102`, and borrowings use IDs such as `BR-00001`. A student’s school number remains a separate value. Existing IDs are preserved so borrowing history and foreign-key links remain intact.

## Screenshots

Browse the [Lab 4 desktop screenshots](docs/screenshots/New%20folder/) and [Lab 5 API screenshots](docs/screenshots/lab5/). Additional screenshots from the project folder are in [docs/screenshots](docs/screenshots/).
