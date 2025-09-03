---------------------------------------------------
-- 1. Creare tabele
---------------------------------------------------
IF OBJECT_ID('DepartamenteIerarhie', 'U') IS NOT NULL DROP TABLE DepartamenteIerarhie;
IF OBJECT_ID('Departamente', 'U') IS NOT NULL DROP TABLE Departamente;

CREATE TABLE Departamente (
    DepartamentID UNIQUEIDENTIFIER NOT NULL 
        DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    Nume NVARCHAR(200) NOT NULL,
    Tip NVARCHAR(50) NOT NULL   -- ex: Categorie, Specialitate, Subspecialitate
);

CREATE TABLE DepartamenteIerarhie (
    AncestorID UNIQUEIDENTIFIER NOT NULL,
    DescendantID UNIQUEIDENTIFIER NOT NULL,
    Nivel INT NOT NULL,   -- 0 = self, 1 = copil direct, 2+ = descendent la distanța

    CONSTRAINT PK_DepartamenteIerarhie PRIMARY KEY (AncestorID, DescendantID),
    CONSTRAINT FK_DepartamenteIerarhie_Ancestor FOREIGN KEY (AncestorID) REFERENCES Departamente(DepartamentID),
    CONSTRAINT FK_DepartamenteIerarhie_Descendant FOREIGN KEY (DescendantID) REFERENCES Departamente(DepartamentID)
);

---------------------------------------------------
-- 2. Procedura de inserare
---------------------------------------------------
--IF OBJECT_ID('InsertDepartament', 'P') IS NOT NULL DROP PROCEDURE InsertDepartament;
--GO

--CREATE PROCEDURE InsertDepartament
--    @Nume NVARCHAR(200),
--    @Tip NVARCHAR(50),
--    @ParentID UNIQUEIDENTIFIER = NULL
--AS
--BEGIN
--    SET NOCOUNT ON;

--    DECLARE @NewID UNIQUEIDENTIFIER = NEWSEQUENTIALID();

--    -- Inseram nodul
--    INSERT INTO Departamente (DepartamentID, Nume, Tip)
--    VALUES (@NewID, @Nume, @Tip);

--    -- Fiecare nod este descendentul lui însusi
--    INSERT INTO DepartamenteIerarhie (AncestorID, DescendantID, Nivel)
--    VALUES (@NewID, @NewID, 0);

--    -- Daca are parinte, adaugam legaturi pentru toți stramosii parintelui
--    IF @ParentID IS NOT NULL
--    BEGIN
--        INSERT INTO DepartamenteIerarhie (AncestorID, DescendantID, Nivel)
--        SELECT AncestorID, @NewID, Nivel + 1
--        FROM DepartamenteIerarhie
--        WHERE DescendantID = @ParentID;
--    END
--END;
--GO

---------------------------------------------------
-- 3. Inserare ierarhii
---------------------------------------------------

-- CATEGORIE: Medicale
DECLARE @Medicale UNIQUEIDENTIFIER, @MedicinaInterna UNIQUEIDENTIFIER, @Neurologie UNIQUEIDENTIFIER, @Psihiatrie UNIQUEIDENTIFIER;

EXEC InsertDepartament N'Medicale', N'Categorie', NULL;
SET @Medicale = (SELECT DepartamentID FROM Departamente WHERE Nume = N'Medicale');

-- Specialitați medicale
EXEC InsertDepartament N'Medicina interna', N'Specialitate', @Medicale;
SET @MedicinaInterna = (SELECT DepartamentID FROM Departamente WHERE Nume = N'Medicina interna');

EXEC InsertDepartament N'Neurologie', N'Specialitate', @Medicale;
SET @Neurologie = (SELECT DepartamentID FROM Departamente WHERE Nume = N'Neurologie');

EXEC InsertDepartament N'Psihiatrie', N'Specialitate', @Medicale;
SET @Psihiatrie = (SELECT DepartamentID FROM Departamente WHERE Nume = N'Psihiatrie');

-- Subspecialitați Medicina Interna
EXEC InsertDepartament N'Cardiologie', N'Subspecialitate', @MedicinaInterna;
EXEC InsertDepartament N'Pneumologie / Medicina respiratorie', N'Subspecialitate', @MedicinaInterna;
EXEC InsertDepartament N'Gastroenterologie', N'Subspecialitate', @MedicinaInterna;
EXEC InsertDepartament N'Nefrologie', N'Subspecialitate', @MedicinaInterna;
EXEC InsertDepartament N'Endocrinologie', N'Subspecialitate', @MedicinaInterna;
EXEC InsertDepartament N'Hematologie', N'Subspecialitate', @MedicinaInterna;

-- Subspecialitați Neurologie
EXEC InsertDepartament N'Neurologie vasculara', N'Subspecialitate', @Neurologie;
EXEC InsertDepartament N'Neurologie pediatrica', N'Subspecialitate', @Neurologie;

-- Subspecialitați Psihiatrie
EXEC InsertDepartament N'Psihiatrie adult', N'Subspecialitate', @Psihiatrie;
EXEC InsertDepartament N'Psihiatrie pediatrica', N'Subspecialitate', @Psihiatrie;
EXEC InsertDepartament N'Psihiatrie geriatrica', N'Subspecialitate', @Psihiatrie;


