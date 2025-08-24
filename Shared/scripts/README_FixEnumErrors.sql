/*
    ==================================================================
    GHID COMPLET PENTRU REZOLVAREA PROBLEMEI "CI was not found"
    ==================================================================
    
    Aceast? problem? apare când în baza de date exist? valori pentru 
    enumer?ri care nu sunt definite în codul C#.
    
    PA?II PENTRU REZOLVARE:
    1. Rula?i diagnosticul (98_DiagnosticLegacyData.sql)
    2. Rula?i cur??area (99_CleanupLegacyData.sql)
    3. Testa?i aplica?ia
    
    ==================================================================
*/

-- ==================================================================
-- PASUL 1: DIAGNOSTIC (op?ional - pentru a vedea problemele)
-- ==================================================================

/*
PRINT 'STEP 1: Diagnostic probleme...'

-- Verificare TipActIdentitate problematice
SELECT 'TipActIdentitate PROBLEMATICE' as Tip, TipActIdentitate, COUNT(*) as Nr
FROM dbo.Persoana 
WHERE TipActIdentitate IS NOT NULL 
  AND TipActIdentitate NOT IN ('CarteIdentitate', 'Pasaport', 'PermisConducere', 'CertificatNastere', 'Altul')
GROUP BY TipActIdentitate

-- Verificare StareCivila problematice  
SELECT 'StareCivila PROBLEMATICE' as Tip, StareCivila, COUNT(*) as Nr
FROM dbo.Persoana 
WHERE StareCivila IS NOT NULL 
  AND StareCivila NOT IN ('Necasatorit', 'Casatorit', 'Divortit', 'Vaduv', 'Concubinaj')
GROUP BY StareCivila

-- Verificare Gen problematice
SELECT 'Gen PROBLEMATICE' as Tip, Gen, COUNT(*) as Nr
FROM dbo.Persoana 
WHERE Gen IS NOT NULL 
  AND Gen NOT IN ('Masculin', 'Feminin', 'Neprecizat')
GROUP BY Gen
*/

-- ==================================================================
-- PASUL 2: CUR??ARE (MANDATORY - pentru a rezolva problemele)
-- ==================================================================

PRINT 'STEP 2: Cur??are valori problematice...'

-- Backup înainte de modificare (op?ional dar recomandat)
-- SELECT * INTO Persoana_Backup_' + FORMAT(GETDATE(), 'yyyyMMdd_HHmmss') + ' FROM dbo.Persoana

-- =============================================
-- Cur??area TipActIdentitate
-- =============================================
PRINT 'Cur??area TipActIdentitate...'

UPDATE dbo.Persoana 
SET TipActIdentitate = 'CarteIdentitate'
WHERE TipActIdentitate IN ('CI', 'CARTE IDENTITATE', 'Carte identitate', 'ci', 'Carte de identitate')

UPDATE dbo.Persoana 
SET TipActIdentitate = 'Pasaport'
WHERE TipActIdentitate IN ('PASAPORT', 'pasaport', 'Pasaport', 'PASSPORT', 'passport')

UPDATE dbo.Persoana 
SET TipActIdentitate = 'PermisConducere'
WHERE TipActIdentitate IN ('PERMIS CONDUCERE', 'Permis conducere', 'permis', 'PERMIS', 'Permis de conducere')

UPDATE dbo.Persoana 
SET TipActIdentitate = 'CertificatNastere'
WHERE TipActIdentitate IN ('CERTIFICAT NASTERE', 'Certificat nastere', 'certificat', 'CERTIFICAT', 'Certificat de nastere')

-- Toate celelalte valori necunoscute
UPDATE dbo.Persoana 
SET TipActIdentitate = 'Altul'
WHERE TipActIdentitate IS NOT NULL 
  AND TipActIdentitate NOT IN ('CarteIdentitate', 'Pasaport', 'PermisConducere', 'CertificatNastere', 'Altul')

-- =============================================
-- Cur??area StareCivila
-- =============================================
PRINT 'Cur??area StareCivila...'

UPDATE dbo.Persoana 
SET StareCivila = 'Necasatorit'
WHERE StareCivila IN ('NECASATORIT', 'necasatorit', 'Necasatorit', 'CELIBATAR', 'celibatar', 'Celibatar', 'NEMARITAT', 'nemaritat')

UPDATE dbo.Persoana 
SET StareCivila = 'Casatorit'
WHERE StareCivila IN ('CASATORIT', 'casatorit', 'Casatorit', 'MARIAJ', 'mariaj', 'Mariaj', 'MARITAT', 'maritat')

UPDATE dbo.Persoana 
SET StareCivila = 'Divortit'
WHERE StareCivila IN ('DIVORTIT', 'divortit', 'Divortit', 'DIVORTAT', 'divortat', 'Divortat')

UPDATE dbo.Persoana 
SET StareCivila = 'Vaduv'
WHERE StareCivila IN ('VADUV', 'vaduv', 'Vaduv', 'VADOVA', 'vadova', 'Vadova', 'VADUVE', 'vaduve')

UPDATE dbo.Persoana 
SET StareCivila = 'Concubinaj'
WHERE StareCivila IN ('CONCUBINAJ', 'concubinaj', 'Concubinaj', 'PARTENERIAT', 'parteneriat', 'Parteneriat')

-- =============================================
-- Cur??area Gen
-- =============================================
PRINT 'Cur??area Gen...'

UPDATE dbo.Persoana 
SET Gen = 'Masculin'
WHERE Gen IN ('M', 'm', 'MASCULIN', 'masculin', 'Masculin', 'BARBAT', 'barbat', 'Barbat', 'MALE', 'male')

UPDATE dbo.Persoana 
SET Gen = 'Feminin'
WHERE Gen IN ('F', 'f', 'FEMININ', 'feminin', 'Feminin', 'FEMEIE', 'femeie', 'Femeie', 'FEMALE', 'female')

UPDATE dbo.Persoana 
SET Gen = 'Neprecizat'
WHERE Gen IS NOT NULL 
  AND Gen NOT IN ('Masculin', 'Feminin', 'Neprecizat')

-- ==================================================================
-- PASUL 3: VERIFICARE FINAL?
-- ==================================================================

PRINT 'STEP 3: Verificare final?...'

-- Verificare c? nu mai exist? valori problematice
DECLARE @ProblemeRamase INT = 0

SELECT @ProblemeRamase = 
    (SELECT COUNT(*) FROM dbo.Persoana WHERE TipActIdentitate IS NOT NULL AND TipActIdentitate NOT IN ('CarteIdentitate', 'Pasaport', 'PermisConducere', 'CertificatNastere', 'Altul')) +
    (SELECT COUNT(*) FROM dbo.Persoana WHERE StareCivila IS NOT NULL AND StareCivila NOT IN ('Necasatorit', 'Casatorit', 'Divortit', 'Vaduv', 'Concubinaj')) +
    (SELECT COUNT(*) FROM dbo.Persoana WHERE Gen IS NOT NULL AND Gen NOT IN ('Masculin', 'Feminin', 'Neprecizat'))

IF @ProblemeRamase = 0
BEGIN
    PRINT '? SUCCESS: Toate valorile au fost corectate!'
    PRINT '? Aplica?ia Blazor ar trebui s? func?ioneze f?r? erori acum.'
END
ELSE
BEGIN
    PRINT '? ATEN?IE: Înc? exist? ' + CAST(@ProblemeRamase AS VARCHAR(10)) + ' valori problematice!'
    
    -- Afi?are valori r?mase problematice
    SELECT 'TipActIdentitate RAMAS PROBLEMATIC' as Tip, TipActIdentitate, COUNT(*) as Nr
    FROM dbo.Persoana 
    WHERE TipActIdentitate IS NOT NULL 
      AND TipActIdentitate NOT IN ('CarteIdentitate', 'Pasaport', 'PermisConducere', 'CertificatNastere', 'Altul')
    GROUP BY TipActIdentitate
    
    UNION ALL
    
    SELECT 'StareCivila RAMAS PROBLEMATIC' as Tip, StareCivila, COUNT(*) as Nr
    FROM dbo.Persoana 
    WHERE StareCivila IS NOT NULL 
      AND StareCivila NOT IN ('Necasatorit', 'Casatorit', 'Divortit', 'Vaduv', 'Concubinaj')
    GROUP BY StareCivila
    
    UNION ALL
    
    SELECT 'Gen RAMAS PROBLEMATIC' as Tip, Gen, COUNT(*) as Nr
    FROM dbo.Persoana 
    WHERE Gen IS NOT NULL 
      AND Gen NOT IN ('Masculin', 'Feminin', 'Neprecizat')
    GROUP BY Gen
END

-- Statistici finale
PRINT 'Statistici finale:'
SELECT 
    COUNT(*) as TotalPersone,
    COUNT(TipActIdentitate) as CuTipAct,
    COUNT(StareCivila) as CuStareCivila,
    COUNT(Gen) as CuGen,
    COUNT(CNP) as CuCNP
FROM dbo.Persoana

PRINT 'Script completat la: ' + FORMAT(GETDATE(), 'yyyy-MM-dd HH:mm:ss')

/*
    ==================================================================
    DUP? RULAREA ACESTUI SCRIPT:
    ==================================================================
    
    1. Restarta?i aplica?ia API (Ctrl+F5 în Visual Studio)
    2. Testa?i înc?rcarea datelor la: /test-persoane-data
    3. Verifica?i c? nu mai exist? erori "CI was not found"
    4. Dac? înc? ave?i probleme, verifica?i logs-urile API pentru alte erori
    
    PENTRU DEBUGGING SUPLIMENTAR:
    - Accesa?i /debug-auth pentru testarea autentific?rii
    - Accesa?i /admin/data-cleanup pentru diagnostic automatizat
    
    ==================================================================
*/