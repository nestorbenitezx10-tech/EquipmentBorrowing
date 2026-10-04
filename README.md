# Campus Equipment Borrowing System

**ITSD 81 – Desktop Application Development**  
**Laboratory Activity 1: From Requirements to Application Structure**

This solution provides the architectural foundation for a Campus Equipment Borrowing System. It focuses on clean separation of concerns, domain modeling, repository abstractions, and application services — without any user interface or database yet.

---

## 1. Solution Structure

The solution follows a layered architecture:

| Project | Purpose |
|---------|---------|
| EquipmentBorrowing.Domain | Contains the core business concepts and rules of the problem domain. It has no dependencies on other projects. |
| EquipmentBorrowing.Application | Contains application services and repository interfaces. It coordinates domain objects and defines what the system can do. |
| EquipmentBorrowing.Infrastructure | Contains concrete implementations of repository interfaces. |
| EquipmentBorrowing.Tests | Contains automated tests for domain and application behavior. |
| EquipmentBorrowing.Demo | A simple console application used to demonstrate borrowing scenarios. |

This separation keeps business rules independent of UI and data-storage technology.

## 2. Dependency Direction

- Application depends only on Domain.
- Infrastructure depends on Domain and Application.
- The Demo and future UI depend on Application and Infrastructure.
- Domain has zero dependencies on other projects.

This follows the Dependency Inversion Principle.

## 3. Use Case Mapping

Actor: Student  
Use Case: Borrow Equipment  
Application Service: BorrowEquipmentService

Domain objects: Student, Equipment, Borrowing, BorrowingStatus.

Repository interfaces: IStudentRepository, IEquipmentRepository, IBorrowingRepository.

Initial infrastructure implementations: InMemoryStudentRepository, InMemoryEquipmentRepository, InMemoryBorrowingRepository.

## 4. Reflection

1. The application service depends on a repository interface to stay independent of any specific storage technology. This improves testing and allows storage to change without modifying business logic.
2. If SQLite is added, the Domain and Application projects should remain largely unchanged; Infrastructure gains database implementations.
3. Avalonia views belong in a UI or Desktop project.
4. An Avalonia button should call an application service rather than execute database queries directly, to preserve separation of concerns.
5. BorrowEquipmentService represents the business operation requested by the student.

## How to Run the Demonstration

    dotnet build
    dotnet run --project src/EquipmentBorrowing.Demo

## Part L: Updated README — Laboratory Activity 1

### 1. Desktop Project

EquipmentBorrowing.Desktop is the Avalonia desktop application. It provides views, navigation, user input, and feedback for equipment borrowing. App.axaml.cs is the composition point that creates repositories, seeds sample data, and injects dependencies into MainWindowViewModel.

The desktop project references EquipmentBorrowing.Application for use cases and EquipmentBorrowing.Infrastructure for repositories. Views do not access repositories directly.

### 2. Updated Architecture

Avalonia View → ViewModel → Application Service → Domain and Repository Interface → Infrastructure Implementation

The main window switches between Equipment and Active Borrowings. Observable collections refresh after borrow or return operations.

### 3. Borrow Equipment Flow

1. The user enters a student ID and borrowing duration, then selects Borrow Equipment.
2. The Equipment view command calls EquipmentViewModel.BorrowEquipmentAsync.
3. The view model calls BorrowEquipmentService.RequestBorrowingAsync.
4. The application service validates the student and equipment, creates and saves a requested Borrowing, and marks the equipment unavailable.
5. The view model refreshes both collections and displays success or error feedback.

### 4. Return Equipment Flow

1. The user opens Active Borrowings and selects Return Equipment.
2. The view command invokes the return command in BorrowingsViewModel.
3. The view model calls ReturnEquipmentService.ReturnAsync.
4. The service marks the borrowing returned and makes its equipment available again.
5. The view model refreshes the equipment and active-borrowing lists and displays the result.

### 5. Architectural Reflection

1. Views handle presentation; repositories should be accessed through application operations, not directly from the view.
2. Business rules belong in Domain or Application so they can be reused and tested independently from Avalonia.
3. A ViewModel exposes presentation state, handles commands, calls application services, and refreshes displayed data.
4. The Application layer is independent of Avalonia because it depends on domain entities and repository interfaces, not UI controls.
5. A composition point centralizes dependency creation and makes implementations easier to replace.
6. When storage changes, views, view models, application services, domain entities, and repository interfaces should remain largely unchanged; infrastructure and composition are updated.

---

# Laboratory Activity 2 – Avalonia UI and MVVM

## 1. Desktop Project

EquipmentBorrowing.Desktop is the presentation layer containing Avalonia Views (XAML), ViewModels, and application startup configuration. It references the Application and Infrastructure projects and does not contain domain business rules.

## 2. Updated Architecture

Avalonia View (XAML) → ViewModel (CommunityToolkit.Mvvm) → Application Service → Domain / Repository Interface → Infrastructure Implementation

