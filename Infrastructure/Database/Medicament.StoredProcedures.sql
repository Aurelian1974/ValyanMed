-- =============================================
-- Medicament Stored Procedures (CRUD)
-- Schema: dbo
-- Requires table: Medicament (as specified)
-- =============================================

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- =============================================
-- Drop if exists
-- =============================================
IF OBJECT_ID('dbo.sp_Medicament_GetAll', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Medicament_GetAll;
GO
IF OBJECT_ID('dbo.sp_Medicament_GetById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Medicament_GetById;
GO
IF OBJECT_ID('dbo.sp_Medicament_Create', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Medicament_Create;
GO
IF OBJECT_ID('dbo.sp_Medicament_Update', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Medicament_Update;
GO
IF OBJECT_ID('dbo.sp_Medicament_Delete', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Medicament_Delete;
GO

-- =============================================
-- Get All
-- =============================================
CREATE PROCEDURE dbo.sp_Medicament_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        MedicamentID,
        MedicamentGUID,
        Nume,
        DenumireComunaInternationala,
        Concentratie,
        FormaFarmaceutica,
        Producator,
        CodATC,
        Status,
        DataInregistrare,
        NumarAutorizatie,
        DataAutorizatie,
        DataExpirare,
        Ambalaj,
        Prospect,
        Contraindicatii,
        Interactiuni,
        Pret,
        PretProducator,
        TVA,
        Compensat,
        PrescriptieMedicala,
        Stoc,
        StocSiguranta,
        DataActualizare,
        UtilizatorActualizare,
        Observatii,
        Activ
    FROM dbo.Medicament
    ORDER BY Nume;
END
GO

-- =============================================
-- Get By Id
-- =============================================
CREATE PROCEDURE dbo.sp_Medicament_GetById
    @MedicamentID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        MedicamentID,
        MedicamentGUID,
        Nume,
        DenumireComunaInternationala,
        Concentratie,
        FormaFarmaceutica,
        Producator,
        CodATC,
        Status,
        DataInregistrare,
        NumarAutorizatie,
        DataAutorizatie,
        DataExpirare,
        Ambalaj,
        Prospect,
        Contraindicatii,
        Interactiuni,
        Pret,
        PretProducator,
        TVA,
        Compensat,
        PrescriptieMedicala,
        Stoc,
        StocSiguranta,
        DataActualizare,
        UtilizatorActualizare,
        Observatii,
        Activ
    FROM dbo.Medicament
    WHERE MedicamentID = @MedicamentID;
END
GO

-- =============================================
-- Create
-- Returns: new identity (int)
-- =============================================
CREATE PROCEDURE dbo.sp_Medicament_Create
    @Nume NVARCHAR(200),
    @DenumireComunaInternationala NVARCHAR(200),
    @Concentratie NVARCHAR(100),
    @FormaFarmaceutica NVARCHAR(100),
    @Producator NVARCHAR(200),
    @CodATC VARCHAR(20),
    @Status VARCHAR(50),
    @NumarAutorizatie VARCHAR(50) = NULL,
    @DataAutorizatie DATE = NULL,
    @DataExpirare DATE,
    @Ambalaj NVARCHAR(200) = NULL,
    @Prospect NTEXT = NULL,
    @Contraindicatii NTEXT = NULL,
    @Interactiuni NTEXT = NULL,
    @Pret DECIMAL(10,2) = NULL,
    @PretProducator DECIMAL(10,2) = NULL,
    @TVA DECIMAL(5,2) = NULL,
    @Compensat BIT = NULL,
    @PrescriptieMedicala BIT = NULL,
    @Stoc INT = 0,
    @StocSiguranta INT = 0,
    @UtilizatorActualizare NVARCHAR(100) = NULL,
    @Observatii NTEXT = NULL,
    @Activ BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Medicament
    (
        Nume,
        DenumireComunaInternationala,
        Concentratie,
        FormaFarmaceutica,
        Producator,
        CodATC,
        Status,
        NumarAutorizatie,
        DataAutorizatie,
        DataExpirare,
        Ambalaj,
        Prospect,
        Contraindicatii,
        Interactiuni,
        Pret,
        PretProducator,
        TVA,
        Compensat,
        PrescriptieMedicala,
        Stoc,
        StocSiguranta,
        UtilizatorActualizare,
        Observatii,
        Activ
    )
    VALUES
    (
        @Nume,
        @DenumireComunaInternationala,
        @Concentratie,
        @FormaFarmaceutica,
        @Producator,
        @CodATC,
        @Status,
        @NumarAutorizatie,
        @DataAutorizatie,
        @DataExpirare,
        @Ambalaj,
        @Prospect,
        @Contraindicatii,
        @Interactiuni,
        @Pret,
        @PretProducator,
        @TVA,
        @Compensat,
        @PrescriptieMedicala,
        @Stoc,
        @StocSiguranta,
        @UtilizatorActualizare,
        @Observatii,
        @Activ
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS MedicamentID;
END
GO

-- =============================================
-- Update
-- =============================================
CREATE PROCEDURE dbo.sp_Medicament_Update
    @MedicamentID INT,
    @Nume NVARCHAR(200),
    @DenumireComunaInternationala NVARCHAR(200),
    @Concentratie NVARCHAR(100),
    @FormaFarmaceutica NVARCHAR(100),
    @Producator NVARCHAR(200),
    @CodATC VARCHAR(20),
    @Status VARCHAR(50),
    @NumarAutorizatie VARCHAR(50) = NULL,
    @DataAutorizatie DATE = NULL,
    @DataExpirare DATE,
    @Ambalaj NVARCHAR(200) = NULL,
    @Prospect NTEXT = NULL,
    @Contraindicatii NTEXT = NULL,
    @Interactiuni NTEXT = NULL,
    @Pret DECIMAL(10,2) = NULL,
    @PretProducator DECIMAL(10,2) = NULL,
    @TVA DECIMAL(5,2) = NULL,
    @Compensat BIT = NULL,
    @PrescriptieMedicala BIT = NULL,
    @Stoc INT = 0,
    @StocSiguranta INT = 0,
    @UtilizatorActualizare NVARCHAR(100) = NULL,
    @Observatii NTEXT = NULL,
    @Activ BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Medicament
    SET 
        Nume = @Nume,
        DenumireComunaInternationala = @DenumireComunaInternationala,
        Concentratie = @Concentratie,
        FormaFarmaceutica = @FormaFarmaceutica,
        Producator = @Producator,
        CodATC = @CodATC,
        Status = @Status,
        NumarAutorizatie = @NumarAutorizatie,
        DataAutorizatie = @DataAutorizatie,
        DataExpirare = @DataExpirare,
        Ambalaj = @Ambalaj,
        Prospect = @Prospect,
        Contraindicatii = @Contraindicatii,
        Interactiuni = @Interactiuni,
        Pret = @Pret,
        PretProducator = @PretProducator,
        TVA = @TVA,
        Compensat = @Compensat,
        PrescriptieMedicala = @PrescriptieMedicala,
        Stoc = @Stoc,
        StocSiguranta = @StocSiguranta,
        DataActualizare = SYSDATETIME(),
        UtilizatorActualizare = @UtilizatorActualizare,
        Observatii = @Observatii,
        Activ = @Activ
    WHERE MedicamentID = @MedicamentID;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- =============================================
-- Delete
-- =============================================
CREATE PROCEDURE dbo.sp_Medicament_Delete
    @MedicamentID INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Medicament
    WHERE MedicamentID = @MedicamentID;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO
