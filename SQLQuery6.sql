WITH CaiIerarhice AS (
    -- CTE recursiv pentru a construi căile
    SELECT 
        d.DepartamentID,
        d.Nume,
        d.Tip,
        0 as Nivel,
        CAST(d.Nume AS NVARCHAR(1000)) AS Cale
    FROM Departamente d
    WHERE d.Tip = 'Categorie' -- Start de la categorii (root)
    
    UNION ALL
    
    SELECT 
        copil.DepartamentID,
        copil.Nume,
        copil.Tip,
        parinte.Nivel + 1,
        CAST(parinte.Cale + ' > ' + copil.Nume AS NVARCHAR(1000))
    FROM CaiIerarhice parinte
    INNER JOIN DepartamenteIerarhie h ON parinte.DepartamentID = h.AncestorID AND h.Nivel = 1
    INNER JOIN Departamente copil ON h.DescendantID = copil.DepartamentID
)
SELECT 
    Cale AS [Calea Completa],
    Tip,
    Nivel
FROM CaiIerarhice
ORDER BY Cale;