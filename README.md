# Campus Equipment Borrowing System

**ITSD 81 – Desktop Application Development**  
**Laboratory Activity 1: From Requirements to Application Structure**

This solution provides the architectural foundation for a Campus Equipment Borrowing System.  
It focuses on clean separation of concerns, domain modeling, repository abstractions, and application services — without any user interface or database yet.

---

## 1. Solution Structure

The solution follows a layered architecture:

| Project | Purpose |
|---------|---------|
| **EquipmentBorrowing.Domain** | Contains the core business concepts and rules of the problem domain. It has no dependencies on other projects. |
| **EquipmentBorrowing.Application** | Contains application services (use cases) and repository interfaces. It coordinates domain objects and defines what the system can do. |
| **EquipmentBorrowing.Infrastructure** | Contains concrete implementations of the repository interfaces (currently in-memory). This layer handles technical details. |
| **EquipmentBorrowing.Tests** | Contains automated tests for domain and application behavior. |
| **EquipmentBorrowing.Demo** | A simple console application used to demonstrate successful and failed borrowing scenarios. |

This separation ensures that business rules remain independent of UI technology and data storage technology.

---

## 2. Dependency Direction

The dependency flow is designed so that inner layers do not know about outer layers:

**Explanation:**
- `Application` depends only on `Domain`.
- `Infrastructure` depends on both `Domain` and `Application`.
- The Demo / future UI depends on `Application` and `Infrastructure`.
- `Domain` has **zero** dependencies on other projects.

This follows the Dependency Inversion Principle.

---

## 3. Use Case Mapping

**Actor:** Student  

**Use Case:** Borrow Equipment  

**Application Service:** `BorrowEquipmentService`  

**Domain Objects Used:**
- `Student`
- `Equipment`
- `Borrowing`
- `BorrowingStatus`

**Repository Interfaces Used:**
- `IStudentRepository`
- `IEquipmentRepository`
- `IBorrowingRepository`

**Infrastructure Implementations Used:**
- `InMemoryStudentRepository`
- `InMemoryEquipmentRepository`
- `InMemoryBorrowingRepository`

---

## 4. Reflection

1. **Why should the application service depend on a repository interface instead of directly depending on a database implementation?**  
   Depending on an interface keeps the application layer independent of any specific storage technology. This makes the system easier to test, maintain, and change later without modifying the business logic.

2. **Which parts of your current solution could remain unchanged if SQLite were added later?**  
   The Domain project, the Application project, and the Demo project would remain unchanged. Only a new repository implementation would be added in the Infrastructure project.

3. **Which project would eventually contain Avalonia Views?**  
   A new project (for example `EquipmentBorrowing.UI`) would contain the Avalonia Views.

4. **Should an Avalonia button directly execute database queries? Why or why not?**  
   No. An Avalonia button should only call an application service. Doing database queries directly from the UI would violate separation of concerns.

5. **What part of your implementation represents the actual business operation requested by the actor?**  
   The `BorrowEquipmentService` represents the actual business operation requested by the student.

---

## How to Run the Demonstration

```bash
dotnet build
dotnet run --project src/EquipmentBorrowing.Demo
```

## Part L: Updated README

### 1. Desktop Project

`EquipmentBorrowing.Desktop` is the Avalonia desktop application. It provides the
views, navigation, user input, and feedback for the equipment borrowing workflow.
`App.axaml.cs` is the composition point: it creates the in-memory repositories,
seeds sample data, and injects those repositories into `MainWindowViewModel`.

The desktop project references `EquipmentBorrowing.Application` for use-case
services and `EquipmentBorrowing.Infrastructure` for the in-memory repository
implementations. It does not access repository implementations from the view.

### 2. Updated Architecture

```text
Avalonia View
      |
      | Binding / Command
      v
ViewModel
      |
      | Application Operation
      v
Application Service
      |
      +----------> Domain
      |
      v
Repository Interface
      ^
      |
Infrastructure Implementation
```

The main window switches between the Equipment and Active Borrowings sections
through view-model state. Observable collections are refreshed after borrowing
or returning equipment, so both sections show the current application state.

### 3. Borrow Equipment Flow

1. The user enters a student ID and borrowing duration, then selects Borrow Equipment.
2. The Equipment view command calls `EquipmentViewModel.BorrowEquipmentAsync`.
3. The equipment view model calls `BorrowEquipmentService.RequestBorrowingAsync`.
4. The application service validates the student and equipment, creates a requested
   `Borrowing`, saves it, and marks the equipment unavailable.
5. The view model refreshes both collections and displays a success or error message.

### 4. Return Equipment Flow

1. The user opens Active Borrowings and selects Return Equipment.
2. The Borrowings view command calls `BorrowingsViewModel.ReturnEquipmentAsync`.
3. The borrowings view model calls `ReturnEquipmentService.ReturnAsync`.
4. The application service marks the borrowing returned and makes its equipment
   available again through the repository interfaces.
