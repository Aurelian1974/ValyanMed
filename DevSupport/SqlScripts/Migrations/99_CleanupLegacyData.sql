/*
    Script pentru cur??area ?i normalizarea datelor legacy în tabela Persoana
    Mapeaz? valorile vechi la noile enumer?ri standard
*/

-- =============================================
-- Cur??area valorilor TipActIdentitate
-- =============================================
PRINT 'Cur??area TipActIdentitate...'

-- Maparea valorilor legacy la noile enumer?ri
UPDATE dbo.Persoana 
SET TipActIdentitate = 'CarteIdentitate'
WHERE TipActIdentitate IN ('CI', 'CARTE IDENTITATE', 'Carte identitate', 'ci')

UPDATE dbo.Persoana 
SET TipActIdentitate = 'Pasaport'
WHERE TipActIdentitate IN ('PASAPORT', 'pasaport', 'Pasaport')

UPDATE dbo.Persoana 
SET TipActIdentitate = 'PermisConducere'
WHERE TipActIdentitate IN ('PERMIS CONDUCERE', 'Permis conducere', 'permis')

UPDATE dbo.Persoana 
SET TipActIdentitate = 'CertificatNastere'
WHERE TipActIdentitate IN ('CERTIFICAT NASTERE', 'Certificat nastere', 'certificat')

-- Toate celelalte valori necunoscute se mapeaz? la 'Altul'
UPDATE dbo.Persoana 
SET TipActIdentitate = 'Altul'
WHERE TipActIdentitate IS NOT NULL 
  AND TipActIdentitate NOT IN ('CarteIdentitate', 'Pasaport', 'PermisConducere', 'CertificatNastere', 'Altul')

-- =============================================
-- Cur??area valorilor StareCivila
-- =============================================
PRINT 'Cur??area StareCivila...'

UPDATE dbo.Persoana 
SET StareCivila = 'Necasatorit'
WHERE StareCivila IN ('NECASATORIT', 'necasatorit', 'Necasatorit', 'CELIBATAR', 'celibatar')

UPDATE dbo.Persoana 
SET StareCivila = 'Casatorit'
WHERE StareCivila IN ('CASATORIT', 'casatorit', 'Casatorit', 'MARIAJ', 'mariaj')

UPDATE dbo.Persoana 
SET StareCivila = 'Divortit'
WHERE StareCivila IN ('DIVORTIT', 'divortit', 'Divortit', 'DIVORTAT', 'divortat')

UPDATE dbo.Persoana 
SET StareCivila = 'Vaduv'
WHERE StareCivila IN ('VADUV', 'vaduv', 'Vaduv', 'VADOVA', 'vadova')

UPDATE dbo.Persoana 
SET StareCivila = 'Concubinaj'
WHERE StareCivila IN ('CONCUBINAJ', 'concubinaj', 'Concubinaj', 'PARTENERIAT', 'parteneriat')

-- =============================================
-- Cur??area valorilor Gen
-- =============================================
PRINT 'Cur??area Gen...'

UPDATE dbo.Persoana 
SET Gen = 'Masculin'
WHERE Gen IN ('M', 'm', 'MASCULIN', 'masculin', 'Masculin', 'BARBAT', 'barbat')

UPDATE dbo.Persoana 
SET Gen = 'Feminin'
WHERE Gen IN ('F', 'f', 'FEMININ', 'feminin', 'Feminin', 'FEMEIE', 'femeie')

UPDATE dbo.Persoana 
SET Gen = 'Neprecizat'
WHERE Gen IS NOT NULL 
  AND Gen NOT IN ('Masculin', 'Feminin', 'Neprecizat')

-- =============================================
-- Raport de cur??are
-- =============================================
PRINT 'Raport final de cur??are:'

-- Raportul pentru TipActIdentitate
PRINT 'Distribu?ia TipActIdentitate:'
SELECT 
    'TipActIdentitate' as Camp,
    TipActIdentitate as Valoare,
    COUNT(*) as Numar
FROM dbo.Persoana 
WHERE TipActIdentitate IS NOT NULL
GROUP BY TipActIdentitate
ORDER BY COUNT(*) DESC

-- Raportul pentru StareCivila
PRINT 'Distribu?ia StareCivila:'
SELECT 
    'StareCivila' as Camp,
    StareCivila as Valoare,
    COUNT(*) as Numar
FROM dbo.Persoana 
WHERE StareCivila IS NOT NULL
GROUP BY StareCivila
ORDER BY COUNT(*) DESC

-- Raportul pentru Gen
PRINT 'Distribu?ia Gen:'
SELECT 
    'Gen' as Camp,
    Gen as Valoare,
    COUNT(*) as Numar
FROM dbo.Persoana 
WHERE Gen IS NOT NULL
GROUP BY Gen
ORDER BY COUNT(*) DESC

-- Sumar general
PRINT 'Sumar general:'
SELECT 
    COUNT(*) as TotalPersone,
    COUNT(TipActIdentitate) as CuTipAct,
    COUNT(StareCivila) as CuStareCivila,
    COUNT(Gen) as CuGen,
    COUNT(CNP) as CuCNP
FROM dbo.Persoana

PRINT 'Cur??area datelor complet?!'