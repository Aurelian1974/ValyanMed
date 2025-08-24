/*
    Script de diagnosticare pentru identificarea valorilor problematice în tabela Persoana
    Rula?i acest script ÎNAINTE de cur??are pentru a vedea ce trebuie corectat
*/

PRINT '========================================='
PRINT 'DIAGNOSTICARE VALORI PROBLEMATICE'
PRINT '========================================='

-- =============================================
-- Verificare TipActIdentitate
-- =============================================
PRINT 'Analiz? TipActIdentitate:'
PRINT '--------------------------'

-- Toate valorile distincte pentru TipActIdentitate
SELECT 
    'TipActIdentitate - Valori existente' as Categorie,
    TipActIdentitate as Valoare,
    COUNT(*) as Numar
FROM dbo.Persoana 
WHERE TipActIdentitate IS NOT NULL
GROUP BY TipActIdentitate
ORDER BY COUNT(*) DESC

-- Valorile problematice care nu sunt standard
PRINT 'TipActIdentitate - Valori PROBLEMATICE (vor fi corectate):'
SELECT 
    TipActIdentitate as ValoareProblematica,
    COUNT(*) as Numar,
    CASE 
        WHEN TipActIdentitate IN ('CI', 'CARTE IDENTITATE', 'Carte identitate', 'ci') THEN 'Va fi CarteIdentitate'
        WHEN TipActIdentitate IN ('PASAPORT', 'pasaport', 'Pasaport') THEN 'Va fi Pasaport'
        WHEN TipActIdentitate IN ('PERMIS CONDUCERE', 'Permis conducere', 'permis') THEN 'Va fi PermisConducere'
        WHEN TipActIdentitate IN ('CERTIFICAT NASTERE', 'Certificat nastere', 'certificat') THEN 'Va fi CertificatNastere'
        ELSE 'Va fi Altul'
    END as VaFiCorectatLa
FROM dbo.Persoana 
WHERE TipActIdentitate IS NOT NULL 
  AND TipActIdentitate NOT IN ('CarteIdentitate', 'Pasaport', 'PermisConducere', 'CertificatNastere', 'Altul')
GROUP BY TipActIdentitate
ORDER BY COUNT(*) DESC

-- =============================================
-- Verificare StareCivila
-- =============================================
PRINT ''
PRINT 'Analiz? StareCivila:'
PRINT '--------------------'

-- Toate valorile distincte pentru StareCivila
SELECT 
    'StareCivila - Valori existente' as Categorie,
    StareCivila as Valoare,
    COUNT(*) as Numar
FROM dbo.Persoana 
WHERE StareCivila IS NOT NULL
GROUP BY StareCivila
ORDER BY COUNT(*) DESC

-- Valorile problematice care nu sunt standard
PRINT 'StareCivila - Valori PROBLEMATICE (vor fi corectate):'
SELECT 
    StareCivila as ValoareProblematica,
    COUNT(*) as Numar,
    CASE 
        WHEN StareCivila IN ('NECASATORIT', 'necasatorit', 'Necasatorit', 'CELIBATAR', 'celibatar') THEN 'Va fi Necasatorit'
        WHEN StareCivila IN ('CASATORIT', 'casatorit', 'Casatorit', 'MARIAJ', 'mariaj') THEN 'Va fi Casatorit'
        WHEN StareCivila IN ('DIVORTIT', 'divortit', 'Divortit', 'DIVORTAT', 'divortat') THEN 'Va fi Divortit'
        WHEN StareCivila IN ('VADUV', 'vaduv', 'Vaduv', 'VADOVA', 'vadova') THEN 'Va fi Vaduv'
        WHEN StareCivila IN ('CONCUBINAJ', 'concubinaj', 'Concubinaj', 'PARTENERIAT', 'parteneriat') THEN 'Va fi Concubinaj'
        ELSE 'Va fi ignorat (NULL)'
    END as VaFiCorectatLa
