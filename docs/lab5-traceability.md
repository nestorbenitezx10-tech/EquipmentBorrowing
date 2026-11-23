# Activity 5 API traceability

This matrix maps Activity 5's Lab 4 workflows to the API implementation. The HTTP contract is published at `/openapi/v1.json` and in `EquipmentBorrowing/src/EquipmentBorrowing.Api/openapi.json`.

## Repository continuity

The repository baseline observed before Activity 5 edits was `6a9e49abd314d6a4b3f4267af372cd61ae5e8a77` (`Add screenshots`, 2026-11-21), matching `origin/main` at inspection. Confirm that this is the exact submitted Lab 4 snapshot before claiming it as the formal submission baseline. The Activity 5 work is currently uncommitted; no `lab5-submitted` tag has been created.

| Lab 4 workflow | API endpoint(s) | Implementation path | Acceptance scenario |
|---|---|---|---|
| UC-01 equipment catalog | API-01 | `EquipmentBorrowing/src/EquipmentBorrowing.Api/Program.cs` — `GET /api/v1/equipment` | T01, T07 |
| UC-02 available-only catalog | API-01 | `GET /api/v1/equipment?available=true` | T01, T07 |
| UC-03 single checkout | API-10 | `POST /api/v1/borrowings`; `EfBorrowingRepository.CheckoutAsync` | T04, T05, T07 |
| UC-04 active borrowing list | API-14 | `GET /api/v1/borrowings?status=Active` | T01, T06, T07 |
| UC-05 return item | API-12, API-13 | `POST /api/v1/borrowings/{id}/return`; `EfBorrowingRepository.ReturnAsync` | T04, T07 |
| UC-06 multi-item checkout | API-11 | `POST /api/v1/checkout-requests`; `EfBorrowingRepository.CheckoutAsync` | T05, T07 |
| UC-07 student history | API-15 | `GET /api/v1/students/{id}/borrowings` | T01, T06, T07 |
| UC-08 equipment history | API-16 | `GET /api/v1/equipment/{id}/borrowings` | T01, T06, T07 |
| UC-09 borrowing search/filter/sort | API-14 | `GET /api/v1/borrowings` | T01, T06, T07 |
| UC-12 register student | API-04 | `POST /api/v1/students` | T02, T07 |
| UC-13 edit student | API-05 | `PUT /api/v1/students/{id}` | T02, T07 |
| UC-14 change eligibility | API-06 | `PATCH /api/v1/students/{id}/eligibility` | T02, T07 |
| UC-15 register equipment | API-08 | `POST /api/v1/equipment` | T03, T07 |
| UC-16 edit equipment | API-09 | `PUT /api/v1/equipment/{id}` | T03, T07 |
| API-only reads and missing-record checks | API-01–03, 07, 13, 15–16 | `Program.cs` route handlers | T01 |
| Desktop/API boundary and refresh | API-01–16 | `EquipmentBorrowing/src/EquipmentBorrowing.Desktop/Services/HttpRepositories.cs` and `App.axaml.cs` | T07 |
| Connection/server failure distinction | API-01–16 | `ApiUnavailableException`, `ApiServerException`, and unknown mutation outcome handling | T08 |
| Migration and schema setup | shared database infrastructure | `EquipmentBorrowing/src/EquipmentBorrowing.Api/Migrations/001_initial_postgresql.sql` | T01–T08 |

## Execution evidence status

Implementation references are recorded above. T01–T06 are recorded as passing. T07 data loading is verified, while its live refresh check remains pending. T08 still requires desktop evidence distinguishing a reachable API/database error from an unavailable API, followed by recovery confirmation. Record actual requests, HTTP responses, fresh database reads, restart checks, failures, fixes, and retests in `docs/lab5-acceptance-record.md`.
