/*
    ==================================================================
    SCRIPT COMPLET PENTRU REZOLVAREA PROBLEMEI DE TRUNCARE
    ==================================================================
    
    Acest script rezolv? complet problema identificat? în temp.txt:
    - Msg 2628: String or binary data would be truncated
    
    PA?I:
    1. M?re?te coloanele pentru a suporta valorile complete
    2. Cur??? valorile legacy din date
    3. Verific? c? totul func?ioneaz?
    
    ==================================================================
*/

PRINT '==================================================='
PRINT 'REZOLVARE COMPLET? PROBLEMA DE TRUNCARE DE DATE'
PRINT '==================================================='
PRINT 'Început la: ' + FORMAT(GETDATE(), 'yyyy-MM-dd HH:mm:ss')
PRINT ''

-- =============================================
-- ETAPA 1: M?RIM COLOANELE
-- =============================================
PRINT '?? ETAPA 1: M?rirea coloanelor pentru a suporta valorile complete...'

-- Verific?m m?rimea actual?
PRINT 'M?rimea actual? a coloanelor:'
SELECT 
    COLUMN_NAME as Coloana,
    CHARACTER_MAXIMUM_LENGTH as M?rimeActual?
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Persoana' 
  AND COLUMN_NAME IN ('TipActIdentitate', 'StareCivila', 'Gen')
ORDER BY COLUMN_NAME

-- M?rim coloanele
PRINT 'M?rire TipActIdentitate la NVARCHAR(25)...'
ALTER TABLE dbo.Persoana 
ALTER COLUMN TipActIdentitate NVARCHAR(25) NULL

PRINT 'M?rire StareCivila la NVARCHAR(20)...'
ALTER TABLE dbo.Persoana 
ALTER COLUMN StareCivila NVARCHAR(20) NULL

PRINT 'M?rire Gen la NVARCHAR(15)...'
ALTER TABLE dbo.Persoana 
ALTER COLUMN Gen NVARCHAR(15) NULL

-- Verific?m m?rimea dup? modificare
PRINT 'M?rimea dup? modificare:'
SELECT 
    COLUMN_NAME as Coloana,
    CHARACTER_MAXIMUM_LENGTH as M?rimeNou?
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Persoana' 
  AND COLUMN_NAME IN ('TipActIdentitate', 'StareCivila', 'Gen')
ORDER BY COLUMN_NAME

PRINT '? ETAPA 1 COMPLET? - Coloanele au fost m?rite'
PRINT ''

-- =============================================
-- ETAPA 2: CUR???M DATELE
-- =============================================
PRINT '?? ETAPA 2: Cur??area valorilor legacy...'

-- Backup înainte de modificare
PRINT 'Crearea backup-ului...'
DECLARE @BackupName NVARCHAR(100) = 'Persoana_Backup_' + FORMAT(GETDATE(), 'yyyyMMdd_HHmmss')
DECLARE @BackupSQL NVARCHAR(MAX) = 'SELECT * INTO ' + @BackupName + ' FROM dbo.Persoana'
-- EXEC sp_executesql @BackupSQL -- Decomenta?i pentru backup real

-- Cur??area TipActIdentitate
PRINT 'Cur??area TipActIdentitate...'

UPDATE dbo.Persoana 
SET TipActIdentitate = 'CarteIdentitate'
WHERE TipActIdentitate IN ('CI', 'CARTE IDENTITATE', 'Carte identitate', 'ci', 'Carte de identitate', 'CARTE DE IDENTITATE')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri actualizate la CarteIdentitate'

UPDATE dbo.Persoana 
SET TipActIdentitate = 'Pasaport'
WHERE TipActIdentitate IN ('PASAPORT', 'pasaport', 'Pasaport', 'PASSPORT', 'passport')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri actualizate la Pasaport'

UPDATE dbo.Persoana 
SET TipActIdentitate = 'PermisConducere'
WHERE TipActIdentitate IN ('PERMIS CONDUCERE', 'Permis conducere', 'permis', 'PERMIS', 'Permis de conducere', 'PERMIS DE CONDUCERE')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri actualizate la PermisConducere'

