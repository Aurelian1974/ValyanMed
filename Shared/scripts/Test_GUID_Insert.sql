-- Test script pentru a identifica problema cu UNIQUEIDENTIFIER
USE [ValyanMed]
GO

PRINT 'Testing UNIQUEIDENTIFIER insertion...';

-- Test 1: Insert cu NEWID()
IF NOT EXISTS (SELECT * FROM Pacienti WHERE Nume = 'TestPacient1')
BEGIN
    INSERT INTO Pacienti (PacientID, Nume, Prenume, CNP, DataNasterii, Gen, EsteActiv)
    VALUES (NEWID(), 'TestPacient1', 'Test', '1234567890123', '1980-01-01', 'Masculin', 1);
    PRINT 'Test 1 PASSED: NEWID() insertion successful';
END

-- Test 2: Insert cu string literal convertit
IF NOT EXISTS (SELECT * FROM Pacienti WHERE Nume = 'TestPacient2')
BEGIN
    INSERT INTO Pacienti (PacientID, Nume, Prenume, CNP, DataNasterii, Gen, EsteActiv)
    VALUES (CAST('11111111-1111-1111-1111-111111111111' AS UNIQUEIDENTIFIER), 'TestPacient2', 'Test', '1234567890124', '1980-01-01', 'Masculin', 1);
    PRINT 'Test 2 PASSED: String literal cast successful';
END

-- Test 3: Verificare ce tip de date accept? coloana
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Pacienti' AND COLUMN_NAME = 'PacientID';

PRINT 'Test completed. Check output for column information.';

-- Cleanup
DELETE FROM Pacienti WHERE Nume LIKE 'TestPacient%';