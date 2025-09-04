-- Script complet pentru ini?ializarea bazei de date ValyanMed
-- Generat din script-ul original de pe 8/22/2025
-- Organizat ?i structurat pentru proiectul ValyanMed

USE [ValyanMed]
GO

-- ==============================================
-- CREARE TABELE PRINCIPALE
-- ==============================================

-- Tabela pentru dispozitive medicale
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DispozitiveMedicale]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DispozitiveMedicale](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Guid] [uniqueidentifier] NULL DEFAULT (newsequentialid()),
        [Denumire] [nvarchar](255) NOT NULL,
        [Categorie] [nvarchar](100) NULL,
        [ClasaRisc] [nvarchar](10) NULL,
        [Producator] [nvarchar](255) NULL,
        [ModelTip] [nvarchar](100) NULL,
        [NumarSerie] [nvarchar](100) NULL,
        [CertificareCE] [bit] NULL DEFAULT 0,
        [DataExpirare] [date] NULL,
        [Specificatii] [nvarchar](max) NULL,
        [DataCreare] [datetime2](7) NULL DEFAULT getdate(),
        [DataModificare] [datetime2](7) NULL DEFAULT getdate(),
        PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END

-- Tabela pentru jude?e
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Judet]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Judet](
        [IdJudet] [int] IDENTITY(1,1) NOT NULL,
        [JudetGuid] [uniqueidentifier] NOT NULL DEFAULT (newsequentialid()),
        [CodJudet] [nvarchar](10) NOT NULL,
        [Nume] [nvarchar](100) NOT NULL,
        [Siruta] [int] NULL,
        [CodAuto] [nvarchar](5) NULL,
        [Ordine] [int] NULL,
        PRIMARY KEY CLUSTERED ([IdJudet] ASC)
    )
END

-- Tabela pentru localit??i
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Localitate]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Localitate](
        [IdOras] [int] IDENTITY(1,1) NOT NULL,
        [LocalitateGuid] [uniqueidentifier] NOT NULL DEFAULT (newsequentialid()),
        [IdJudet] [int] NOT NULL,
        [Nume] [nvarchar](100) NOT NULL,
        [Siruta] [int] NOT NULL,
        [IdTipLocalitate] [int] NULL,
        [CodLocalitate] [varchar](10) NOT NULL,
        PRIMARY KEY CLUSTERED ([IdOras] ASC),
        CONSTRAINT [UQ_Oras_Nume_Judet] UNIQUE NONCLUSTERED ([IdJudet] ASC, [Nume] ASC)
    )
END

-- Tabela pentru materiale sanitare
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MaterialeSanitare]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MaterialeSanitare](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Guid] [uniqueidentifier] NULL DEFAULT (newsequentialid()),
        [Denumire] [nvarchar](255) NOT NULL,
        [Categorie] [nvarchar](100) NULL,
        [Specificatii] [nvarchar](max) NULL,
        [UnitateaMasura] [nvarchar](50) NULL,
        [Sterile] [bit] NULL DEFAULT 0,
        [UniUzinta] [bit] NULL DEFAULT 1,
        [DataCreare] [datetime2](7) NULL DEFAULT getdate(),
        [DataModificare] [datetime2](7) NULL DEFAULT getdate(),
        PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END

-- Tabela pentru medicamente
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Medicament]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Medicament](
        [MedicamentID] [int] IDENTITY(1,1) NOT NULL,
        [MedicamentGUID] [uniqueidentifier] NOT NULL DEFAULT (newsequentialid()),
        [Nume] [nvarchar](200) NOT NULL,
        [DenumireComunaInternationala] [nvarchar](200) NOT NULL,
        [Concentratie] [nvarchar](100) NOT NULL,
        [FormaFarmaceutica] [nvarchar](100) NOT NULL,
        [Producator] [nvarchar](200) NOT NULL,
        [CodATC] [varchar](20) NOT NULL,
        [Status] [varchar](50) NOT NULL DEFAULT 'Activ',
        [DataInregistrare] [datetime2](7) NOT NULL DEFAULT getdate(),
        [NumarAutorizatie] [varchar](50) NULL,
        [DataAutorizatie] [date] NULL,
        [DataExpirare] [date] NOT NULL,
        [Ambalaj] [nvarchar](200) NULL,
        [Prospect] [ntext] NULL,
        [Contraindicatii] [ntext] NULL,
        [Interactiuni] [ntext] NULL,
        [Pret] [decimal](10, 2) NULL,
        [PretProducator] [decimal](10, 2) NULL,
        [TVA] [decimal](5, 2) NULL DEFAULT 19.00,
        [Compensat] [bit] NULL DEFAULT 0,
        [PrescriptieMedicala] [bit] NULL DEFAULT 0,
        [Stoc] [int] NOT NULL DEFAULT 0,
        [StocSiguranta] [int] NOT NULL DEFAULT 0,
        [DataActualizare] [datetime2](7) NULL DEFAULT getdate(),
        [UtilizatorActualizare] [nvarchar](100) NULL,
        [Observatii] [ntext] NULL,
        [Activ] [bit] NOT NULL DEFAULT 1,
        PRIMARY KEY CLUSTERED ([MedicamentID] ASC),
        CONSTRAINT [CHK_Medicament_Status] CHECK ([Status] IN ('Activ', 'Inactiv', 'Suspendat'))
    )
END

