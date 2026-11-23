-- Activity 5 PostgreSQL baseline. Safe to apply repeatedly; it never drops data.
CREATE TABLE IF NOT EXISTS "Students" (
    "Id" text PRIMARY KEY,
    "StudentNumber" varchar(32) NOT NULL,
    "StudentNumberNormalized" varchar(32) NOT NULL,
    "FullName" varchar(120) NOT NULL,
    "Contact" varchar(254),
    "IsEligible" boolean NOT NULL DEFAULT TRUE
);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Students_StudentNumberNormalized"
    ON "Students" ("StudentNumberNormalized");

CREATE TABLE IF NOT EXISTS "Equipment" (
    "Id" text PRIMARY KEY,
    "Identifier" varchar(32) NOT NULL,
    "IdentifierNormalized" varchar(32) NOT NULL,
    "Name" varchar(120) NOT NULL,
    "Description" varchar(500),
    "IsAvailable" boolean NOT NULL DEFAULT TRUE
);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Equipment_IdentifierNormalized"
    ON "Equipment" ("IdentifierNormalized");

CREATE TABLE IF NOT EXISTS "Borrowings" (
    "Id" text PRIMARY KEY,
    "StudentId" text NOT NULL REFERENCES "Students" ("Id") ON DELETE RESTRICT,
    "EquipmentId" text NOT NULL REFERENCES "Equipment" ("Id") ON DELETE RESTRICT,
    "CheckoutDate" date NOT NULL,
    "ExpectedReturnDate" date NOT NULL,
    "ActualReturnDate" date,
    "Status" text NOT NULL DEFAULT 'Active',
    CONSTRAINT "CK_Borrowings_ExpectedReturnDate" CHECK ("ExpectedReturnDate" > "CheckoutDate"),
    CONSTRAINT "CK_Borrowings_Status" CHECK ("Status" IN ('Active', 'Returned', 'Requested', 'Approved', 'Borrowed', 'Overdue'))
);
CREATE INDEX IF NOT EXISTS "IX_Borrowings_StudentId" ON "Borrowings" ("StudentId");
CREATE INDEX IF NOT EXISTS "IX_Borrowings_EquipmentId" ON "Borrowings" ("EquipmentId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Borrowings_EquipmentId_Active"
    ON "Borrowings" ("EquipmentId") WHERE "Status" = 'Active';

CREATE SEQUENCE IF NOT EXISTS "StudentIdSequence" AS bigint MINVALUE 1121 START WITH 1121;
SELECT setval('"StudentIdSequence"', GREATEST(1121, COALESCE(MAX(substring("Id" from 4)::bigint), 1121)), true)
FROM "Students"
WHERE "Id" ~ '^ST-[0-9]+$';

CREATE SEQUENCE IF NOT EXISTS "BorrowingIdSequence" AS bigint MINVALUE 0 START WITH 0;
SELECT setval('"BorrowingIdSequence"', GREATEST(0, COALESCE(MAX(substring("Id" from 4)::bigint), 0)), true)
FROM "Borrowings"
WHERE "Id" ~ '^BR-[0-9]+$';

CREATE TABLE IF NOT EXISTS "Activity5Migrations" (
    "MigrationId" text PRIMARY KEY,
    "AppliedAtUtc" timestamptz NOT NULL DEFAULT now()
);
INSERT INTO "Activity5Migrations" ("MigrationId")
VALUES ('001_initial_postgresql') ON CONFLICT DO NOTHING;
