-- Campus Equipment Borrowing System (PostgreSQL)
-- The application stores Status as 'Active' or 'Returned'.
-- Use these read-only statements to inspect records after a workflow.

-- 1. Equipment catalog.
SELECT "Id", "Identifier", "Name", "Description", "IsAvailable"
FROM "Equipment"
ORDER BY "Identifier";

-- 2. Items currently available.
SELECT "Id", "Identifier", "Name", "Description"
FROM "Equipment"
WHERE "IsAvailable" = TRUE
ORDER BY "Identifier";

-- 3. Borrowing records with their related student and equipment.
SELECT
    s."StudentNumber",
    s."FullName",
    e."Identifier" AS "EquipmentIdentifier",
    e."Name" AS "EquipmentName",
    b."Id" AS "BorrowingId",
    b."CheckoutDate",
    b."ExpectedReturnDate",
    b."ActualReturnDate",
    b."Status"
FROM "Borrowings" AS b
JOIN "Students" AS s ON s."Id" = b."StudentId"
JOIN "Equipment" AS e ON e."Id" = b."EquipmentId"
ORDER BY b."CheckoutDate" DESC, b."Id";

-- 4. Active borrowing list.
SELECT b."Id", s."StudentNumber", e."Identifier",
       b."CheckoutDate", b."ExpectedReturnDate"
FROM "Borrowings" AS b
JOIN "Students" AS s ON s."Id" = b."StudentId"
JOIN "Equipment" AS e ON e."Id" = b."EquipmentId"
WHERE b."Status" = 'Active'
ORDER BY b."CheckoutDate", b."Id";

-- 5. One student's full history; replace the parameter value.
SELECT b."Id", e."Identifier", e."Name",
       b."CheckoutDate", b."ExpectedReturnDate", b."ActualReturnDate", b."Status"
FROM "Borrowings" AS b
JOIN "Equipment" AS e ON e."Id" = b."EquipmentId"
WHERE b."StudentId" = 'STU-001'
ORDER BY b."CheckoutDate" DESC, b."Id";

-- 6. One item's full history; replace the parameter value.
SELECT b."Id", s."StudentNumber", s."FullName",
       b."CheckoutDate", b."ExpectedReturnDate", b."ActualReturnDate", b."Status"
FROM "Borrowings" AS b
JOIN "Students" AS s ON s."Id" = b."StudentId"
WHERE b."EquipmentId" = 'EQ-101'
ORDER BY b."CheckoutDate" DESC, b."Id";

-- 7. Case-insensitive partial search across student and equipment labels.
-- Replace 'oscillo' with a sample term when running this read-only query.
SELECT b."Id", s."StudentNumber", s."FullName",
       e."Identifier", e."Name", b."CheckoutDate",
       b."ExpectedReturnDate", b."ActualReturnDate", b."Status"
FROM "Borrowings" AS b
JOIN "Students" AS s ON s."Id" = b."StudentId"
JOIN "Equipment" AS e ON e."Id" = b."EquipmentId"
WHERE s."FullName" ILIKE '%oscillo%'
   OR s."StudentNumber" ILIKE '%oscillo%'
   OR e."Name" ILIKE '%oscillo%'
ORDER BY b."CheckoutDate", b."Id";

-- 8. Inspect applied EF Core migrations.
SELECT "MigrationId", "ProductVersion"
FROM "__EFMigrationsHistory"
ORDER BY "MigrationId";
