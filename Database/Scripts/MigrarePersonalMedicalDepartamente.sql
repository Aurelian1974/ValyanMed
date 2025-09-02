-- =============================================
-- MIGRARE TABELA PERSONALMEDICAL PENTRU DEPARTAMENTE IERARHICE
-- Înlocuie?te enum-urile cu rela?ii c?tre structura ierarhic?
-- =============================================

USE [ValyanMed];
GO

-- =============================================
-- PASUL 1: BACKUP DATE EXISTENTE
-- =============================================
-- Creeaz? tabel? temporar? pentru backup
IF OBJECT_ID('PersonalMedical_Backup', 'U') IS NOT NULL
    DROP TABLE PersonalMedical_Backup;

SELECT * 
INTO PersonalMedical_Backup 
FROM PersonalMedical;

PRINT 'Backup complet realizat: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' înregistr?ri salvate';

-- =============================================
-- PASUL 2: AD?UGARE COLOANE NOI
-- =============================================

-- Adaug? coloanele pentru structura ierarhic?
ALTER TABLE PersonalMedical 
ADD 
    CategorieID UNIQUEIDENTIFIER NULL,
    SpecializareID UNIQUEIDENTIFIER NULL,
    SubspecializareID UNIQUEIDENTIFIER NULL;

-- Adaug? foreign keys c?tre tabela Departamente
ALTER TABLE PersonalMedical 
ADD CONSTRAINT FK_PersonalMedical_Categorie 
    FOREIGN KEY (CategorieID) REFERENCES Departamente(DepartamentID);

ALTER TABLE PersonalMedical 
ADD CONSTRAINT FK_PersonalMedical_Specializare 
    FOREIGN KEY (SpecializareID) REFERENCES Departamente(DepartamentID);

ALTER TABLE PersonalMedical 
ADD CONSTRAINT FK_PersonalMedical_Subspecializare 
    FOREIGN KEY (SubspecializareID) REFERENCES Departamente(DepartamentID);

-- Adaug? indexuri pentru performan??
CREATE NONCLUSTERED INDEX IX_PersonalMedical_CategorieID 
ON PersonalMedical (CategorieID);

CREATE NONCLUSTERED INDEX IX_PersonalMedical_SpecializareID 
ON PersonalMedical (SpecializareID);

CREATE NONCLUSTERED INDEX IX_PersonalMedical_SubspecializareID 
ON PersonalMedical (SubspecializareID);

PRINT 'Coloane noi ad?ugate cu succes';

-- =============================================
-- PASUL 3: MIGRARE DATE EXISTENTE
-- =============================================

-- Actualizeaz? CategorieID bazat pe Departament existent
UPDATE pm
SET CategorieID = d.DepartamentID
FROM PersonalMedical pm
INNER JOIN Departamente d ON d.Tip = 'Categorie'
WHERE pm.Departament IS NOT NULL
  AND d.Nume = CASE 
    WHEN UPPER(pm.Departament) LIKE '%CARDIO%' OR UPPER(pm.Departament) LIKE '%MEDICINA INTERNA%' 
         OR UPPER(pm.Departament) LIKE '%NEUROLOG%' OR UPPER(pm.Departament) LIKE '%PSIHIATRI%'
         OR UPPER(pm.Departament) LIKE '%DERMATOLOG%' OR UPPER(pm.Departament) LIKE '%BOLI INFECT%'
         OR UPPER(pm.Departament) LIKE '%GERIATRI%' OR UPPER(pm.Departament) LIKE '%RECUPERARE%' THEN 'Medicale'
    WHEN UPPER(pm.Departament) LIKE '%CHIRURG%' OR UPPER(pm.Departament) LIKE '%ORL%' 
         OR UPPER(pm.Departament) LIKE '%OFTALMOLOG%' OR UPPER(pm.Departament) LIKE '%UROLOG%'
         OR UPPER(pm.Departament) LIKE '%GINECOLOG%' OR UPPER(pm.Departament) LIKE '%ORTOPED%' THEN 'Chirurgicale'
    WHEN UPPER(pm.Departament) LIKE '%PEDIATR%' OR UPPER(pm.Departament) LIKE '%NEONATO%' THEN 'Pediatrice'
    WHEN UPPER(pm.Departament) LIKE '%URGENTA%' OR UPPER(pm.Departament) LIKE '%TERAPIE INTENSIVA%'
         OR UPPER(pm.Departament) LIKE '%ICU%' OR UPPER(pm.Departament) LIKE '%ANESTEZIE%' THEN 'Critice si urgente'
    WHEN UPPER(pm.Departament) LIKE '%RADIOLOG%' OR UPPER(pm.Departament) LIKE '%LABORATOR%'
         OR UPPER(pm.Departament) LIKE '%FARMACIE%' OR UPPER(pm.Departament) LIKE '%NUTRITIE%' THEN 'Suport si diagnostic'
    WHEN UPPER(pm.Departament) LIKE '%TRANSPLANT%' OR UPPER(pm.Departament) LIKE '%IVF%'
         OR UPPER(pm.Departament) LIKE '%MEDICINA SPORTIVA%' OR UPPER(pm.Departament) LIKE '%GENETIC%' THEN 'Speciale'
    ELSE 'Medicale'  -- default
  END;

