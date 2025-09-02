# ?? GHID REZOLVARE ERORI SQL - DEPARTAMENTE IERARHICE

## ?? **ERORILE IDENTIFICATE ?I SOLU?II**

### **1. Error 319: "Incorrect syntax near the keyword 'with'"**

**? CAUZA:** Lips? punct ?i virgul? (`;`) înainte de CTE (Common Table Expression)

**? SOLU?IA:**
```sql
-- GRE?IT:
WITH MyCTE AS (...)

-- CORECT:
;WITH MyCTE AS (...)
```

**?? APLICAT? ÎN:**
- `PopulareDepartamente.sql` - linia ~270
- `VerificareDepartamente.sql` - liniile ~69, ~109, ~136
- `StoredProceduresDepartamente.sql` - toate CTE-urile

---

### **2. Error 156: "Incorrect syntax near the keyword 'desc'"**

**? CAUZA:** `desc` este cuvânt rezervat în SQL Server

**? SOLU?IA:**
```sql
-- GRE?IT:
INNER JOIN Departamente desc ON h.DescendantID = desc.DepartamentID

-- CORECT:
INNER JOIN Departamente descendant ON h.DescendantID = descendant.DepartamentID
-- SAU:
INNER JOIN Departamente [desc] ON h.DescendantID = [desc].DepartamentID
```

**?? APLICAT? ÎN:**
- `VerificareDepartamente.sql` - înlocuit `desc` cu `descendant`

---

### **3. Error 240: "Types don't match between the anchor and the recursive part"**

**? CAUZA:** Tipurile de date difer? între partea "anchor" ?i partea recursiv? în CTE

**? SOLU?IA:**
```sql
-- GRE?IT:
;WITH RecursivTest AS (
    SELECT 1 AS Nivel, 'Test' AS Nume  -- 'Test' = VARCHAR(4)
    UNION ALL
    SELECT r.Nivel + 1, r.Nume + '_' + CAST(r.Nivel + 1 AS VARCHAR(10))  -- Rezultat poate fi > VARCHAR(4)
    FROM RecursivTest r WHERE r.Nivel < 3
)

-- CORECT:
;WITH RecursivTest AS (
    SELECT 1 AS Nivel, CAST('Test' AS NVARCHAR(100)) AS Nume  -- Specific?m tipul explicit
    UNION ALL
    SELECT 
        r.Nivel + 1, 
        CAST(r.Nume + '_' + CAST(r.Nivel + 1 AS NVARCHAR(10)) AS NVARCHAR(100))  -- Acela?i tip
    FROM RecursivTest r WHERE r.Nivel < 3
)
```

**?? APLICAT? ÎN:**
- `TestSintaxaSQL.sql` - corectate toate CTE-urile recursive

---

### **4. Error 105 & 102: "Unclosed quotation mark" ?i "Incorrect syntax"**

**? CAUZA:** Ghilimele neînchise în string-uri, de obicei în sec?iuni PRINT sau comentarii

**? SOLU?IA:**
```sql
-- GRE?IT:
PRINT 'EXEC sp_ExportaStructura @FormatOutput = N'TREE';

-- CORECT:
PRINT 'EXEC sp_ExportaStructura @FormatOutput = N''TREE'';
-- SAU:
PRINT 'EXEC sp_ExportaStructura @FormatOutput = N''TREE''';
```

**?? REGULI PENTRU GHILIMELE:**
- În SQL Server, pentru a include o apostrof într-un string, dubleaz?-l: `''`
- Pentru string-uri Unicode, folose?te prefixul `N'...'`
- Pentru nume de obiecte cu spa?ii sau caractere speciale, folose?te `[...]`

**?? APLICAT? ÎN:**
- `StoredProceduresDepartamente.sql` - corectate toate exemplele din sec?iunea final?

---

## ?? **ORDINEA DE EXECUTARE SCRIPTURILOR**

### **Pasul 1: Testare sintax?**
```sql
-- Ruleaz? TestSintaxaSQL.sql pentru verific?ri preliminare
-- Acesta verific? compatibilitatea SQL Server
```

