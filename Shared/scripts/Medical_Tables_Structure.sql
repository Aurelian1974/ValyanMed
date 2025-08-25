-- ===================================================================
-- CREAREA TABELELOR PENTRU SISTEMUL MEDICAL DE MANAGEMENT PACIENTI
-- Bazat pe sqlTableStructure.txt cu îmbunătățiri conform planului de refactorizare
-- ===================================================================

USE [ValyanMed]
GO

-- ===== TABELE PRINCIPALE =====

-- Pacienți (renumit din Patients pentru consistență)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Pacienti' AND xtype='U')
BEGIN
    CREATE TABLE Pacienti (
        PacientID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
        Nume NVARCHAR(100) NOT NULL,
        Prenume NVARCHAR(100) NOT NULL,
        CNP NVARCHAR(13) UNIQUE,
        DataNasterii DATE NOT NULL,
        Gen NVARCHAR(20) CHECK (Gen IN ('Masculin', 'Feminin', 'Neprecizat')),
        Telefon NVARCHAR(20),
        Email NVARCHAR(100),
        Adresa NVARCHAR(500),
        Oras NVARCHAR(100),
        Judet NVARCHAR(100),
        CodPostal NVARCHAR(10),
        NumeContactUrgenta NVARCHAR(200),
        TelefonContactUrgenta NVARCHAR(20),
        FurnizorAsigurare NVARCHAR(100),
        NumarAsigurare NVARCHAR(50),
        DataCreare DATETIME2 DEFAULT GETDATE(),
        DataUltimeiModificari DATETIME2 DEFAULT GETDATE(),
        EsteActiv BIT DEFAULT 1
    );
    
    PRINT 'Tabela Pacienti creată cu succes.';
END
ELSE
    PRINT 'Tabela Pacienti există deja.';

-- Personal medical
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PersonalMedical' AND xtype='U')
BEGIN
    CREATE TABLE PersonalMedical (
        PersonalID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
        Nume NVARCHAR(100) NOT NULL,
        Prenume NVARCHAR(100) NOT NULL,
        Specializare NVARCHAR(100),
        NumarLicenta NVARCHAR(50) UNIQUE,
        Telefon NVARCHAR(20),
        Email NVARCHAR(100),
        Departament NVARCHAR(100),
        Pozitie NVARCHAR(50), -- Doctor, Asistent, Tehnician, etc.
        EsteActiv BIT DEFAULT 1,
        DataCreare DATETIME2 DEFAULT GETDATE()
    );
    
    PRINT 'Tabela PersonalMedical creată cu succes.';
END
ELSE
    PRINT 'Tabela PersonalMedical există deja.';

-- ===== PROGRAMĂRI ȘI ÎNREGISTRARE =====

-- Programări
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Programari' AND xtype='U')
BEGIN
    CREATE TABLE Programari (
        ProgramareID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
        PacientID UNIQUEIDENTIFIER NOT NULL,
        DoctorID UNIQUEIDENTIFIER NOT NULL,
        DataProgramare DATETIME2 NOT NULL,
        TipProgramare NVARCHAR(100), -- Consultatie, Control, Procedura
        Status NVARCHAR(50) DEFAULT 'Programata', -- Programata, Finalizata, Anulata, Nu s-a prezentat
        Observatii NVARCHAR(1000),
        DataCreare DATETIME2 DEFAULT GETDATE(),
        DataUltimeiModificari DATETIME2 DEFAULT GETDATE(),
        CreatDe UNIQUEIDENTIFIER,
        FOREIGN KEY (PacientID) REFERENCES Pacienti(PacientID),
        FOREIGN KEY (DoctorID) REFERENCES PersonalMedical(PersonalID),
        FOREIGN KEY (CreatDe) REFERENCES PersonalMedical(PersonalID)
    );
    
    PRINT 'Tabela Programari creată cu succes.';
END
ELSE
BEGIN
    -- Verifică și adaugă coloana DataUltimeiModificari dacă nu există
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Programari') AND name = 'DataUltimeiModificari')
    BEGIN
        ALTER TABLE Programari ADD DataUltimeiModificari DATETIME2 DEFAULT GETDATE();
        PRINT 'Coloana DataUltimeiModificari adăugată în Programari.';
    END
    ELSE
        PRINT 'Tabela Programari există deja.';
END

-- Formulare de consimțământ
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FormulareConsimtamant' AND xtype='U')
BEGIN
    CREATE TABLE FormulareConsimtamant (
        ConsimtamantID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
        PacientID UNIQUEIDENTIFIER NOT NULL,
        TipFormular NVARCHAR(100) NOT NULL, -- Tratament, Procesare Date, Fotografie, etc.
        ConsimtamantAcordat BIT NOT NULL,
        DataConsimtamant DATETIME2 NOT NULL,
        MartorID UNIQUEIDENTIFIER,
        CaleDocument NVARCHAR(500),
        FOREIGN KEY (PacientID) REFERENCES Pacienti(PacientID),
        FOREIGN KEY (MartorID) REFERENCES PersonalMedical(PersonalID)
    );
    
    PRINT 'Tabela FormulareConsimtamant creată cu succes.';
