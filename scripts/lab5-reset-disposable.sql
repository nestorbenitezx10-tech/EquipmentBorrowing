-- DESTRUCTIVE: run only against a dedicated, disposable Activity 5 database.
-- This resets the three application tables to the controlled T01-T08 dataset.
BEGIN;
DELETE FROM "Borrowings";
DELETE FROM "Students";
DELETE FROM "Equipment";

INSERT INTO "Students" ("Id", "StudentNumber", "StudentNumberNormalized", "FullName", "Contact", "IsEligible") VALUES
('ST-100', 'ST-100', 'ST-100', 'Ada Cruz', NULL, TRUE),
('ST-200', 'ST-200', 'ST-200', 'Ben Diaz', NULL, FALSE),
('ST-300', 'ST-300', 'ST-300', 'Cara Lim', NULL, TRUE),
('ST-400', 'ST-400', 'ST-400', 'Dani Ong', NULL, TRUE);

INSERT INTO "Equipment" ("Id", "Identifier", "IdentifierNormalized", "Name", "Description", "IsAvailable") VALUES
('EQ-001', 'EQ-001', 'EQ-001', 'Camera', NULL, FALSE),
('EQ-002', 'EQ-002', 'EQ-002', 'Laptop', NULL, TRUE),
('EQ-003', 'EQ-003', 'EQ-003', 'Projector', NULL, TRUE),
('EQ-004', 'EQ-004', 'EQ-004', 'Tablet', NULL, FALSE),
('EQ-005', 'EQ-005', 'EQ-005', 'Microphone', NULL, FALSE),
('EQ-006', 'EQ-006', 'EQ-006', 'Tripod', NULL, TRUE),
('EQ-007', 'EQ-007', 'EQ-007', 'Speaker', NULL, TRUE);

INSERT INTO "Borrowings" ("Id", "StudentId", "EquipmentId", "CheckoutDate", "ExpectedReturnDate", "ActualReturnDate", "Status") VALUES
('BR-100-A', 'ST-100', 'EQ-001', CURRENT_DATE - 9, CURRENT_DATE - 1, NULL, 'Active'),
('BR-100-R', 'ST-100', 'EQ-003', CURRENT_DATE - 8, CURRENT_DATE + 2, CURRENT_DATE - 2, 'Returned'),
('BR-300-A', 'ST-300', 'EQ-004', CURRENT_DATE - 7, CURRENT_DATE + 3, NULL, 'Active'),
('BR-300-B', 'ST-300', 'EQ-005', CURRENT_DATE - 7, CURRENT_DATE + 3, NULL, 'Active');
COMMIT;