## 3. Borrow Equipment Flow

1. User selects equipment and enters Student ID in the Equipment view.
2. User clicks Borrow Equipment.
3. A RelayCommand in EquipmentViewModel invokes BorrowEquipmentService.
4. The application service validates the request using domain rules and repositories.
5. The ViewModel updates presentation state and displays a success or failure message.

## 4. Return Equipment Flow

1. User opens Active Borrowings and selects a borrowing.
2. The return command in BorrowingsViewModel invokes ReturnEquipmentService.
3. The service updates borrowing status and makes the equipment available again.
4. The active list is refreshed and feedback is displayed.

## 5. Architectural Reflection

1. The View handles presentation; direct repository access couples it to storage and bypasses application rules.
2. Business rules belong in Domain or Application so they can be reused and tested without the UI.
3. The ViewModel exposes presentation state, accepts commands, and calls application services.
4. The Application layer is independent of Avalonia because it references domain types and repository interfaces rather than Avalonia controls.
5. Centralized composition makes dependencies explicit and implementations replaceable.
6. If storage changes, presentation, application services, domain types, and repository interfaces remain largely stable; Infrastructure and composition change.

---

# Laboratory Activity 3 – SQLite and EF Core

Activity 3 extends the system with persistent SQLite storage using Entity Framework Core and a database-backed Avalonia desktop application.

## 1. Build, test, and run

Run from the repository root with the .NET 8 SDK. Explicit project paths also work with SDKs that do not support the .slnx format:

    dotnet restore src/EquipmentBorrowing.Desktop/EquipmentBorrowing.Desktop.csproj
    dotnet build src/EquipmentBorrowing.Desktop/EquipmentBorrowing.Desktop.csproj --configuration Release
    dotnet test tests/EquipmentBorrowing.Tests/EquipmentBorrowing.Tests.csproj --configuration Release
    dotnet run --project src/EquipmentBorrowing.Desktop/EquipmentBorrowing.Desktop.csproj --configuration Release

Launch the app from the repository root. It applies pending EF Core migrations and uses equipment_borrowing.db in the current working directory. If the Equipment table is empty, it seeds sample students: STU-001 and STU-002 (active), STU-999 (inactive); sample equipment: EQ-101, EQ-102, and EQ-103.

## 2. Borrow and return workflow

1. Open Equipment, enter an active student ID and borrowing duration, and select an available equipment row.
2. Click Borrow selected equipment and verify feedback and the new row under Active Borrowings.
3. Close and restart the app from the same repository root; verify the same borrowing remains.
4. Select the borrowing and click Return selected borrowing; verify success feedback and the updated record.

## 3. EF Core and database artifacts

- SQLite provider, DbContext, and entity configurations: src/EquipmentBorrowing.Infrastructure/
- Initial migration and model snapshot: src/EquipmentBorrowing.Infrastructure/Persistence/Migrations/
- EF Core repository implementations: src/EquipmentBorrowing.Infrastructure/Repositories/
- Required SQL statements: docs/database-queries.sql
- Logical relational diagram: docs/database-schema.md

Borrowing status is stored as an integer enum: Requested=0, Approved=1, Borrowed=2, Returned=3, Overdue=4. The current migration does not enforce foreign-key constraints for borrowing IDs.

## 4. Activity 3 submission checklist

| # | Requirement | Artifact or evidence |
|---|---|---|
| 1 | Complete repository link or compressed copy | This repository or a ZIP copy. |
| 2 | Complete .NET solution that builds | Solution and successful Release build screenshot. |
| 3 | SQLite database implementation using EF Core | SQLite provider, DbContext, migrations, and repositories. |
| 4 | Relational database diagram | docs/database-schema.md. |
| 5 | database-queries.sql | docs/database-queries.sql. |
| 6 | EF Core DbContext and entity configurations | DbContext and entity configuration classes. |
| 7 | Initial EF Core migration | Initial migration and model snapshot. |
| 8 | Database-backed repositories | EF repository implementations. |
| 9 | Working Avalonia app using persistent data | Capture borrow, restart persistence, and return workflows. |
| 10 | Updated README | This file, preserving Activities 1 and 2 above. |
| 11 | Screenshot of database tables | Capture SQLite viewer showing Students, Equipment, and Borrowings. |
| 12 | Screenshot of stored Student, Equipment, and Borrowing data | Capture actual rows in SQLite viewer. |
| 13 | Borrowing, restart, and return screenshots | Capture actual successful app operations and matching database records. |
| 14 | Two inspected EF Core-generated SQL queries | Capture actual EF Core SQL logging output; the SQL file alone is not evidence. |
| 15 | Successful build and Git history | Capture actual successful build output and repository history. |

Attach genuine screenshots from the application, SQLite viewer, build output, and EF Core logs. Do not claim evidence that has not been captured and verified. Do not publish a database containing personal or sensitive information.
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