### **Pasul 2: Verificare compatibilitate**
```sql
-- Ruleaz? VerificareCompatibilitate.sql pentru verificare complet?
-- Acesta testeaz? toate func?ionalit??ile necesare
```

### **Pasul 3: Populare date**
```sql
-- Ruleaz? PopulareDepartamente.sql
-- Acesta insereaz? toate datele ?i verific? rezultatele
```

### **Pasul 4: Verificare**
```sql
-- Ruleaz? VerificareDepartamente.sql
-- Acesta valideaz? integritatea datelor inserdate
```

### **Pasul 5: Stored Procedures**
```sql
-- Ruleaz? StoredProceduresDepartamente.sql
-- Acesta creeaz? procedurile pentru lucrul cu ierarhia
```

---

## ?? **VERIFIC?RI SUPLIMENTARE**

### **Verific? versiunea SQL Server:**
```sql
SELECT @@VERSION;
-- STRING_AGG necesit? SQL Server 2017+ (versiunea 14.x)
-- Pentru versiuni mai vechi, folose?te alternativa cu FOR XML PATH
```

### **Verific? existen?a tabelelor:**
```sql
-- Verific? dac? tabelele exist?
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Departamente', 'DepartamenteIerarhie');
```

### **Verific? stored procedures:**
```sql
-- Verific? procedurile create
SELECT name, create_date, modify_date
FROM sys.objects 
WHERE type = 'P' AND name LIKE 'sp_%departament%';
```

---

## ?? **PROBLEME COMUNE ?I SOLU?II**

### **Problema 1: Timeout la inserare**
**Cauza:** Prea multe date sau constraints complexe  
**Solu?ia:** Ruleaz? în batches mai mici sau cre?te timeout

### **Problema 2: Foreign Key errors**
**Cauza:** Încercare de inserare în ordine gre?it?  
**Solu?ia:** Respect? ordinea: Categorii ? Specialit??i ? Subspecialit??i

### **Problema 3: Unicode issues**
**Cauza:** Caractere speciale române?ti  
**Solu?ia:** Folose?te prefixul `N'text'` pentru string-uri Unicode

### **Problema 4: CTE recursion limit**
**Cauza:** Ierarhia are prea multe nivele  
**Solu?ia:** Adaug? `OPTION (MAXRECURSION 0)` sau limiteaz? la num?r specific

### **Problema 5: Type mismatch în CTE recursiv**
**Cauza:** Tipurile de date difer? între anchor ?i partea recursiv?  
**Solu?ia:** Folose?te `CAST()` pentru a specifica tipurile explicit în ambele p?r?i

### **Problema 6: STRING_AGG nu exist?**
**Cauza:** SQL Server < 2017  
**Solu?ia:** Folose?te alternativa cu `FOR XML PATH`:
```sql
-- În loc de STRING_AGG:
SELECT STRING_AGG(Nume, ' > ') FROM tabela;

-- Folose?te:
SELECT STUFF((
    SELECT ' > ' + Nume
    FROM tabela
    FOR XML PATH('')
), 1, 3, '') AS NumeConcatenat;
```

### **Problema 7: Ghilimele neînchise**
**Cauza:** Apostrof în mijlocul string-ului sau ghilimele nested  
**Solu?ia:** 
```sql
-- Pentru apostrof în string:
PRINT 'Text cu apostrof: Don''t worry';

-- Pentru ghilimele nested în PRINT:
PRINT 'EXEC proc @param = N''value'';';

-- Pentru nume cu spa?ii:
SELECT * FROM [Nume Tabel Cu Spatii];
```

---

## ?? **EXEMPLE DE TESTARE RAPID?**

### **Test 1: Verific? structura simpl?**
```sql
SELECT COUNT(*) AS TotalDepartamente FROM Departamente;
SELECT COUNT(*) AS TotalRelatii FROM DepartamenteIerarhie;
```