5. The view model refreshes the equipment and active-borrowing lists and displays
   the operation result.

### 5. Architectural Reflection

1. **Why should the View not call a repository directly?**  The View should only
   handle presentation and user interaction. Calling a repository directly would
   couple the UI to storage details and bypass application business rules.
2. **Why should business rules not be implemented in the ViewModel?**  Business
   rules belong in the application or domain layers so they can be reused and
   tested without an Avalonia UI.
3. **What is the responsibility of the ViewModel?**  It exposes presentation state,
   accepts commands from the View, invokes application services, and refreshes
   the data displayed by the View.
4. **Why can the existing Application layer work without knowing that Avalonia is
   being used?**  It depends on domain entities and repository interfaces, not on
   UI controls or Avalonia types.
5. **What advantage is gained from registering dependencies in one composition
   point?**  Object creation and wiring are centralized, making dependencies
   explicit and making it easier to replace implementations later.
6. **If the in-memory repository were replaced by SQLite later, which parts of the
   current interface should remain largely unchanged?**  The View, ViewModel,
   application services, domain entities, and repository interfaces should remain
   largely unchanged. Only the infrastructure implementations and composition
   registration would need to change.

---

# Laboratory Activity 2 - Avalonia UI & MVVM Integration

## 1. Desktop Project Responsibility

The `EquipmentBorrowing.Desktop` project functions entirely as the system
presentation layer. It manages visual assets, window layout rendering, user
interactions, and feedback messages.

It consumes the underlying layers by collecting input from the interface,
passing it to ViewModel commands, and displaying the results without changing
the underlying business workflows.

## 2. Updated Architecture Diagram

The system dependencies flow inward from the interface to the domain core:

```text
   [ Avalonia UI View ] (XAML Windows / Layouts)
         |
         | (Data Binding / Relay Commands)
         v
    [ ViewModel Layer ] (Presentation State / Selections)
         |
         | (Asynchronous Use Case Operations)
         v
  [ Application Service ] (Orchestration & Validation)
         |
     +-----+-----+
     v           v
  [ Domain ]  [ Repository Interfaces ]
                 ^
                 |
        [ Infrastructure Memory Store ]
```

## 3. Use Case Data Flows

### A. Borrow Equipment Flow

1. **User Action:** The student fills in the form fields and clicks the Borrow Equipment button.
2. **View to ViewModel:** The click invokes an asynchronous RelayCommand in the matching ViewModel.
3. **ViewModel Validation:** The ViewModel provides presentation values such as the student ID and borrowing duration.
4. **Application Logic Execution:** The ViewModel passes those values to `BorrowEquipmentService`.
5. **Domain and Repository Checks:** The service validates the student and checks current equipment availability.
6. **State Persistence:** The service saves the borrowing and marks the equipment unavailable.
7. **UI Notification:** The ViewModel refreshes its observable collections and displays a success or error message.

### B. Return Equipment Flow

1. **User Action:** The user selects an active borrowing and clicks the Return Equipment button.
2. **View to ViewModel:** The command receives the selected borrowing record and its unique ID.
3. **Application Layer Call:** The ViewModel passes the record ID to `ReturnEquipmentService`.
4. **Storage Evaluation:** The service finds the record, confirms it has not already been returned, changes its status to returned, and makes the equipment available.
5. **Data View Synchronization:** The ViewModel refreshes the observable collections, removing the returned item from the active list.

## 4. Architectural Reflection Answers

### 1. Why should the View not call a repository directly?

If the View queried database layers directly, the user interface would be tightly
coupled to storage choices. A database change could then break the UI and violate
separation of concerns.

### 2. Why should business rules not be implemented in the ViewModel?

ViewModels manage presentation state, such as selected records and form values.
Business rules belong in the application or domain layers so they can be shared
and tested independently of Avalonia.

### 3. What is the responsibility of the ViewModel?

The ViewModel converts application data into observable presentation state,
receives commands from the XAML View, calls application services, and refreshes
the data displayed by the View.

### 4. Why can the existing Application layer work without knowing that Avalonia is being used?

The Application layer depends on plain C# domain models and repository interfaces.
It does not depend on Avalonia controls, so it can be called by a desktop app,
console app, or automated tests.

### 5. What advantage is gained from registering dependencies in one composition point?

Centralizing dependency configuration makes object creation and application
lifecycle management explicit. It also makes replacing the in-memory stores with
production implementations easier because the wiring is changed in one place.

### 6. If the in-memory repository were replaced by SQLite later, which parts of the current interface should remain largely unchanged?

The XAML Views, styles, ViewModels, application services, domain entities, and
repository interfaces should remain largely unchanged. The SQLite repository
implementations and composition registration would need to be added or updated.

   