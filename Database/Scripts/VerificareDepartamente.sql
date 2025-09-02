-- =============================================
-- SCRIPT VERIFICARE DEPARTAMENTE IERARHICE
-- Testeaz? integritatea ?i corectitudinea datelor inserdate
-- =============================================

USE [ValyanMed];
GO

PRINT '======================================= ';
PRINT '  VERIFICARE DEPARTAMENTE IERARHICE  ';
PRINT '======================================= ';

-- =============================================
-- 1. STATISTICI GENERALE
-- =============================================
PRINT '';
PRINT '=== 1. STATISTICI GENERALE ===';

SELECT 
    Tip,
    COUNT(*) AS [Numar Departamente]
FROM Departamente
GROUP BY Tip
ORDER BY 
    CASE Tip 
        WHEN 'Categorie' THEN 1 
        WHEN 'Specialitate' THEN 2 
        WHEN 'Subspecialitate' THEN 3 
        ELSE 4 
    END;

-- =============================================
-- 2. VERIFICARE INTEGRITATE IERARHIE
-- =============================================
PRINT '';
PRINT '=== 2. VERIFICARE INTEGRITATE IERARHIE ===';

-- Verific? c? fiecare departament are self-reference
SELECT 
    'Self-references lipsa' AS TipVerificare,
    COUNT(*) AS NumarProbleme
FROM Departamente d
WHERE NOT EXISTS (
    SELECT 1 FROM DepartamenteIerarhie h 
    WHERE h.AncestorID = d.DepartamentID 
    AND h.DescendantID = d.DepartamentID 
    AND h.Nivel = 0
);

-- Verific? leg?turi parent-child corecte
SELECT 
    'Legaturi parent-child invalide' AS TipVerificare,
    COUNT(*) AS NumarProbleme
FROM DepartamenteIerarhie h
WHERE h.Nivel = 1
AND (NOT EXISTS (
    SELECT 1 FROM Departamente p WHERE p.DepartamentID = h.AncestorID
)
OR NOT EXISTS (
    SELECT 1 FROM Departamente c WHERE c.DepartamentID = h.DescendantID
));

-- =============================================
-- 3. STRUCTURA IERARHIC? COMPLET?
-- =============================================
PRINT '';
PRINT '=== 3. STRUCTURA IERARHICA COMPLETA (PRIMELE 25 LINII) ===';

;WITH StructuraCompleta AS (
    SELECT 
        d.DepartamentID,
        d.Nume,
        d.Tip,
        0 as Nivel,
        CAST(d.Nume AS NVARCHAR(1000)) AS Cale,
        CAST(d.Nume AS NVARCHAR(1000)) AS CaleDeLaRadacina
    FROM Departamente d
    WHERE d.Tip = 'Categorie'
    
    UNION ALL
    
    SELECT 
        copil.DepartamentID,
        copil.Nume,
        copil.Tip,
        parinte.Nivel + 1,
        CAST(parinte.Cale + ' > ' + copil.Nume AS NVARCHAR(1000)),
        CAST(parinte.CaleDeLaRadacina + '/' + copil.Nume AS NVARCHAR(1000))
    FROM StructuraCompleta parinte
    INNER JOIN DepartamenteIerarhie h ON parinte.DepartamentID = h.AncestorID AND h.Nivel = 1
    INNER JOIN Departamente copil ON h.DescendantID = copil.DepartamentID
)
SELECT TOP 25
    REPLICATE('??', Nivel) + 
    CASE WHEN Nivel > 0 THEN ' ' ELSE '' END + 
    Nume AS [Structura Vizuala],
    Tip,
    Nivel,
    Cale AS [Calea Completa]
FROM StructuraCompleta
ORDER BY Cale;

-- =============================================
-- 4. STATISTICI PE CATEGORII
-- =============================================
PRINT '';
PRINT '=== 4. STATISTICI DETALIATE PE CATEGORII ===';

;WITH StatisticiCategorii AS (
    SELECT 
        cat.Nume AS Categorie,
        cat.DepartamentID,
        COUNT(CASE WHEN d.Tip = 'Specialitate' THEN 1 END) AS NumarSpecialitati,
        COUNT(CASE WHEN d.Tip = 'Subspecialitate' THEN 1 END) AS NumarSubspecialitati,
        COUNT(*) - 1 AS TotalDescendenti
    FROM Departamente cat
    LEFT JOIN DepartamenteIerarhie h ON cat.DepartamentID = h.AncestorID
    LEFT JOIN Departamente d ON h.DescendantID = d.DepartamentID AND h.Nivel > 0
    WHERE cat.Tip = 'Categorie'
    GROUP BY cat.DepartamentID, cat.Nume
)
SELECT 
    Categorie,
    NumarSpecialitati AS [Nr. Specialitati],
    NumarSubspecialitati AS [Nr. Subspecialitati],
    TotalDescendenti AS [Total Descendenti]
FROM StatisticiCategorii
ORDER BY Categorie;

-- =============================================
-- 5. EXEMPLE SPECIFICE - MEDICINA INTERNA
-- =============================================
PRINT '';
PRINT '=== 5. EXEMPLU DETALIAT - MEDICINA INTERNA ===';

