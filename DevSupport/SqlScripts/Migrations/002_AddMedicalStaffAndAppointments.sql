-- Scripturi pentru migrarea bazei de date c?tre noua structur? departamente
-- Data: 2025-01-04
-- Versiune: 002

-- Tabele pentru departamente ?i personal medical
CREATE TABLE Departamente (
    DepartamentID UNIQUEIDENTIFIER NOT NULL 
        DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    Nume NVARCHAR(200) NOT NULL,
    Tip NVARCHAR(50) NOT NULL,   -- ex: Categorie, Specialitate, Subspecialitate
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE DepartamenteIerarhie (
    AncestorID UNIQUEIDENTIFIER NOT NULL,
    DescendantID UNIQUEIDENTIFIER NOT NULL,
    Nivel INT NOT NULL,   -- 0 = self, 1 = copil direct, 2+ = descendent la distan?a

    CONSTRAINT PK_DepartamenteIerarhie PRIMARY KEY (AncestorID, DescendantID),
    CONSTRAINT FK_DepartamenteIerarhie_Ancestor FOREIGN KEY (AncestorID) REFERENCES Departamente(DepartamentID),
    CONSTRAINT FK_DepartamenteIerarhie_Descendant FOREIGN KEY (DescendantID) REFERENCES Departamente(DepartamentID)
);

-- Tabela pentru personalul medical
CREATE TABLE PersonalMedical (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PersoanaId INT NOT NULL, -- FK c?tre tabela Persoana
    DepartamentId UNIQUEIDENTIFIER NOT NULL, -- FK c?tre Departamente
    Functie NVARCHAR(100) NOT NULL,
    NumarLicenta NVARCHAR(50),
    DataAngajare DATE NOT NULL,
    DataPlecare DATE NULL,
    Activ BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_PersonalMedical_Persoana FOREIGN KEY (PersoanaId) REFERENCES Persoana(Id),
    CONSTRAINT FK_PersonalMedical_Departament FOREIGN KEY (DepartamentId) REFERENCES Departamente(DepartamentID)
);

-- Tabela pentru programari
CREATE TABLE Programari (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PacientId UNIQUEIDENTIFIER NOT NULL,
    MedicId UNIQUEIDENTIFIER NOT NULL,
    DepartamentId UNIQUEIDENTIFIER NOT NULL,
    DataProgramare DATETIME2 NOT NULL,
    Durata INT NOT NULL DEFAULT 30, -- minute
    Status NVARCHAR(50) NOT NULL DEFAULT 'Programata',
    Observatii NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Programari_Medic FOREIGN KEY (MedicId) REFERENCES PersonalMedical(Id),
    CONSTRAINT FK_Programari_Departament FOREIGN KEY (DepartamentId) REFERENCES Departamente(DepartamentID)
);

-- Index-uri pentru performan??
CREATE NONCLUSTERED INDEX IX_PersonalMedical_Departament ON PersonalMedical (DepartamentId);
CREATE NONCLUSTERED INDEX IX_PersonalMedical_Persoana ON PersonalMedical (PersoanaId);
CREATE NONCLUSTERED INDEX IX_Programari_DataProgramare ON Programari (DataProgramare);
CREATE NONCLUSTERED INDEX IX_Programari_Medic ON Programari (MedicId);
CREATE NONCLUSTERED INDEX IX_Programari_Status ON Programari (Status);