END
ELSE
    PRINT 'Tabela FormulareConsimtamant există deja.';

-- ===== TRIAJ ȘI EVALUARE =====

-- Triaj pacienți
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TriajPacienti' AND xtype='U')
BEGIN
    CREATE TABLE TriajPacienti (
        TriajID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
        ProgramareID UNIQUEIDENTIFIER NOT NULL,
        NivelTriaj INT CHECK (NivelTriaj BETWEEN 1 AND 5), -- 1=Critic, 5=Neurgent
        PlangereaPrincipala NVARCHAR(1000),
        AsistentTriajID UNIQUEIDENTIFIER,
        DataTriaj DATETIME2 DEFAULT GETDATE(),
        Observatii NVARCHAR(1000),
        FOREIGN KEY (ProgramareID) REFERENCES Programari(ProgramareID),
        FOREIGN KEY (AsistentTriajID) REFERENCES PersonalMedical(PersonalID)
    );
    
    PRINT 'Tabela TriajPacienti creată cu succes.';
END
ELSE
    PRINT 'Tabela TriajPacienti există deja.';

-- Semne vitale
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SemneVitale' AND xtype='U')
BEGIN
    CREATE TABLE SemneVitale (
        SemneVitaleID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
        PacientID UNIQUEIDENTIFIER NOT NULL,
        TensiuneArterialaMax INT,
        TensiuneArterialaMin INT,
        FrecariaCardiaca INT,
        Temperatura DECIMAL(4,1),
        Greutate DECIMAL(5,2),
        Inaltime INT, -- în cm
        FrecariaRespiratorie INT,
        SaturatieOxigen DECIMAL(5,2),
        MasuratDe UNIQUEIDENTIFIER,
        DataMasurare DATETIME2 DEFAULT GETDATE(),
        FOREIGN KEY (PacientID) REFERENCES Pacienti(PacientID),
        FOREIGN KEY (MasuratDe) REFERENCES PersonalMedical(PersonalID)
    );
    
    PRINT 'Tabela SemneVitale creată cu succes.';
END
ELSE
BEGIN
    -- Verifică și adaugă coloana PacientID dacă nu există
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SemneVitale') AND name = 'PacientID')
    BEGIN
        ALTER TABLE SemneVitale ADD PacientID UNIQUEIDENTIFIER NOT NULL;
        ALTER TABLE SemneVitale ADD FOREIGN KEY (PacientID) REFERENCES Pacienti(PacientID);
        PRINT 'Coloana PacientID adăugată în SemneVitale.';
    END
    ELSE
        PRINT 'Tabela SemneVitale există deja.';
END

-- ===== CONSULTAȚII MEDICALE =====

-- Istoric medical
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='IstoricMedical' AND xtype='U')
BEGIN
    CREATE TABLE IstoricMedical (
        IstoricID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
        PacientID UNIQUEIDENTIFIER NOT NULL,
        Afectiune NVARCHAR(200) NOT NULL,
        DataDiagnostic DATE,
        Status NVARCHAR(50), -- Activ, Rezolvat, Cronic
        Severitate NVARCHAR(50), -- Usoara, Moderata, Severa
        Observatii NVARCHAR(1000),
        InregistratDe UNIQUEIDENTIFIER,
        DataInregistrare DATETIME2 DEFAULT GETDATE(),
        FOREIGN KEY (PacientID) REFERENCES Pacienti(PacientID),
        FOREIGN KEY (InregistratDe) REFERENCES PersonalMedical(PersonalID)
    );
    
    PRINT 'Tabela IstoricMedical creată cu succes.';
END
ELSE
    PRINT 'Tabela IstoricMedical există deja.';

-- Consultații
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Consultatii' AND xtype='U')
BEGIN
    CREATE TABLE Consultatii (
        ConsultatieID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
        ProgramareID UNIQUEIDENTIFIER NOT NULL,
        PlangereaPrincipala NVARCHAR(1000),
        IstoricBoalaActuala NVARCHAR(2000),
        ExamenFizic NVARCHAR(2000),
        Evaluare NVARCHAR(1000),
        [Plan] NVARCHAR(1000),
        DataConsultatie DATETIME2 DEFAULT GETDATE(),
        Durata INT, -- în minute
        FOREIGN KEY (ProgramareID) REFERENCES Programari(ProgramareID)
    );
    
    PRINT 'Tabela Consultatii creată cu succes.';