-- Actualizeaz? SpecializareID bazat pe Specializare existent?
UPDATE pm
SET SpecializareID = d.DepartamentID
FROM PersonalMedical pm
INNER JOIN Departamente d ON d.Tip = 'Specialitate'
WHERE pm.Specializare IS NOT NULL
  AND (
    UPPER(d.Nume) LIKE '%' + UPPER(pm.Specializare) + '%' OR
    UPPER(pm.Specializare) LIKE '%' + UPPER(d.Nume) + '%'
  );

-- Pentru specializ?rile care nu se potrivesc exact, încearc? map?ri specifice
UPDATE pm
SET SpecializareID = d.DepartamentID
FROM PersonalMedical pm
INNER JOIN Departamente d ON d.Tip = 'Specialitate'
WHERE pm.SpecializareID IS NULL
  AND pm.Specializare IS NOT NULL
  AND d.Nume = CASE 
    WHEN UPPER(pm.Specializare) LIKE '%CARDIO%' THEN 'Medicina interna'  -- va fi subspecializare
    WHEN UPPER(pm.Specializare) LIKE '%PNEUMOLOG%' THEN 'Medicina interna'
    WHEN UPPER(pm.Specializare) LIKE '%GASTROENTEROLOG%' THEN 'Medicina interna'
    WHEN UPPER(pm.Specializare) LIKE '%NEFROLOG%' THEN 'Medicina interna'
    WHEN UPPER(pm.Specializare) LIKE '%ENDOCRINOLOG%' THEN 'Medicina interna'
    WHEN UPPER(pm.Specializare) LIKE '%HEMATOLOG%' THEN 'Medicina interna'
    WHEN UPPER(pm.Specializare) LIKE '%ONCOLOG%' THEN 'Medicina interna'
    WHEN UPPER(pm.Specializare) LIKE '%REUMATOLOG%' THEN 'Medicina interna'
    WHEN UPPER(pm.Specializare) LIKE '%ALERGOLOG%' THEN 'Medicina interna'
    WHEN UPPER(pm.Specializare) LIKE '%RADIOLOG%' THEN 'Radiologie si imagistica medicala'
    WHEN UPPER(pm.Specializare) LIKE '%LABORATOR%' THEN 'Laborator analize medicale'
    WHEN UPPER(pm.Specializare) LIKE '%TRANSPLANT%' THEN 'Transplant'
    ELSE NULL
  END;

-- Actualizeaz? SubspecializareID pentru specializ?rile din Medicina interna
UPDATE pm
SET SubspecializareID = d.DepartamentID
FROM PersonalMedical pm
INNER JOIN Departamente d ON d.Tip = 'Subspecialitate'
INNER JOIN DepartamenteIerarhie h ON d.DepartamentID = h.DescendantID
INNER JOIN Departamente parent ON h.AncestorID = parent.DepartamentID
WHERE pm.Specializare IS NOT NULL
  AND parent.Nume = 'Medicina interna'
  AND h.Nivel = 1
  AND (
    UPPER(d.Nume) LIKE '%' + UPPER(pm.Specializare) + '%' OR
    UPPER(pm.Specializare) LIKE '%' + UPPER(d.Nume) + '%'
  );

PRINT 'Date migrarile cu succes';

-- =============================================
-- PASUL 4: VALIDARE MIGRARE
-- =============================================

-- Statistici migrare
SELECT 
    'Total înregistr?ri' AS Descriere,
    COUNT(*) AS Numar
FROM PersonalMedical

UNION ALL

SELECT 
    'Cu Categorie setat?' AS Descriere,
    COUNT(*) AS Numar
FROM PersonalMedical
WHERE CategorieID IS NOT NULL

UNION ALL

SELECT 
    'Cu Specializare setat?' AS Descriere,
    COUNT(*) AS Numar
FROM PersonalMedical
WHERE SpecializareID IS NOT NULL

UNION ALL

SELECT 
    'Cu Subspecializare setat?' AS Descriere,
    COUNT(*) AS Numar
FROM PersonalMedical
WHERE SubspecializareID IS NOT NULL;

-- Verific? înregistr?rile care nu au fost migrate
SELECT 
    PersonalID,
    Nume,
    Prenume,
    Departament AS DepartamentVechi,
    Specializare AS SpecializareVeche,
    CategorieID,
    SpecializareID,
    SubspecializareID
FROM PersonalMedical
WHERE Departament IS NOT NULL 
  AND CategorieID IS NULL;

PRINT 'Migrare PersonalMedical complet?!';