;WITH MedicinaInterna AS (
    SELECT 
        d.DepartamentID,
        d.Nume,
        d.Tip,
        h.Nivel,
        REPLICATE('  ', h.Nivel) + d.Nume AS NumeIndentat
    FROM Departamente d
    INNER JOIN DepartamenteIerarhie h ON d.DepartamentID = h.DescendantID
    WHERE h.AncestorID = (
        SELECT DepartamentID 
        FROM Departamente 
        WHERE Nume = N'Medicina interna' AND Tip = 'Specialitate'
    )
)
SELECT 
    NumeIndentat AS [Medicina Interna - Structura],
    Tip,
    Nivel
FROM MedicinaInterna
ORDER BY Nivel, Nume;

-- =============================================
-- 6. TESTE FUNC?IONALE - C?UT?RI HIERARHICE
-- =============================================
PRINT '';
PRINT '=== 6. TESTE FUNCTIONALE ===';

-- Test 1: G?se?te toate subspecialit??ile din categoria "Medicale"
PRINT 'Test 1: Subspecialitati din categoria Medicale';
SELECT COUNT(*) AS [Numar Subspecialitati Medicale]
FROM DepartamenteIerarhie h
INNER JOIN Departamente anc ON h.AncestorID = anc.DepartamentID
INNER JOIN Departamente descendant ON h.DescendantID = descendant.DepartamentID
WHERE anc.Nume = N'Medicale' 
AND anc.Tip = 'Categorie'
AND descendant.Tip = 'Subspecialitate'
AND h.Nivel = 2;

-- Test 2: G?se?te p?rintele pentru "Cardiologie"
PRINT 'Test 2: Parintele subspecialitatii Cardiologie';
SELECT 
    p.Nume AS [Parinte],
    p.Tip AS [Tip Parinte]
FROM DepartamenteIerarhie h
INNER JOIN Departamente p ON h.AncestorID = p.DepartamentID
INNER JOIN Departamente c ON h.DescendantID = c.DepartamentID
WHERE c.Nume = N'Cardiologie' 
AND c.Tip = 'Subspecialitate'
AND h.Nivel = 1;

-- Test 3: G?se?te toate departamentele din categoria "Critice si urgente"
PRINT 'Test 3: Departamente din categoria Critice si urgente';
SELECT COUNT(*) AS [Total Departamente Critice]
FROM DepartamenteIerarhie h
INNER JOIN Departamente anc ON h.AncestorID = anc.DepartamentID
INNER JOIN Departamente descendant ON h.DescendantID = descendant.DepartamentID
WHERE anc.Nume = N'Critice si urgente' 
AND anc.Tip = 'Categorie'
AND h.Nivel > 0;

-- =============================================
-- 7. VERIFICARE DUPLICATE
-- =============================================
PRINT '';
PRINT '=== 7. VERIFICARE DUPLICATE ===';

-- Verific? duplicate în tabela Departamente
SELECT 
    'Duplicate in Departamente' AS TipVerificare,
    COUNT(*) AS NumarDuplicate
FROM (
    SELECT Nume, Tip, COUNT(*) as cnt
    FROM Departamente
    GROUP BY Nume, Tip
    HAVING COUNT(*) > 1
) duplicates;

-- Verific? duplicate în tabela DepartamenteIerarhie
SELECT 
    'Duplicate in DepartamenteIerarhie' AS TipVerificare,
    COUNT(*) AS NumarDuplicate
FROM (
    SELECT AncestorID, DescendantID, Nivel, COUNT(*) as cnt
    FROM DepartamenteIerarhie
    GROUP BY AncestorID, DescendantID, Nivel
    HAVING COUNT(*) > 1
) duplicates;

-- =============================================
-- 8. EXEMPLE DE QUERIES PRACTICE
-- =============================================
PRINT '';
PRINT '=== 8. EXEMPLE QUERIES PRACTICE ===';

-- Query 1: Toate subspecialit??ile de radiologie
PRINT 'Query 1: Subspecialitati de radiologie';
SELECT 
    sub.Nume AS [Subspecialitate Radiologie]
FROM DepartamenteIerarhie h
INNER JOIN Departamente parent ON h.AncestorID = parent.DepartamentID
INNER JOIN Departamente sub ON h.DescendantID = sub.DepartamentID
WHERE parent.Nume = N'Radiologie si imagistica medicala'
AND parent.Tip = 'Specialitate'
AND sub.Tip = 'Subspecialitate'
AND h.Nivel = 1
ORDER BY sub.Nume;

-- Query 2: Toate departamentele de transplant
PRINT 'Query 2: Departamente de transplant';
SELECT 
    sub.Nume AS [Tip Transplant]
FROM DepartamenteIerarhie h
INNER JOIN Departamente parent ON h.AncestorID = parent.DepartamentID
INNER JOIN Departamente sub ON h.DescendantID = sub.DepartamentID
WHERE parent.Nume = N'Transplant'
AND parent.Tip = 'Specialitate'
AND sub.Tip = 'Subspecialitate'
AND h.Nivel = 1
ORDER BY sub.Nume;

PRINT '';
PRINT '======================================= ';
PRINT '    VERIFICARE COMPLETA FINALIZATA   ';
PRINT '======================================= ';