END
ELSE
    PRINT 'Tabela Consultatii există deja.';

-- ===== INVESTIGAȚII ȘI ANALIZE =====

-- Tipuri de teste
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TipuriTeste' AND xtype='U')
BEGIN
    CREATE TABLE TipuriTeste (
        TipTestID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
        NumeTest NVARCHAR(200) NOT NULL,
        Categorie NVARCHAR(100), -- Laborator, Imagistica, Functional
        Departament NVARCHAR(100),
        IntervalNormal NVARCHAR(200),
        UnitateaMasura NVARCHAR(50),
        EsteActiv BIT DEFAULT 1
    );
    
    PRINT 'Tabela TipuriTeste creată cu succes.';
END
ELSE
    PRINT 'Tabela TipuriTeste există deja.';

-- Comenzi teste
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ComenziTeste' AND xtype='U')
BEGIN
    CREATE TABLE ComenziTeste (
        ComandaID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
        ConsultatieID UNIQUEIDENTIFIER NOT NULL,
        TipTestID UNIQUEIDENTIFIER NOT NULL,
        DataComanda DATETIME2 DEFAULT GETDATE(),
        Status NVARCHAR(50) DEFAULT 'Comandat', -- Comandat, In progres, Finalizat, Anulat
        Prioritate NVARCHAR(50) DEFAULT 'Rutina', -- STAT, Urgent, Rutina
        ComantatDe UNIQUEIDENTIFIER NOT NULL,
        Observatii NVARCHAR(500),
        FOREIGN KEY (ConsultatieID) REFERENCES Consultatii(ConsultatieID),
        FOREIGN KEY (TipTestID) REFERENCES TipuriTeste(TipTestID),
        FOREIGN KEY (ComantatDe) REFERENCES PersonalMedical(PersonalID)
    );
    
    PRINT 'Tabela ComenziTeste creată cu succes.';
END
ELSE
    PRINT 'Tabela ComenziTeste există deja.';

-- Rezultate teste
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RezultateTeste' AND xtype='U')
BEGIN
    CREATE TABLE RezultateTeste (
        RezultatID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
        ComandaID UNIQUEIDENTIFIER NOT NULL,
        Rezultat NVARCHAR(500),
        ValoareNumerica DECIMAL(10,3),
        IntervalReferinta NVARCHAR(100),
        Status NVARCHAR(50), -- Normal, Anormal, Critic
        DataRezultat DATETIME2,
        EfectuatDe UNIQUEIDENTIFIER,
        RevizuitDe UNIQUEIDENTIFIER,
        DataRevizuire DATETIME2,
        Observatii NVARCHAR(1000),
        FOREIGN KEY (ComandaID) REFERENCES ComenziTeste(ComandaID),
        FOREIGN KEY (EfectuatDe) REFERENCES PersonalMedical(PersonalID),
        FOREIGN KEY (RevizuitDe) REFERENCES PersonalMedical(PersonalID)
    );
    
    PRINT 'Tabela RezultateTeste creată cu succes.';
END
ELSE
    PRINT 'Tabela RezultateTeste există deja.';

-- ===== DIAGNOSTIC ȘI TRATAMENT =====

-- Diagnostice
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Diagnostice' AND xtype='U')
BEGIN
    CREATE TABLE Diagnostice (
        DiagnosticID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
        ConsultatieID UNIQUEIDENTIFIER NOT NULL,
        CodICD NVARCHAR(10),
        DescriereaDiagnosticului NVARCHAR(500) NOT NULL,
        TipDiagnostic NVARCHAR(50), -- Principal, Secundar, Differential
        Severitate NVARCHAR(50),
        Status NVARCHAR(50), -- Activ, Rezolvat, Cronic
        DataDiagnostic DATETIME2 DEFAULT GETDATE(),
        FOREIGN KEY (ConsultatieID) REFERENCES Consultatii(ConsultatieID)
    );
    
    PRINT 'Tabela Diagnostice creată cu succes.';
END
ELSE
    PRINT 'Tabela Diagnostice există deja.';

-- Medicamente (îmbunătățit față de tabelul existent)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MedicamenteNoi' AND xtype='U')
BEGIN
    CREATE TABLE MedicamenteNoi (
        MedicamentID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
        NumeMedicament NVARCHAR(200) NOT NULL,
        NumeGeneric NVARCHAR(200),
        Concentratie NVARCHAR(50),
        Forma NVARCHAR(50), -- Tableta, Capsula, Sirop, etc.
        Producator NVARCHAR(100),
        EsteActiv BIT DEFAULT 1
    );
    
    PRINT 'Tabela MedicamenteNoi creată cu succes.';
END
ELSE
    PRINT 'Tabela MedicamenteNoi există deja.';