FROM dbo.Persoana 
WHERE StareCivila IS NOT NULL 
  AND StareCivila NOT IN ('Necasatorit', 'Casatorit', 'Divortit', 'Vaduv', 'Concubinaj')
GROUP BY StareCivila
ORDER BY COUNT(*) DESC

-- =============================================
-- Verificare Gen
-- =============================================
PRINT ''
PRINT 'Analiz? Gen:'
PRINT '------------'

-- Toate valorile distincte pentru Gen
SELECT 
    'Gen - Valori existente' as Categorie,
    Gen as Valoare,
    COUNT(*) as Numar
FROM dbo.Persoana 
WHERE Gen IS NOT NULL
GROUP BY Gen
ORDER BY COUNT(*) DESC

-- Valorile problematice care nu sunt standard
PRINT 'Gen - Valori PROBLEMATICE (vor fi corectate):'
SELECT 
    Gen as ValoareProblematica,
    COUNT(*) as Numar,
    CASE 
        WHEN Gen IN ('M', 'm', 'MASCULIN', 'masculin', 'Masculin', 'BARBAT', 'barbat') THEN 'Va fi Masculin'
        WHEN Gen IN ('F', 'f', 'FEMININ', 'feminin', 'Feminin', 'FEMEIE', 'femeie') THEN 'Va fi Feminin'
        ELSE 'Va fi Neprecizat'
    END as VaFiCorectatLa
FROM dbo.Persoana 
WHERE Gen IS NOT NULL 
  AND Gen NOT IN ('Masculin', 'Feminin', 'Neprecizat')
GROUP BY Gen
ORDER BY COUNT(*) DESC

-- =============================================
-- Sumar general
-- =============================================
PRINT ''
PRINT 'SUMAR GENERAL:'
PRINT '=============='
SELECT 
    COUNT(*) as TotalPersone,
    COUNT(TipActIdentitate) as CuTipAct,
    COUNT(StareCivila) as CuStareCivila,
    COUNT(Gen) as CuGen,
    COUNT(CNP) as CuCNP,
    COUNT(DataNasterii) as CuDataNasterii
FROM dbo.Persoana

-- Calculare câte înregistr?ri vor fi afectate
DECLARE @TipActProblematice INT, @StareCivilaProblematice INT, @GenProblematice INT

SELECT @TipActProblematice = COUNT(*)
FROM dbo.Persoana 
WHERE TipActIdentitate IS NOT NULL 
  AND TipActIdentitate NOT IN ('CarteIdentitate', 'Pasaport', 'PermisConducere', 'CertificatNastere', 'Altul')

SELECT @StareCivilaProblematice = COUNT(*)
FROM dbo.Persoana 
WHERE StareCivila IS NOT NULL 
  AND StareCivila NOT IN ('Necasatorit', 'Casatorit', 'Divortit', 'Vaduv', 'Concubinaj')

SELECT @GenProblematice = COUNT(*)
FROM dbo.Persoana 
WHERE Gen IS NOT NULL 
  AND Gen NOT IN ('Masculin', 'Feminin', 'Neprecizat')

PRINT ''
PRINT 'IMPACT ESTIMAT AL CUR???RII:'
PRINT '============================='
PRINT 'TipActIdentitate problematice: ' + CAST(@TipActProblematice AS VARCHAR(10))
PRINT 'StareCivila problematice: ' + CAST(@StareCivilaProblematice AS VARCHAR(10))
PRINT 'Gen problematice: ' + CAST(@GenProblematice AS VARCHAR(10))
PRINT ''

IF @TipActProblematice > 0 OR @StareCivilaProblematice > 0 OR @GenProblematice > 0
BEGIN
    PRINT '??  ATEN?IE: Exist? valori problematice care trebuie corectate!'
    PRINT 'Rula?i script-ul 99_CleanupLegacyData.sql pentru a le corecta.'
END
ELSE
BEGIN
    PRINT '? Toate valorile sunt deja în formatul corect!'
END

PRINT ''
PRINT 'Diagnosticare complet?!'