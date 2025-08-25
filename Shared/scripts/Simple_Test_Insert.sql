-- Script de test simplificat pentru debug UNIQUEIDENTIFIER
USE [ValyanMed]
GO

PRINT 'Test populare Pacienti simplificat...';

-- ?terge înregistr?ri test dac? exist?
DELETE FROM Pacienti WHERE Nume = 'TestPopa';

-- Test inserare simpl?
BEGIN TRY
    INSERT INTO Pacienti (
        PacientID, 
        Nume, 
        Prenume, 
        DataNasterii, 
        Gen, 
        EsteActiv,
        DataCreare
    )
    VALUES (
        NEWID(), 
        'TestPopa', 
        'Ion', 
        '1980-01-01', 
        'Masculin', 
        1,
        GETDATE()
    );
    
    PRINT 'SUCCESS: Test inserare reusita';
    
    -- Verific? dac? înregistrarea exist?
    IF EXISTS (SELECT * FROM Pacienti WHERE Nume = 'TestPopa')
        PRINT 'SUCCESS: Inregistrarea a fost gasita in baza de date';
    ELSE
        PRINT 'ERROR: Inregistrarea nu a fost gasita';
        
END TRY
BEGIN CATCH
    PRINT 'ERROR: ' + ERROR_MESSAGE();
    PRINT 'ERROR_NUMBER: ' + CAST(ERROR_NUMBER() AS VARCHAR(10));
    PRINT 'ERROR_LINE: ' + CAST(ERROR_LINE() AS VARCHAR(10));
END CATCH

-- Cleanup
DELETE FROM Pacienti WHERE Nume = 'TestPopa';

PRINT 'Test completat.';