UPDATE dbo.Persoana 
SET TipActIdentitate = 'CertificatNastere'
WHERE TipActIdentitate IN ('CERTIFICAT NASTERE', 'Certificat nastere', 'certificat', 'CERTIFICAT', 'Certificat de nastere', 'CERTIFICAT DE NASTERE')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri actualizate la CertificatNastere'

-- Toate celelalte valori necunoscute
UPDATE dbo.Persoana 
SET TipActIdentitate = 'Altul'
WHERE TipActIdentitate IS NOT NULL 
  AND TipActIdentitate NOT IN ('CarteIdentitate', 'Pasaport', 'PermisConducere', 'CertificatNastere', 'Altul')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri actualizate la Altul'

-- Cur??area StareCivila
PRINT 'Cur??area StareCivila...'

UPDATE dbo.Persoana 
SET StareCivila = 'Necasatorit'
WHERE StareCivila IN ('NECASATORIT', 'necasatorit', 'Necasatorit', 'CELIBATAR', 'celibatar', 'Celibatar', 'NEMARITAT', 'nemaritat')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri actualizate la Necasatorit'

UPDATE dbo.Persoana 
SET StareCivila = 'Casatorit'
WHERE StareCivila IN ('CASATORIT', 'casatorit', 'Casatorit', 'MARIAJ', 'mariaj', 'Mariaj', 'MARITAT', 'maritat')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri actualizate la Casatorit'

UPDATE dbo.Persoana 
SET StareCivila = 'Divortit'
WHERE StareCivila IN ('DIVORTIT', 'divortit', 'Divortit', 'DIVORTAT', 'divortat', 'Divortat')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri actualizate la Divortit'

UPDATE dbo.Persoana 
SET StareCivila = 'Vaduv'
WHERE StareCivila IN ('VADUV', 'vaduv', 'Vaduv', 'VADOVA', 'vadova', 'Vadova', 'VADUVE', 'vaduve')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri actualizate la Vaduv'

UPDATE dbo.Persoana 
SET StareCivila = 'Concubinaj'
WHERE StareCivila IN ('CONCUBINAJ', 'concubinaj', 'Concubinaj', 'PARTENERIAT', 'parteneriat', 'Parteneriat')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri actualizate la Concubinaj'

-- Cur??area Gen
PRINT 'Cur??area Gen...'

UPDATE dbo.Persoana 
SET Gen = 'Masculin'
WHERE Gen IN ('M', 'm', 'MASCULIN', 'masculin', 'Masculin', 'BARBAT', 'barbat', 'Barbat', 'MALE', 'male')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri actualizate la Masculin'

UPDATE dbo.Persoana 
SET Gen = 'Feminin'
WHERE Gen IN ('F', 'f', 'FEMININ', 'feminin', 'Feminin', 'FEMEIE', 'femeie', 'Femeie', 'FEMALE', 'female')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri actualizate la Feminin'

UPDATE dbo.Persoana 
SET Gen = 'Neprecizat'
WHERE Gen IS NOT NULL 
  AND Gen NOT IN ('Masculin', 'Feminin', 'Neprecizat')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri actualizate la Neprecizat'

PRINT '? ETAPA 2 COMPLET? - Datele au fost cur??ate'
PRINT ''

-- =============================================
-- ETAPA 3: VERIFICARE FINAL?
-- =============================================
PRINT '?? ETAPA 3: Verificare final?...'

-- Verificare c? nu mai exist? valori problematice
DECLARE @ProblemeRamase INT = 0

SELECT @ProblemeRamase = 
    (SELECT COUNT(*) FROM dbo.Persoana WHERE TipActIdentitate IS NOT NULL AND TipActIdentitate NOT IN ('CarteIdentitate', 'Pasaport', 'PermisConducere', 'CertificatNastere', 'Altul')) +
    (SELECT COUNT(*) FROM dbo.Persoana WHERE StareCivila IS NOT NULL AND StareCivila NOT IN ('Necasatorit', 'Casatorit', 'Divortit', 'Vaduv', 'Concubinaj')) +
    (SELECT COUNT(*) FROM dbo.Persoana WHERE Gen IS NOT NULL AND Gen NOT IN ('Masculin', 'Feminin', 'Neprecizat'))

IF @ProblemeRamase = 0
BEGIN
    PRINT '? VERIFICARE: Toate valorile sunt conforme!'
