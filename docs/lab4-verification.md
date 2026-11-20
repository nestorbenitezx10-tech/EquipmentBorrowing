# Laboratory Activity 4 — verification record

Use a disposable PostgreSQL database for deliberate failure/constraint tests.
For each acceptance scenario, record setup, action, expected result, observed
result, and evidence. A screenshot alone does not establish persistence,
atomicity, or architecture. Verify database state through a fresh connection
after each write.

## Reproducible checks

From the solution directory:

```powershell
dotnet build src\EquipmentBorrowing.Desktop\EquipmentBorrowing.Desktop.csproj
dotnet test tests\EquipmentBorrowing.Tests\EquipmentBorrowing.Tests.csproj
dotnet run --project src\EquipmentBorrowing.Desktop\EquipmentBorrowing.Desktop.csproj
```

The current automated tests cover valid single checkout, multi-item minimum,
duplicate and date validation, single-operation delegation, and combined
case-insensitive borrowing search/status/sort. They do not replace the live
PostgreSQL acceptance scenarios below.

## Workspace run recorded

- Desktop build: passed with no warnings.
- `dotnet test EquipmentBorrowing\EquipmentBorrowing.slnx`: 6 passed, 0 failed.
- Desktop launch: the `Campus Equipment Borrowing` window opened. Startup
  completed migration/initialization and displayed the three catalog seed
  items (`EQ-101`–`EQ-103`).
- A follow-up manual run used the user-confirmed disposable test database.
  The results below are observed in the application; screenshots and direct
  SQL snapshots were not saved.
- The final checkout-form layout adjustment was rebuilt and the app reopened;
  the student selector, date picker, and borrow button are visible after the
  catalog without scrolling to the bottom of the view.

## Acceptance evidence checklist

| Evidence | Scenario / expected state | Observed result and evidence to attach |
|---|---|---|
| A01 / UC-01 | Catalog reflects PostgreSQL; empty and connection-error states are distinct | App displayed EQ-101–EQ-103 at startup; direct DB comparison and empty/error states not captured |
| A02 / UC-02 | Available-only excludes unavailable items; no-match/error states differ | Multi-checkout list showed only EQ-103 available after EQ-101/EQ-102 were checked out; catalog's Available only button and no-match/error states not captured |
| A03 / UC-03 | Valid single issue creates one active row and unavailable item; invalid/ineligible/missing/over-limit/date requests write nothing | Not tested |
| A04 / UC-04 | Active list includes related student/equipment and excludes returned rows | Active list showed two STU-001 records for EQ-101/EQ-102; EQ-101 disappeared from active list after return |
| A05 / UC-05 | Same row is returned with actual date, item available; repeat/stale return does not change data | EQ-101 returned; status/date and subsequent item/student history showed Returned. Repeat/stale return not tested |
| A06 / UC-06 | Two selected items commit together; each rejection and a post-write controlled failure leaves no partial change | STU-001 checked out EQ-101 and EQ-102 together; two active rows appeared. Rejection/controlled-failure rollback not tested; EQ-102 remains active in the test DB |
| A07 / UC-07 | Student histories contain only the selected student's active/returned rows; empty/error differ | STU-001 history showed EQ-101 Returned and EQ-102 Active; other-student, empty/error cases not tested |
| A08 / UC-08 | Equipment histories contain only the selected item's active/returned rows; empty/error differ | EQ-101 history showed STU-001 and its Returned record; other-item, empty/error cases not tested |
| A09 / UC-09 | Search each field, all status values, both sort fields/directions, combine/reset, no-match/error | Partial search `Nicolette` returned matching rows; Active and Returned filters worked. Expected-return sort was selected; remaining combinations/directions/reset/no-match/error not tested |
| A12 / UC-12 | Registration persists eligible student; blank and case/whitespace duplicate values rejected | Registered QA-STU-20261121-0115; lower-case/space-padded duplicate was rejected. Blank-field case not tested |
| A13 / UC-13 | Edit same student identity with history; clear contact; reject blank/duplicate edits | Updated QA student number/name and cleared contact; row remained after app restart. History preservation and invalid edit cases not tested |
| A14 / UC-14 | Restriction blocks new issue but permits return of active loan; permission restores checkout | QA test student was restricted successfully; checkout rejection, existing-loan return while restricted, and re-permit not tested |
| A15 / UC-15 | Registration persists available item; blank and normalized duplicate rejected | Registered QA-EQ-20261121-0115; lower-case/space-padded duplicate was rejected. Blank-field case not tested |
| A16 / UC-16 | Descriptive edit preserves identity, availability, and history; blank/duplicate fields rejected | Updated QA item name and cleared description; it remained available after app restart. History preservation and other invalid edits not tested |
| A17 / Cross-cutting | Clean migration creates keys, foreign keys, checks, unique and partial active-loan index | Startup migration completed; direct `__EFMigrationsHistory` and schema/index inspection not captured |
| A18 / Cross-cutting | Safe failure has clear feedback, no false success/partial known-failure change; retry works after recovery | Not captured yet |
| A19 / Submission | Diagram, suite, matrix, README, source, migration and Git history agree | Source/docs present; inspect Git history at submission and add partner identities |

For A06, enable `Testing:FailBeforeCommit` only when connecting to
`campus_equipment_test`. The configured fault injector throws after writes are
sent but before transaction commit. Compare before/after state using a fresh
database connection; never enable it against a shared or production database.

For A17, inspect `__EFMigrationsHistory` and the constraints/indexes described
in [database-schema.md](database-schema.md). For A18, disconnect only while
using disposable data or a controlled test seam. A failure during commit can
be an unknown outcome; reconnect and inspect before deciding whether to retry.

## Architecture checks

- The Avalonia Views bind to ViewModels and do not contain SQL or configure
  connections.
- ViewModels call application services; services call repository abstractions.
- EF repositories create/dispose a short-lived context per operation from
  `IDbContextFactory`.
- Multi-item checkout revalidates inside a serializable transaction; PostgreSQL
  constraints provide the final concurrency protection.
- Borrowing history remains linked through student/equipment edits and returns;
  hard deletion is not part of this activity.