-- Prescripții
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Prescriptii' AND xtype='U')
BEGIN
    CREATE TABLE Prescriptii (
        PrescriptieID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
        ConsultatieID UNIQUEIDENTIFIER NOT NULL,
        MedicamentID UNIQUEIDENTIFIER NOT NULL,
        Doza NVARCHAR(100),
        Frecventa NVARCHAR(100),
        Durata NVARCHAR(100),
        Cantitate INT,
        Reinnoire INT DEFAULT 0,
        Instructiuni NVARCHAR(500),
        DataPrescriptie DATETIME2 DEFAULT GETDATE(),
        PrescrisDe UNIQUEIDENTIFIER NOT NULL,
        FOREIGN KEY (ConsultatieID) REFERENCES Consultatii(ConsultatieID),
        FOREIGN KEY (MedicamentID) REFERENCES MedicamenteNoi(MedicamentID),
        FOREIGN KEY (PrescrisDe) REFERENCES PersonalMedical(PersonalID)
    );
    
    PRINT 'Tabela Prescriptii creată cu succes.';
END
ELSE
    PRINT 'Tabela Prescriptii există deja.';

-- ===== TABELE PENTRU AUTENTIFICARE ȘI ROLURI =====

-- Utilizatori sistem (pentru sistemul medical)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UtilizatoriSistem' AND xtype='U')
BEGIN
    CREATE TABLE UtilizatoriSistem (
        UtilizatorID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
        NumeUtilizator NVARCHAR(50) NOT NULL UNIQUE,
        HashParola NVARCHAR(255) NOT NULL,
        Email NVARCHAR(100) NOT NULL,
        PersonalID UNIQUEIDENTIFIER,
        EsteActiv BIT DEFAULT 1,
        DataUltimeiAutentificari DATETIME2,
        IncercariEsuateAutentificare INT DEFAULT 0,
        BlocatPanaLa DATETIME2,
        DataCreare DATETIME2 DEFAULT GETDATE(),
        CreatDe UNIQUEIDENTIFIER,
        FOREIGN KEY (PersonalID) REFERENCES PersonalMedical(PersonalID)
    );
    
    PRINT 'Tabela UtilizatoriSistem creată cu succes.';
END
ELSE
    PRINT 'Tabela UtilizatoriSistem există deja.';

-- Roluri sistem
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RoluriSistem' AND xtype='U')
BEGIN
    CREATE TABLE RoluriSistem (
        RolID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
        NumeRol NVARCHAR(50) NOT NULL UNIQUE,
        Descriere NVARCHAR(200),
        EsteActiv BIT DEFAULT 1,
        DataCreare DATETIME2 DEFAULT GETDATE()
    );
    
    PRINT 'Tabela RoluriSistem creată cu succes.';
END
ELSE
    PRINT 'Tabela RoluriSistem există deja.';

-- Asocierea utilizatori-roluri
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UtilizatorRoluri' AND xtype='U')
BEGIN
    CREATE TABLE UtilizatorRoluri (
        UtilizatorRolID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
        UtilizatorID UNIQUEIDENTIFIER NOT NULL,
        RolID UNIQUEIDENTIFIER NOT NULL,
        DataAtribuire DATETIME2 DEFAULT GETDATE(),
        AtribuitDe UNIQUEIDENTIFIER,
        FOREIGN KEY (UtilizatorID) REFERENCES UtilizatoriSistem(UtilizatorID),
        FOREIGN KEY (RolID) REFERENCES RoluriSistem(RolID),
        UNIQUE(UtilizatorID, RolID)
    );
    
    PRINT 'Tabela UtilizatorRoluri creată cu succes.';
END
ELSE
    PRINT 'Tabela UtilizatorRoluri există deja.';

-- ===== INDEXURI PENTRU PERFORMANȚĂ =====

-- Indexuri pentru căutări frecvente
IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_Pacienti_CNP')
    CREATE INDEX IX_Pacienti_CNP ON Pacienti(CNP);

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_Pacienti_NumeComplet')
    CREATE INDEX IX_Pacienti_NumeComplet ON Pacienti(Nume, Prenume);

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_PersonalMedical_NumarLicenta')
    CREATE INDEX IX_PersonalMedical_NumarLicenta ON PersonalMedical(NumarLicenta);

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_Programari_Data_Doctor')
    CREATE INDEX IX_Programari_Data_Doctor ON Programari(DataProgramare, DoctorID);

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_Programari_Pacient_Status')
    CREATE INDEX IX_Programari_Pacient_Status ON Programari(PacientID, Status);

PRINT 'Toate tabelele și indexurile pentru sistemul medical au fost create cu succes!';
PRINT 'Următorul pas: Popularea cu date dummy și crearea procedurilor stocate.';