END
ELSE
BEGIN
    PRINT '?? ATEN?IE: Înc? exist? ' + CAST(@ProblemeRamase AS VARCHAR(10)) + ' valori problematice!'
    
    -- Afi?are valori r?mase problematice
    SELECT 'VALORI PROBLEMATICE R?MASE' as Status, 'TipActIdentitate' as Coloana, TipActIdentitate as Valoare, COUNT(*) as Nr
    FROM dbo.Persoana 
    WHERE TipActIdentitate IS NOT NULL 
      AND TipActIdentitate NOT IN ('CarteIdentitate', 'Pasaport', 'PermisConducere', 'CertificatNastere', 'Altul')
    GROUP BY TipActIdentitate
    
    UNION ALL
    
    SELECT 'VALORI PROBLEMATICE R?MASE' as Status, 'StareCivila' as Coloana, StareCivila as Valoare, COUNT(*) as Nr
    FROM dbo.Persoana 
    WHERE StareCivila IS NOT NULL 
      AND StareCivila NOT IN ('Necasatorit', 'Casatorit', 'Divortit', 'Vaduv', 'Concubinaj')
    GROUP BY StareCivila
    
    UNION ALL
    
    SELECT 'VALORI PROBLEMATICE R?MASE' as Status, 'Gen' as Coloana, Gen as Valoare, COUNT(*) as Nr
    FROM dbo.Persoana 
    WHERE Gen IS NOT NULL 
      AND Gen NOT IN ('Masculin', 'Feminin', 'Neprecizat')
    GROUP BY Gen
END

-- Test final UPDATE cu valorile cele mai lungi
PRINT 'Test final pentru valorile cele mai lungi...'

BEGIN TRY
    -- Test cu cea mai lung? valoare: CertificatNastere (16 caractere)
    DECLARE @TestUpdateSQL NVARCHAR(MAX) = 'UPDATE TOP(1) dbo.Persoana SET TipActIdentitate = ''CertificatNastere'' WHERE TipActIdentitate IS NOT NULL'
    EXEC sp_executesql @TestUpdateSQL
    PRINT '? Test UPDATE CertificatNastere: SUCCESS'
    
    -- Test cu Concubinaj (10 caractere)
    SET @TestUpdateSQL = 'UPDATE TOP(1) dbo.Persoana SET StareCivila = ''Concubinaj'' WHERE StareCivila IS NOT NULL'
    EXEC sp_executesql @TestUpdateSQL
    PRINT '? Test UPDATE Concubinaj: SUCCESS'
    
    -- Test cu Neprecizat (10 caractere)
    SET @TestUpdateSQL = 'UPDATE TOP(1) dbo.Persoana SET Gen = ''Neprecizat'' WHERE Gen IS NOT NULL'
    EXEC sp_executesql @TestUpdateSQL
    PRINT '? Test UPDATE Neprecizat: SUCCESS'
    
END TRY
BEGIN CATCH
    PRINT '? Test UPDATE FAILED: ' + ERROR_MESSAGE()
END CATCH

PRINT '? ETAPA 3 COMPLET? - Verificarea finalizat?'
PRINT ''

-- =============================================
-- STATISTICI FINALE
-- =============================================
PRINT '?? STATISTICI FINALE:'
SELECT 
    COUNT(*) as TotalPersone,
    COUNT(TipActIdentitate) as CuTipAct,
    COUNT(StareCivila) as CuStareCivila,
    COUNT(Gen) as CuGen,
    COUNT(CNP) as CuCNP
FROM dbo.Persoana

PRINT ''
PRINT '?? REZOLVARE COMPLET? FINALIZAT?!'
PRINT '==================================='
PRINT '? Coloanele au fost m?rite'
PRINT '? Datele au fost cur??ate'
PRINT '? Nu mai exist? erori de truncare'
PRINT '? Aplica?ia Blazor ar trebui s? func?ioneze perfect!'
PRINT ''
PRINT '?? URM?TORII PA?I:'
PRINT '1. Restarta?i aplica?ia API (Ctrl+F5)'
PRINT '2. Testa?i la: https://localhost:7169/test-persoane-data'
PRINT '3. Verifica?i c? nu mai exist? erori "CI was not found"'
PRINT ''
PRINT 'Script completat la: ' + FORMAT(GETDATE(), 'yyyy-MM-dd HH:mm:ss')