# Activity 5 acceptance execution record

**Status:** T01–T06 are recorded as passing against the disposable `campus_equipment_lab5` database. T07 and T08 remain in progress. Do not treat an HTTP error status alone as proof that a mutation rolled back.

## Run details

- Date/time and local timezone: 2026-11-22 to 2026-11-23, Asia/Singapore
- API base URL: `http://localhost:5090`
- PostgreSQL version and disposable database name (no password): PostgreSQL 17, `campus_equipment_lab5`
- Dataset reset time:
- Application date used for checkout rules:
- API revision/commit: Activity 5 working tree, uncommitted; no submission tag

## Results

| Test | Expected | Observed HTTP/UI result | Fresh PostgreSQL state/evidence | Pass/fail | Diagnosis, fix, retest |
|---|---|---|---|---|---|
| T01 independent API reads/OpenAPI | 200 lists/details, empty arrays, 404 missing IDs/routes, 405 wrong methods; OpenAPI covers API-01–16 | 200 empty equipment/students/borrowings lists; 404 missing equipment/student/borrowing/history owners/routes; 405 wrong method; OpenAPI has 16 operations | Startup baseline applied to disposable database; list endpoints returned `[]`; user captured empty equipment endpoint | Pass | Wrong method initially returned 404; fallback now returns 405 with `Allow`; rebuilt and retested |
| T02 student create/edit/eligibility | 201/200; 400 invalid; 409 duplicate/business conflict; history retained | Create 201; normalized duplicate 409; invalid required field 400; edit 200; eligibility update 200; later edit after checkout 200; history remained four rows | API GET readback confirms committed student record; browser and pgAdmin evidence received | Pass | |
| T03 equipment create/edit/protected fields | 201/200; invalid/protected fields 400; duplicate 409; availability/history retained | Create 201; duplicate normalized code 409; invalid required field 400; protected `isAvailable` field 400; later edit while borrowed 200; availability stayed false and one history row remained | API GET readback confirms persisted equipment record; screenshot pending | Pass | |
| T04 single checkout/return/restart | Correct 201/200, 400/404/409; saved state persists after restart | Create student/equipment 201/201; invalid date 400; checkout 201; duplicate active checkout 409; missing borrowing 404; active detail 200; return 200; returned detail 200; readable-ID sample returned 200 | After API restart, borrowing remained `Returned`; student and equipment histories each returned one row; equipment remained available. Readable sample: student `ST-01122` / school number `2024302407`, equipment `EQ-102`, borrowing `BR-00001` | Pass; browser and pgAdmin evidence received | |
| T05 multi-item limit/atomic rollback | Valid checkout persists all items; invalid/over-limit/fault cases leave no partial writes | Two-item checkout 201; third item 201; over-limit 409; duplicate, one-item, and invalid-date requests 400; unavailable-item mix 409; injected database fault 500 | `ST-01122` has three active test borrowings; `ST-01123` history remains empty; `EQ-106`–`EQ-109` remain available after rejected/faulted requests; temporary trigger/function removed; browser and pgAdmin evidence received | Pass | No partial writes after the injected database error; the trigger and function were removed immediately after the check. |
| T06 histories/search/filter/sort | Correct relationships/order; `[]` for no matches; 404 missing owner; 400 invalid query | Student/equipment histories retained after edits; student-number search 4 rows; equipment-name search 1; no-match and valid empty history `[]`; Active/Returned filters 5/2; combined filter 3; expected-date and borrowed-date ascending/descending order verified; invalid status/sort/direction 400; missing owners 404 | Readbacks show relationship IDs and expected-date order; browser and pgAdmin screenshots received | Pass | |
| T07 Avalonia HTTP integration | API works without desktop; UI refreshes API changes; no desktop DB access | Desktop loaded the existing equipment list; API terminal logged successful equipment and borrowing GET requests (200) | Latest desktop screenshot shows persisted equipment records; application source uses HTTP repositories and no desktop DB provider | Partial; data load verified, live refresh after an API-side change remains to verify | Desktop/API defaults are aligned to port 5090.
| T08 unavailable API vs database/server error | No response is shown as connection issue; reachable API 500 is distinct; recovery and restart preserve committed data | Desktop displayed the connection-error message for unavailable API; intentional missing-database API returned HTTP 500 in browser and terminal; after restoring `campus_equipment_lab5` and restarting, equipment GET returned saved records; desktop server-error message and final recovery display have not been captured | Browser recovery readback returned existing equipment (`EQ-102` through `EQ-109` and prior Activity 5 rows) after API restart | Partial; desktop server-error message and final recovery display need retest | Desktop/API address mismatch was found; both defaults are now set to 5090.

## Evidence index

- `Screenshot 2026-11-22 221532.png`: browser capture of `GET /api/v1/equipment` returning `[]`.
- User browser capture of `GET /api/v1/students`: edited student and `isEligible=false`.
- User pgAdmin capture of `public.Students`: one persisted edited student row with `IsEligible=false`.
- T03 browser and pgAdmin captures of the edited equipment record: pending from user.
- `Screenshot 2026-11-22 225951.png`: browser capture of returned `BR-00001` showing `ST-01122`, `EQ-102`, and school number `2024302407` after restart.
- `Screenshot 2026-11-22 230040.png`: pgAdmin readback of `BR-00001` with `StudentId=ST-01122` and `EquipmentId=EQ-102`; safe table list also visible.
- User browser screenshot of the three active borrowings and pgAdmin screenshot confirming `EQ-103`–`EQ-105` active / unavailable and `EQ-106`–`EQ-109` available with no borrowing: received.
- `Screenshot 2026-11-22 235659.png`: browser capture of borrowings sorted by expected return date.
- `Screenshot 2026-11-22 235724.png`: pgAdmin join readback for `ST-01122` and `ST-01123`, showing linked IDs, dates, and statuses.
- User desktop captures on 2026-11-23 show the connection-error banner for Equipment and Students when the API is unavailable.
- User browser and API-terminal captures on 2026-11-23 show `GET /api/v1/equipment` returning HTTP 500 with `server_error` when the test DB name is intentionally missing.
- User browser capture on 2026-11-23 shows the equipment list returning records after restoring `campus_equipment_lab5` and restarting the API.
- T07 desktop data-load/API HTTP 200 capture: received; live refresh after an API-side change remains pending.
- T08 desktop HTTP 500 distinction and recovery capture: pending.
- Earlier API response checks used `http://localhost:5080`; the current launch configuration and final checks use `http://localhost:5090`. No passwords were recorded here.