-- CATEGORIE: Chirurgicale
DECLARE @Chirurgicale UNIQUEIDENTIFIER, @ChirurgieGenerala UNIQUEIDENTIFIER, @Ortopedie UNIQUEIDENTIFIER, @ORL UNIQUEIDENTIFIER;

EXEC InsertDepartament N'Chirurgicale', N'Categorie', NULL;
SET @Chirurgicale = (SELECT DepartamentID FROM Departamente WHERE Nume = N'Chirurgicale');

-- Specialitați chirurgicale
EXEC InsertDepartament N'Chirurgie generala', N'Specialitate', @Chirurgicale;
SET @ChirurgieGenerala = (SELECT DepartamentID FROM Departamente WHERE Nume = N'Chirurgie generala');

EXEC InsertDepartament N'Ortopedie si traumatologie', N'Specialitate', @Chirurgicale;
SET @Ortopedie = (SELECT DepartamentID FROM Departamente WHERE Nume = N'Ortopedie si traumatologie');

EXEC InsertDepartament N'Otorinolaringologie (ORL)', N'Specialitate', @Chirurgicale;
SET @ORL = (SELECT DepartamentID FROM Departamente WHERE Nume = N'Otorinolaringologie (ORL)');

-- Subspecialitați chirurgie
EXEC InsertDepartament N'Chirurgie oncologica', N'Subspecialitate', @ChirurgieGenerala;
EXEC InsertDepartament N'Chirurgie pediatrica', N'Subspecialitate', @ChirurgieGenerala;

EXEC InsertDepartament N'Ortopedie pediatrica', N'Subspecialitate', @Ortopedie;
EXEC InsertDepartament N'Chirurgie spinala', N'Subspecialitate', @Ortopedie;

EXEC InsertDepartament N'ORL pediatric', N'Subspecialitate', @ORL;


-- CATEGORIE: Materno-infantile
DECLARE @MaternoInfantile UNIQUEIDENTIFIER, @Pediatrie UNIQUEIDENTIFIER, @Ginecologie UNIQUEIDENTIFIER;

EXEC InsertDepartament N'Materno-infantile', N'Categorie', NULL;
SET @MaternoInfantile = (SELECT DepartamentID FROM Departamente WHERE Nume = N'Materno-infantile');

-- Specialitați
EXEC InsertDepartament N'Pediatrie', N'Specialitate', @MaternoInfantile;
SET @Pediatrie = (SELECT DepartamentID FROM Departamente WHERE Nume = N'Pediatrie');

EXEC InsertDepartament N'Obstetrica-Ginecologie', N'Specialitate', @MaternoInfantile;
SET @Ginecologie = (SELECT DepartamentID FROM Departamente WHERE Nume = N'Obstetrica-Ginecologie');

-- Subspecialitați
EXEC InsertDepartament N'Neonatologie', N'Subspecialitate', @Pediatrie;
EXEC InsertDepartament N'Pediatrie generala', N'Subspecialitate', @Pediatrie;

EXEC InsertDepartament N'Medicina materno-fetala', N'Subspecialitate', @Ginecologie;
EXEC InsertDepartament N'Ginecologie oncologica', N'Subspecialitate', @Ginecologie;


-- CATEGORIE: Suport si laborator
DECLARE @Suport UNIQUEIDENTIFIER, @Lab UNIQUEIDENTIFIER;

EXEC InsertDepartament N'Suport si laborator', N'Categorie', NULL;
SET @Suport = (SELECT DepartamentID FROM Departamente WHERE Nume = N'Suport si laborator');

-- Specialitați
EXEC InsertDepartament N'Laborator analize medicale', N'Specialitate', @Suport;
SET @Lab = (SELECT DepartamentID FROM Departamente WHERE Nume = N'Laborator analize medicale');

EXEC InsertDepartament N'Anatomie patologica', N'Specialitate', @Suport;
EXEC InsertDepartament N'Farmacie', N'Specialitate', @Suport;

-- Subspecialitați laborator
EXEC InsertDepartament N'Hematologie de laborator', N'Subspecialitate', @Lab;
EXEC InsertDepartament N'Microbiologie', N'Subspecialitate', @Lab;
EXEC InsertDepartament N'Biochimie clinica', N'Subspecialitate', @Lab;


-- CATEGORIE: Imagistica
DECLARE @Imagistica UNIQUEIDENTIFIER, @Radiologie UNIQUEIDENTIFIER;

EXEC InsertDepartament N'Imagistica', N'Categorie', NULL;
SET @Imagistica = (SELECT DepartamentID FROM Departamente WHERE Nume = N'Imagistica');

EXEC InsertDepartament N'Radiologie', N'Specialitate', @Imagistica;
SET @Radiologie = (SELECT DepartamentID FROM Departamente WHERE Nume = N'Radiologie');

-- Subspecialitați
EXEC InsertDepartament N'Radiologie intervenționala', N'Subspecialitate', @Radiologie;
EXEC InsertDepartament N'Neuroradiologie', N'Subspecialitate', @Radiologie;
