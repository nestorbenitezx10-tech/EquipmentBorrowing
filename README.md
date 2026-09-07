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

# Laboratory Activity 2 – Avalonia UI and MVVM

## 1. Desktop Project

The `EquipmentBorrowing.Desktop` project is responsible for the presentation layer of the system.  
It contains:

- Avalonia Views (XAML)
- ViewModels
- Application startup and Dependency Injection configuration

This project interacts with the existing layers as follows:

- It references `EquipmentBorrowing.Application` to use the application services (`BorrowEquipmentService` and `ReturnEquipmentService`).
- It references `EquipmentBorrowing.Infrastructure` to register the in-memory repositories.
- It does **not** contain any business rules or domain logic.

---

## 2. Updated Architecture
Avalonia View (XAML)
│
│ Binding / Command
▼
ViewModel (CommunityToolkit.Mvvm)
│
│ Application Operation
▼
Application Service
│
├──────────► Domain
│
▼
Repository Interface
▲
│
Infrastructure Implementation (In-Memory)
text---

## 3. Borrow Equipment Flow

1. User selects equipment and enters Student ID in the **Equipment** view.
2. User clicks the **Borrow Equipment** button.
3. The button triggers a `RelayCommand` in `EquipmentViewModel`.
4. The ViewModel calls `BorrowEquipmentService`.
5. The Application Service validates the request using Domain rules and repositories.
6. The result (success or failure message) is returned to the ViewModel.
7. The ViewModel updates the `StatusMessage` property.
8. The View displays the message to the user through data binding.

---

## 4. Return Equipment Flow

1. User navigates to the **Active Borrowings** view.
2. User clicks the **Return Equipment** button on a specific borrowing.
3. The button triggers the `ReturnCommand` in `BorrowingsViewModel`.
4. The ViewModel calls `ReturnEquipmentService`.
5. The service updates the borrowing status and makes the equipment available again.
6. The result is returned to the ViewModel.
7. The list of active borrowings is refreshed.
8. The success or failure message is shown to the user.

---

## 5. Architectural Reflection

1. **Why should the View not call a repository directly?**  
   Because the View should only handle presentation. Calling a repository directly would break separation of concerns and make the UI dependent on data access technology.

2. **Why should business rules not be implemented in the ViewModel?**  
   Business rules belong to the Domain or Application layer. Putting them in the ViewModel would make the rules harder to reuse and test, and would mix presentation logic with business logic.

3. **What is the responsibility of the ViewModel?**  
   The ViewModel is responsible for presentation state, user input, commands, and calling application services. It acts as a bridge between the View and the Application layer.

4. **Why can the existing Application layer work without knowing that Avalonia is being used?**  
   Because the Application layer only depends on Domain and repository interfaces. It has no reference to Avalonia, so it remains independent of the UI technology.

5. **What advantage is gained from registering dependencies in one composition point?**  
   It makes the dependency graph clear and centralized. Changing implementations (for example, replacing in-memory repositories later) can be done in one place without modifying ViewModels or services.

6. **If the in-memory repository were replaced by SQLite later, which parts of the current interface should remain largely unchanged?**  
   The Views, ViewModels, Application Services, and Domain models should remain largely unchanged. Only a new rep
   