-- Tabela pentru parteneri
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Partener]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Partener](
        [PartenerId] [int] IDENTITY(1,1) NOT NULL,
        [PartenerGuid] [uniqueidentifier] NOT NULL DEFAULT (newsequentialid()),
        [CodIntern] [nvarchar](50) NOT NULL,
        [Denumire] [nvarchar](200) NOT NULL,
        [CodFiscal] [nvarchar](50) NULL,
        [Judet] [nvarchar](100) NULL,
        [Localitate] [nvarchar](100) NULL,
        [Adresa] [nvarchar](500) NULL,
        [DataCreare] [datetime2](7) NOT NULL DEFAULT getdate(),
        [DataActualizare] [datetime2](7) NOT NULL DEFAULT getdate(),
        [UtilizatorCreare] [nvarchar](100) NULL,
        [UtilizatorActualizare] [nvarchar](100) NULL,
        [Activ] [bit] NOT NULL DEFAULT 1,
        CONSTRAINT [PK_Partener] PRIMARY KEY CLUSTERED ([PartenerId] ASC),
        CONSTRAINT [UQ_Partener_CodIntern] UNIQUE NONCLUSTERED ([CodIntern] ASC),
        CONSTRAINT [UQ_Partener_Guid] UNIQUE NONCLUSTERED ([PartenerGuid] ASC)
    )
END

-- Tabela pentru persoane
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Persoana]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Persoana](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Guid] [uniqueidentifier] NOT NULL DEFAULT (newsequentialid()),
        [Nume] [nvarchar](100) NOT NULL,
        [Prenume] [nvarchar](100) NOT NULL,
        [Judet] [nvarchar](100) NULL,
        [Localitate] [nvarchar](100) NULL,
        [Strada] [nvarchar](150) NULL,
        [NumarStrada] [nvarchar](50) NULL,
        [CodPostal] [nvarchar](20) NULL,
        [PozitieOrganizatie] [nvarchar](100) NULL,
        [DataNasterii] [date] NULL,
        [DataCreare] [datetime] NULL DEFAULT getdate(),
        [DataModificare] [datetime] NULL,
        [CNP] [varchar](13) NULL,
        [TipActIdentitate] [varchar](5) NULL,
        [SerieActIdentitate] [varchar](2) NULL,
        [NumarActIdentitate] [varchar](6) NULL,
        [StareCivila] [varchar](50) NULL,
        [Gen] [varchar](50) NULL,
        PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END

-- Tabela pentru tipuri de localit??i
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TipLocalitate]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TipLocalitate](
        [IdTipLocalitate] [int] IDENTITY(1,1) NOT NULL,
        [CodTipLocalitate] [varchar](10) NULL,
        [DenumireTipLocalitate] [varchar](100) NULL,
        CONSTRAINT [PK_TipLocalitate] PRIMARY KEY CLUSTERED ([IdTipLocalitate] ASC)
    )
END

-- Tabela pentru utilizatori
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Utilizator]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Utilizator](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Guid] [uniqueidentifier] NOT NULL DEFAULT (newsequentialid()),
        [PersoanaId] [int] NOT NULL,
        [NumeUtilizator] [nvarchar](100) NOT NULL,
        [ParolaHash] [nvarchar](512) NOT NULL,
        [Email] [nvarchar](150) NULL,
        [Telefon] [nvarchar](50) NULL,
        [DataCreare] [datetime] NULL DEFAULT getdate(),
        [DataModificare] [datetime] NULL,
        PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END

-- ==============================================
-- CREARE CONSTRAINT-URI ?I FOREIGN KEYS
-- ==============================================

-- FK pentru Localitate -> Judet
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Localitate_Judet]'))
BEGIN
    ALTER TABLE [dbo].[Localitate] ADD CONSTRAINT [FK_Localitate_Judet] 
    FOREIGN KEY([IdJudet]) REFERENCES [dbo].[Judet] ([IdJudet])
END

-- FK pentru Localitate -> TipLocalitate
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Localitate_TipLocalitate]'))
BEGIN
    ALTER TABLE [dbo].[Localitate] ADD CONSTRAINT [FK_Localitate_TipLocalitate] 
    FOREIGN KEY([IdTipLocalitate]) REFERENCES [dbo].[TipLocalitate] ([IdTipLocalitate])
END

-- FK pentru Utilizator -> Persoana
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Utilizator_Persoana]'))
BEGIN
    ALTER TABLE [dbo].[Utilizator] ADD CONSTRAINT [FK_Utilizator_Persoana] 
    FOREIGN KEY([PersoanaId]) REFERENCES [dbo].[Persoana] ([Id])
END

-- ==============================================
-- CREARE INDEX-URI PENTRU PERFORMAN??
-- ==============================================

-- Index pentru c?utare rapid? dup? email în tabela Users
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = N'IX_Users_Email')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_Email ON [dbo].[Users] ([Email])
END

-- Index pentru filtrare dup? rol
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = N'IX_Users_Role')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_Role ON [dbo].[Users] ([Role])
END

-- Index pentru c?utare dup? CNP în Patients
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Patients]') AND name = N'IX_Patients_CNP')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Patients_CNP ON [dbo].[Patients] ([CNP])
END

-- Index pentru c?utare dup? nume în Patients
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Patients]') AND name = N'IX_Patients_Name')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Patients_Name ON [dbo].[Patients] ([LastName], [FirstName])
END

PRINT 'Structura tabelelor a fost creat? cu succes!'
PRINT 'Script-ul se va completa cu datele din celelalte fi?iere.'