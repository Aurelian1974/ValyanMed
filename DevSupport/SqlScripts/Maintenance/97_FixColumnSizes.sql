/*
    ==================================================================
    SCRIPT PENTRU CORECTAREA M?RIMII COLOANELOR ENUM ÎN TABELA PERSOANA
    ==================================================================
    
    PROBLEMA: Erorile de truncare indic? c? coloanele pentru enumer?ri
    sunt prea mici pentru a p?stra valorile standard.
    
    SOLU?IA: M?rirea m?rimii coloanelor pentru a suporta valorile complete:
    - TipActIdentitate: trebuie s? suporte 'CertificatNastere' (16 caractere)
    - StareCivila: trebuie s? suporte 'Concubinaj' (10 caractere)  
    - Gen: trebuie s? suporte 'Neprecizat' (10 caractere)
    
    ==================================================================
*/

PRINT '========================================'
PRINT 'VERIFICARE ?I CORECTARE M?RIME COLOANE'
PRINT '========================================'

-- =============================================
-- PASUL 1: Verificare m?rimea actual? a coloanelor
-- =============================================
PRINT 'PASUL 1: Verificare m?rimea actual?...'

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Persoana' 
  AND COLUMN_NAME IN ('TipActIdentitate', 'StareCivila', 'Gen')
ORDER BY COLUMN_NAME

-- =============================================
-- PASUL 2: Verificare valori existente (lungimi)
-- =============================================
PRINT 'PASUL 2: Verificare lungimi valori existente...'

-- Verificare TipActIdentitate
SELECT 
    'TipActIdentitate' as Coloana,
    TipActIdentitate as Valoare,
    LEN(TipActIdentitate) as Lungime,
    COUNT(*) as Numar
FROM dbo.Persoana 
WHERE TipActIdentitate IS NOT NULL
GROUP BY TipActIdentitate
ORDER BY LEN(TipActIdentitate) DESC

-- Verificare StareCivila
SELECT 
    'StareCivila' as Coloana,
    StareCivila as Valoare,
    LEN(StareCivila) as Lungime,
    COUNT(*) as Numar
FROM dbo.Persoana 
WHERE StareCivila IS NOT NULL
GROUP BY StareCivila
ORDER BY LEN(StareCivila) DESC

-- Verificare Gen
SELECT 
    'Gen' as Coloana,
    Gen as Valoare,
    LEN(Gen) as Lungime,
    COUNT(*) as Numar
FROM dbo.Persoana 
WHERE Gen IS NOT NULL
GROUP BY Gen
ORDER BY LEN(Gen) DESC

-- =============================================
-- PASUL 3: Corectare m?rime coloane
-- =============================================
PRINT 'PASUL 3: Corectare m?rime coloane...'

-- M?rire TipActIdentitate pentru a suporta 'CertificatNastere' (16 + buffer = 20)
PRINT 'M?rire coloana TipActIdentitate la NVARCHAR(20)...'
ALTER TABLE dbo.Persoana 
ALTER COLUMN TipActIdentitate NVARCHAR(20) NULL

-- M?rire StareCivila pentru a suporta 'Concubinaj' (10 + buffer = 15)
PRINT 'M?rire coloana StareCivila la NVARCHAR(15)...'
ALTER TABLE dbo.Persoana 
ALTER COLUMN StareCivila NVARCHAR(15) NULL

-- M?rire Gen pentru a suporta 'Neprecizat' (10 + buffer = 15)
PRINT 'M?rire coloana Gen la NVARCHAR(15)...'
ALTER TABLE dbo.Persoana 
ALTER COLUMN Gen NVARCHAR(15) NULL

-- =============================================
-- PASUL 4: Verificare dup? modificare
-- =============================================
PRINT 'PASUL 4: Verificare dup? modificare...'

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Persoana' 
  AND COLUMN_NAME IN ('TipActIdentitate', 'StareCivila', 'Gen')
ORDER BY COLUMN_NAME

-- =============================================
-- PASUL 5: Test UPDATE cu valorile standard
-- =============================================
PRINT 'PASUL 5: Test UPDATE cu valorile standard...'

-- Test TipActIdentitate (cel mai lung: CertificatNastere = 16 caractere)
BEGIN TRY
    DECLARE @TestId INT = (SELECT TOP 1 Id FROM dbo.Persoana WHERE TipActIdentitate IS NOT NULL)
    
    IF @TestId IS NOT NULL
    BEGIN
        UPDATE dbo.Persoana 
        SET TipActIdentitate = 'CertificatNastere'
        WHERE Id = @TestId
        
        PRINT '? Test TipActIdentitate: SUCCESS'
        
        -- Revert test
        UPDATE dbo.Persoana 
        SET TipActIdentitate = 'CarteIdentitate'
        WHERE Id = @TestId
    END
END TRY
BEGIN CATCH
    PRINT '? Test TipActIdentitate: FAILED - ' + ERROR_MESSAGE()
END CATCH

-- Test StareCivila (cel mai lung: Concubinaj = 10 caractere)
BEGIN TRY
    DECLARE @TestId2 INT = (SELECT TOP 1 Id FROM dbo.Persoana WHERE StareCivila IS NOT NULL)
    
    IF @TestId2 IS NOT NULL
    BEGIN
        UPDATE dbo.Persoana 
        SET StareCivila = 'Concubinaj'
        WHERE Id = @TestId2
        
        PRINT '? Test StareCivila: SUCCESS'
        
        -- Revert test
        UPDATE dbo.Persoana 
        SET StareCivila = 'Necasatorit'
        WHERE Id = @TestId2
    END
END TRY
BEGIN CATCH
    PRINT '? Test StareCivila: FAILED - ' + ERROR_MESSAGE()
END CATCH

-- Test Gen (cel mai lung: Neprecizat = 10 caractere)
BEGIN TRY
    DECLARE @TestId3 INT = (SELECT TOP 1 Id FROM dbo.Persoana WHERE Gen IS NOT NULL)
    
    IF @TestId3 IS NOT NULL
    BEGIN
        UPDATE dbo.Persoana 
        SET Gen = 'Neprecizat'
        WHERE Id = @TestId3
        
        PRINT '? Test Gen: SUCCESS'
        
        -- Revert test
        UPDATE dbo.Persoana 
        SET Gen = 'Masculin'
        WHERE Id = @TestId3
    END
END TRY
BEGIN CATCH
    PRINT '? Test Gen: FAILED - ' + ERROR_MESSAGE()
END CATCH

PRINT ''
PRINT '? CORECTARE M?RIME COLOANE COMPLET?!'
PRINT ''
PRINT '?? URM?TORII PA?I:'
PRINT '1. Rula?i script-ul de cur??are date (99_CleanupLegacyData.sql)'
PRINT '2. Testa?i aplica?ia Blazor'
PRINT '3. Verifica?i c? nu mai exist? erori de truncare'
PRINT ''
PRINT 'Script completat la: ' + FORMAT(GETDATE(), 'yyyy-MM-dd HH:mm:ss')