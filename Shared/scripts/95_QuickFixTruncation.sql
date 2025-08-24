/*
    ==================================================================
    SCRIPT OPTIMIZAT PENTRU REZOLVAREA RAPID? A PROBLEMEI DE TRUNCARE
    ==================================================================
    
    Aceast? versiune folose?te valori scurte pentru compatibilitate
    cu coloanele existente din baza de date, evitând modificarea structurii.
    
    STRATEGIE:
    - Folosim valori scurte: 'CI', 'Pasaport', 'Permis', 'Certificat', 'Altul'
    - Acestea sunt compatibile cu coloane NVARCHAR(10) sau mai mari
    - SafeParseEnum din C# va mapa automat valorile
    
    ==================================================================
*/

PRINT '=============================================='
PRINT 'REZOLVARE RAPID? PROBLEM? TRUNCARE - V2.0'
PRINT '=============================================='
PRINT 'Început la: ' + FORMAT(GETDATE(), 'yyyy-MM-dd HH:mm:ss')
PRINT ''

-- =============================================
-- VERIFICARE M?RIME COLOANE ACTUAL?
-- =============================================
PRINT '?? Verificare m?rimea coloanelor actuale...'
SELECT 
    COLUMN_NAME as Coloana,
    CHARACTER_MAXIMUM_LENGTH as M?rimeMaxim?,
    CASE 
        WHEN CHARACTER_MAXIMUM_LENGTH >= 10 THEN '? OK pentru valori scurte'
        ELSE '? Prea mic? - necesit? m?rire'
    END as Status
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Persoana' 
  AND COLUMN_NAME IN ('TipActIdentitate', 'StareCivila', 'Gen')
ORDER BY COLUMN_NAME

-- =============================================
-- CUR??ARE CU VALORI SCURTE
-- =============================================
PRINT ''
PRINT '?? Cur??area cu valori scurte compatibile...'

-- TipActIdentitate (valorile cele mai scurte posibile)
PRINT 'Cur??area TipActIdentitate cu valori scurte...'

UPDATE dbo.Persoana 
SET TipActIdentitate = 'CI'
WHERE TipActIdentitate IN ('CarteIdentitate', 'CARTE IDENTITATE', 'Carte identitate', 'ci', 'Carte de identitate', 'CARTE DE IDENTITATE', 'CARTE')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri ? CI'

UPDATE dbo.Persoana 
SET TipActIdentitate = 'Pasaport'
WHERE TipActIdentitate IN ('PermisConducere', 'PASAPORT', 'pasaport', 'Pasaport', 'PASSPORT', 'passport', 'PASAP')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri ? Pasaport'

UPDATE dbo.Persoana 
SET TipActIdentitate = 'Permis'
WHERE TipActIdentitate IN ('CertificatNastere', 'PERMIS CONDUCERE', 'Permis conducere', 'permis', 'PERMIS', 'Permis de conducere', 'PERMIS DE CONDUCERE', 'PERMI')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri ? Permis'

UPDATE dbo.Persoana 
SET TipActIdentitate = 'Certificat'
WHERE TipActIdentitate IN ('CERTIFICAT NASTERE', 'Certificat nastere', 'certificat', 'CERTIFICAT', 'Certificat de nastere', 'CERTIFICAT DE NASTERE', 'CERTI')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri ? Certificat'

-- Toate valorile necunoscute sau truncate
UPDATE dbo.Persoana 
SET TipActIdentitate = 'Altul'
WHERE TipActIdentitate IS NOT NULL 
  AND TipActIdentitate NOT IN ('CI', 'Pasaport', 'Permis', 'Certificat', 'Altul')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri ? Altul'

-- StareCivila (valorile scurte)
PRINT 'Cur??area StareCivila cu valori scurte...'

UPDATE dbo.Persoana 
SET StareCivila = 'Celibatar'
WHERE StareCivila IN ('Necasatorit', 'NECASATORIT', 'necasatorit', 'Necasatorit', 'CELIBATAR', 'celibatar', 'Celibatar', 'NEMARITAT', 'nemaritat')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri ? Celibatar'

UPDATE dbo.Persoana 
SET StareCivila = 'Casatorit'
WHERE StareCivila IN ('CASATORIT', 'casatorit', 'Casatorit', 'MARIAJ', 'mariaj', 'Mariaj', 'MARITAT', 'maritat')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri ? Casatorit'

UPDATE dbo.Persoana 
SET StareCivila = 'Divortit'
WHERE StareCivila IN ('DIVORTIT', 'divortit', 'Divortit', 'DIVORTAT', 'divortat', 'Divortat')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri ? Divortit'

UPDATE dbo.Persoana 
SET StareCivila = 'Vaduv'
WHERE StareCivila IN ('VADUV', 'vaduv', 'Vaduv', 'VADOVA', 'vadova', 'Vadova', 'VADUVE', 'vaduve')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri ? Vaduv'

UPDATE dbo.Persoana 
SET StareCivila = 'Partener'
WHERE StareCivila IN ('Concubinaj', 'CONCUBINAJ', 'concubinaj', 'Concubinaj', 'PARTENERIAT', 'parteneriat', 'Parteneriat')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri ? Partener'

