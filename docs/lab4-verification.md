# Laboratory Activity 4 — verification record

Contributor attribution supplied for this submission: **NESTOR S. BENITEZ**
(Student ID `2024302407`). The supplied names identify the same person, so the
contributor is listed once. No cover sheet or role template is present in the
retained project.

The retained application uses SQLite. Run the commands from the repository
root; do not treat evidence from the earlier PostgreSQL copy as verification
of this SQLite implementation.

## Reproducible checks

```powershell
dotnet build --configuration Release
dotnet test
dotnet run --project EquipmentBorrowing\src\EquipmentBorrowing.Desktop\EquipmentBorrowing.Desktop.csproj
```

`dotnet test` runs the root solution, which includes the retained projects and
test project under `EquipmentBorrowing\`. The current automated suite contains
two legacy borrowing-service tests and one SQLite migration/integrity test.
It is not a full automated implementation of A01–A19.

Final verification on 2026-11-21: the root-solution Release build succeeded;
`dotnet test` passed 3 tests with 0 failures. Restore still reports NU1903
high-severity vulnerability warnings for `Microsoft.Extensions.Caching.Memory`
8.0.0 and `SQLitePCLRaw.lib.e_sqlite3` 2.1.6.

## Acceptance evidence matrix

Status definitions: **Partial** means some but not all required checks have
evidence; **Not tested** means no current SQLite evidence was recorded;
**Deferred** means outside this activity's scope.

| ID / use case | Acceptance focus | Current status and evidence |
|---|---|---|
| A01 / UC-01 | SQLite catalog; empty and connection-error states differ | **Partial.** The current desktop screenshot displayed EQ-101–EQ-103. Direct database comparison and empty/error states were not captured. |
| A02 / UC-02 | Available-only excludes unavailable items; no-match/error differ | **Not tested.** Earlier observations were from the PostgreSQL copy; this filter and its empty/error cases need revalidation on SQLite. |
| A03 / UC-03 | Valid single issue; rejected requests write nothing | **Not tested.** Current UI checkout behavior has not been exercised against SQLite. |
| A04 / UC-04 | Active list includes related rows and excludes returned rows | **Not retested.** The earlier PostgreSQL observations are not evidence for the retained SQLite project. |
| A05 / UC-05 | Same row returned with actual date; repeat/stale return is safe | **Not retested.** Earlier PostgreSQL observations are not evidence for the retained SQLite project. |
| A06 / UC-06 | Multi-item checkout is atomic; rejections do not partially write | **Not tested.** The current controlled-failure rollback path and rejection cases need verification. |
| A07 / UC-07 | Only selected student's history; empty/error states differ | **Not retested** on SQLite. |
| A08 / UC-08 | Only selected equipment's history; empty/error states differ | **Not retested** on SQLite. |
| A09 / UC-09 | Search, status, sort directions, combine/reset, no-match/error | **Not retested** on SQLite. |
| A10 / UC-10 | View overdue borrowings | **Deferred** to Laboratory Activity 5. |
| A11 / UC-11 | View current equipment holder | **Deferred** to Laboratory Activity 5. |
| A12 / UC-12 | Register eligible student; reject blank/normalized duplicate values | **Not retested** on SQLite. |
| A13 / UC-13 | Edit student without losing identity/history; reject invalid edits | **Not retested** on SQLite. |
| A14 / UC-14 | Restriction blocks new issue but permits return; permission restores checkout | **Not retested** on SQLite. |
| A15 / UC-15 | Register available item; reject blank/normalized duplicates | **Not retested** on SQLite. |
| A16 / UC-16 | Descriptive item edit preserves identity, availability, and history | **Not retested** on SQLite. |
| A17 / UC-17 | Migration creates current schema and integrity rules | **Automated partial; passed.** `MigrationTests.Upgrade_PreservesLegacyRecordsAndMapsCurrentSchema` upgrades a temporary legacy SQLite database, verifies data/date/status conversion, and exercises foreign keys, checks, unique normalized identifiers, and the partial active-loan index. A separate inspection of the app database's `__EFMigrationsHistory` was not captured. |
| A18 / UC-18 | Safe failure, no partial write, useful feedback, and retry | **Not tested.** The controlled-failure/recovery walkthrough has not been run on the retained SQLite app. |
| A19 / UC-19 | README, diagram, matrix, tests, source, migrations, and history agree | **In progress.** Documentation and the root solution are aligned to SQLite; Git status/history inspection and commit are pending. |

## Schema and architecture checks

- Physical table names are `Students`, `Equipment`, and `Borrowings`; the
  diagram and rules are in [database-schema.md](database-schema.md).
- The retained migration sequence is
  `20261004084854_InitialCreate` then
  `20261121022649_AlignActivity4Schema`. PostgreSQL migrations
  `20261120145548_InitialPostgresCreate` and
  `20261120165014_EnforceIdentifierIntegrity` are not in the retained SQLite
  source tree.
- Unique normalized student/equipment identifiers, restrictive foreign keys,
  borrowing date/status checks, and the partial unique active-equipment index
  are defined by the SQLite model and migrations. The student limit of three
  active loans is checked in checkout logic, not by a database constraint.
- Views bind to ViewModels. ViewModels call application services; services use
  repository interfaces; EF Core repositories persist to SQLite.
- Multi-item checkout performs validation and writes in a serializable SQLite
  transaction. No `Testing:FailBeforeCommit` configuration is present in this
  retained source; do not claim a controlled-failure test until that seam is
  implemented and exercised.

The A01–A19 statuses above are evidence statuses, not claims that every
acceptance scenario has passed. Attach direct database and UI evidence as each
remaining scenario is verified.
