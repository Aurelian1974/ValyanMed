/*
    Script alternativ pentru cur??area ?i normalizarea datelor legacy în tabela Persoana
    Versiune cu raport combinat folosind CTE (Common Table Expression)
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
-- Raport combinat de cur??are folosind CTE
-- =============================================
PRINT 'Raport combinat de cur??are:'

WITH RaportCombinata AS (
    SELECT 
        'TipActIdentitate' as Camp,
        TipActIdentitate as Valoare,
        COUNT(*) as Numar,
        1 as TipOrdine
    FROM dbo.Persoana 
    WHERE TipActIdentitate IS NOT NULL
    GROUP BY TipActIdentitate
    
    UNION ALL
    
    SELECT 
        'StareCivila' as Camp,
        StareCivila as Valoare,
        COUNT(*) as Numar,
        2 as TipOrdine
    FROM dbo.Persoana 
    WHERE StareCivila IS NOT NULL
    GROUP BY StareCivila
    
    UNION ALL
    
    SELECT 
        'Gen' as Camp,
        Gen as Valoare,
        COUNT(*) as Numar,
        3 as TipOrdine
    FROM dbo.Persoana 
    WHERE Gen IS NOT NULL
    GROUP BY Gen
)
SELECT 
    Camp,
    Valoare,
    Numar
FROM RaportCombinata
ORDER BY TipOrdine, Numar DESC

-- Sumar general
PRINT 'Sumar general dup? cur??are:'
SELECT 
    COUNT(*) as TotalPersone,
    COUNT(TipActIdentitate) as CuTipAct,
    COUNT(StareCivila) as CuStareCivila,
    COUNT(Gen) as CuGen,
    COUNT(CNP) as CuCNP,
    COUNT(DataNasterii) as CuDataNasterii,
    COUNT(PozitieOrganizatie) as CuPozitie
FROM dbo.Persoana

-- Verificare pentru valori problem? r?mase
PRINT 'Verificare pentru valori problem? r?mase:'
SELECT 'TipActIdentitate problem?' as Tip, TipActIdentitate as Valoare, COUNT(*) as Nr
FROM dbo.Persoana 
WHERE TipActIdentitate IS NOT NULL 
  AND TipActIdentitate NOT IN ('CarteIdentitate', 'Pasaport', 'PermisConducere', 'CertificatNastere', 'Altul')
GROUP BY TipActIdentitate

UNION ALL

SELECT 'StareCivila problem?' as Tip, StareCivila as Valoare, COUNT(*) as Nr
FROM dbo.Persoana 
WHERE StareCivila IS NOT NULL 
  AND StareCivila NOT IN ('Necasatorit', 'Casatorit', 'Divortit', 'Vaduv', 'Concubinaj')
GROUP BY StareCivila

UNION ALL

SELECT 'Gen problem?' as Tip, Gen as Valoare, COUNT(*) as Nr
FROM dbo.Persoana 
WHERE Gen IS NOT NULL 
  AND Gen NOT IN ('Masculin', 'Feminin', 'Neprecizat')
GROUP BY Gen

PRINT 'Cur??area datelor complet?!'