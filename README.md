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