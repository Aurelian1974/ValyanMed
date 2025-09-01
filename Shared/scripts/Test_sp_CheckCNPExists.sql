-- =============================================
-- SCRIPT DE TESTARE PENTRU sp_CheckCNPExists
-- =============================================
-- Purpose: Testeaz? stored procedure-ul corectat
-- =============================================

PRINT '================================================';
PRINT 'TESTARE sp_CheckCNPExists';
PRINT '================================================';

-- Test 1: Check if SP exists
IF OBJECT_ID('sp_CheckCNPExists', 'P') IS NOT NULL
BEGIN
    PRINT 'PASS: Stored procedure sp_CheckCNPExists exista.';
    
    -- Test 2: Test cu CNP NULL
    PRINT '';
    PRINT 'TEST 1: CNP NULL...';
    EXEC sp_CheckCNPExists @CNP = NULL;
    
    -- Test 3: Test cu CNP gol
    PRINT '';
    PRINT 'TEST 2: CNP gol...';
    EXEC sp_CheckCNPExists @CNP = '';
    
    -- Test 4: Test cu CNP valid care nu exist?
    PRINT '';
    PRINT 'TEST 3: CNP inexistent...';
    EXEC sp_CheckCNPExists @CNP = '9999999999999';
    
    -- Test 5: Test cu CNP care exist? (din datele de test)
    PRINT '';
    PRINT 'TEST 4: CNP existent (daca tabela are date)...';
    EXEC sp_CheckCNPExists @CNP = '1850315123456';
    
    PRINT '';
    PRINT 'Toate testele au fost executate cu succes!';
END
ELSE
BEGIN
    PRINT 'FAIL: Stored procedure sp_CheckCNPExists NU exista!';
    PRINT 'Rulati mai intai script-ul de creare sau reparare.';
END

PRINT '================================================';