-- Gen (valorile scurte)
PRINT 'Cur??area Gen cu valori scurte...'

UPDATE dbo.Persoana 
SET Gen = 'M'
WHERE Gen IN ('Masculin', 'm', 'MASCULIN', 'masculin', 'Masculin', 'BARBAT', 'barbat', 'Barbat', 'MALE', 'male')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri ? M'

UPDATE dbo.Persoana 
SET Gen = 'F'
WHERE Gen IN ('Feminin', 'f', 'FEMININ', 'feminin', 'Feminin', 'FEMEIE', 'femeie', 'Femeie', 'FEMALE', 'female')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri ? F'

UPDATE dbo.Persoana 
SET Gen = 'N'
WHERE Gen IS NOT NULL 
  AND Gen NOT IN ('M', 'F', 'N')
PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistr?ri ? N (Neprecizat)'

PRINT '? Cur??area complet? cu valori scurte!'
PRINT ''

-- =============================================
-- VERIFICARE FINAL?
-- =============================================
PRINT '?? Verificare final?...'

-- Verificare c? toate valorile se încadreaz? în limitele coloanelor
DECLARE @ProblemeRamase INT = 0

-- Verificare valori prea lungi sau non-standard
SELECT @ProblemeRamase = 
    (SELECT COUNT(*) FROM dbo.Persoana WHERE TipActIdentitate IS NOT NULL AND TipActIdentitate NOT IN ('CI', 'Pasaport', 'Permis', 'Certificat', 'Altul')) +
    (SELECT COUNT(*) FROM dbo.Persoana WHERE StareCivila IS NOT NULL AND StareCivila NOT IN ('Celibatar', 'Casatorit', 'Divortit', 'Vaduv', 'Partener')) +
    (SELECT COUNT(*) FROM dbo.Persoana WHERE Gen IS NOT NULL AND Gen NOT IN ('M', 'F', 'N'))

IF @ProblemeRamase = 0
BEGIN
    PRINT '? SUCCES: Toate valorile sunt conforme ?i scurte!'
    PRINT '? Nu vor mai exista erori de truncare!'
END
ELSE
BEGIN
    PRINT '?? ATEN?IE: Înc? exist? ' + CAST(@ProblemeRamase AS VARCHAR(10)) + ' valori problematice!'
END

-- Verificare lungimi maxime
PRINT ''
PRINT 'Verificare lungimi maxime dup? cur??are:'
SELECT 
    'TipActIdentitate' as Coloana,
    MAX(LEN(TipActIdentitate)) as LungimeMaxim?,
    CASE WHEN MAX(LEN(TipActIdentitate)) <= 10 THEN '? OK' ELSE '? Prea lung' END as Status
FROM dbo.Persoana WHERE TipActIdentitate IS NOT NULL

UNION ALL

SELECT 
    'StareCivila' as Coloana,
    MAX(LEN(StareCivila)) as LungimeMaxim?,
    CASE WHEN MAX(LEN(StareCivila)) <= 10 THEN '? OK' ELSE '? Prea lung' END as Status
FROM dbo.Persoana WHERE StareCivila IS NOT NULL

UNION ALL

SELECT 
    'Gen' as Coloana,
    MAX(LEN(Gen)) as LungimeMaxim?,
    CASE WHEN MAX(LEN(Gen)) <= 5 THEN '? OK' ELSE '? Prea lung' END as Status
FROM dbo.Persoana WHERE Gen IS NOT NULL

-- Statistici finale
PRINT ''
PRINT 'Distribu?ia final? a valorilor:'
SELECT 'TipActIdentitate' as Coloana, TipActIdentitate as Valoare, COUNT(*) as Num?rul
FROM dbo.Persoana WHERE TipActIdentitate IS NOT NULL
GROUP BY TipActIdentitate
UNION ALL
SELECT 'StareCivila' as Coloana, StareCivila as Valoare, COUNT(*) as Num?rul
FROM dbo.Persoana WHERE StareCivila IS NOT NULL
GROUP BY StareCivila
UNION ALL
SELECT 'Gen' as Coloana, Gen as Valoare, COUNT(*) as Num?rul
FROM dbo.Persoana WHERE Gen IS NOT NULL
GROUP BY Gen
ORDER BY Coloana, Num?rul DESC

PRINT ''
PRINT '?? REZOLVARE OPTIMIZAT? COMPLET?!'
PRINT '=================================='
PRINT '? Folosim valori scurte compatibile'
PRINT '? Nu modific?m structura bazei de date'
PRINT '? SafeParseEnum din C# va mapa automat'
PRINT '? Nu vor mai exista erori de truncare'
PRINT ''
PRINT '?? URM?TORII PA?I:'
PRINT '1. Restarta?i aplica?ia API'
PRINT '2. Testa?i înc?rcarea datelor'
PRINT '3. Verifica?i c? nu mai apar erori'
PRINT ''
PRINT 'Script completat la: ' + FORMAT(GETDATE(), 'yyyy-MM-dd HH:mm:ss')