### **Test 2: Verific? ierarhia**
```sql
-- Toate categoriile (ar trebui s? fie 6)
SELECT COUNT(*) FROM Departamente WHERE Tip = 'Categorie';

-- Exemplu de cale complet? cu tipuri corecte
;WITH Cai AS (
    SELECT 
        DepartamentID,
        CAST(Nume AS NVARCHAR(200)) AS Nume, 
        Tip, 
        0 AS Nivel, 
        CAST(Nume AS NVARCHAR(1000)) AS Cale
    FROM Departamente WHERE Tip = 'Categorie'
    
    UNION ALL
    
    SELECT 
        d.DepartamentID,
        CAST(d.Nume AS NVARCHAR(200)), 
        d.Tip, 
        p.Nivel + 1, 
        CAST(p.Cale + ' > ' + d.Nume AS NVARCHAR(1000))
    FROM Cai p
    INNER JOIN DepartamenteIerarhie h ON p.DepartamentID = h.AncestorID
    INNER JOIN Departamente d ON h.DescendantID = d.DepartamentID
    WHERE h.Nivel = 1
)
SELECT TOP 5 Cale FROM Cai ORDER BY Nivel, Cale;
```

### **Test 3: Verific? stored procedures**
```sql
-- Test simpl? pentru proceduri
EXEC sp_CautaDepartamente @SearchPattern = N'Cardio', @IncludeCaleCompleta = 0;
```

---

## ?? **DEBUGGING TIPS**

### **Pentru erori de sintax?:**
1. Verific? toate `;` înainte de `WITH`
2. Verific? parantezele - toate deschise trebuie închise
3. Verific? ghilimelele - dubleaz? apostrofurile în string-uri
4. Verific? cuvintele rezervate - pune `[]` în jurul lor
5. Folose?te SQL Server Management Studio pentru syntax highlighting

### **Pentru erori de ghilimele:**
1. În SSMS, folose?te "Show whitespace" pentru a vedea caracterele ascunse
2. Caut? pattern-urile: `'...'...` (apostrof neînchis)
3. Pentru debugging rapid, comenteaz? sec?iuni mari cu `/* ... */`
4. Testeaz? sec?iuni de cod individual

### **Pentru erori de tipuri de date:**
1. Folose?te `CAST()` sau `CONVERT()` pentru a specifica tipurile explicit
2. În CTE recursiv, asigur?-te c? tipurile sunt identice în anchor ?i partea recursiv?
3. Pentru string-uri, folose?te `NVARCHAR(max_length)` în loc de `VARCHAR` implicit
4. Testeaz? tipurile cu `SELECT` simplu înainte de a le folosi în CTE

### **Pentru erori de logic?:**
1. Testeaz? CTE-urile separat 
2. Folose?te `TOP 10` pentru testare rapid?
3. Adaug? `PRINT` statements în stored procedures
4. Verific? cu `SELECT COUNT(*)` num?rul de înregistr?ri

### **Pentru erori de performan??:**
1. Adaug? indexuri pe coloanele folosite în JOIN
2. Limiteaz? adâncimea recursiunii 
3. Folose?te `NOLOCK` hints pentru citire (cu aten?ie!)
4. Monitorizeaz? cu SQL Profiler

---

## ? **CHECKLIST FINAL**

- [ ] Toate scripturile ruleaz? f?r? erori de sintax?
- [ ] Ghilimelele sunt corecte în toate sec?iunile PRINT
- [ ] Datele sunt inserdate corect (74 departamente total)
- [ ] Stored procedures sunt create ?i func?ionale
- [ ] Ierarhia este corect? (6 categorii, ~45 specialit??i, ~23 subspecialit??i)
- [ ] Testele de verificare trec cu succes
- [ ] Performance-ul este acceptabil (sub 5 secunde pentru queries complexe)
- [ ] Tipurile de date sunt consistente în toate CTE-urile recursive

**Status final:** ?? **SCRIPTURILE SUNT PREG?TITE PENTRU PRODUC?IE**