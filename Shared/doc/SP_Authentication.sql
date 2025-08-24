-- Stored procedures pentru sistemul de autentificare refactorizat

-- =============================================
-- Utilizator Create
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'usp_Utilizator_Insert')
    DROP PROCEDURE usp_Utilizator_Insert
GO

CREATE PROCEDURE [dbo].[usp_Utilizator_Insert]
    @NumeUtilizator NVARCHAR(100),
    @ParolaHash NVARCHAR(512),
    @Email NVARCHAR(150),
    @Telefon NVARCHAR(50) = NULL,
    @PersoanaId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Verific? dac? persoana exist?
        IF NOT EXISTS (SELECT 1 FROM Persoana WHERE Id = @PersoanaId)
        BEGIN
            RAISERROR('Persoana specificata nu exista.', 16, 1);
            RETURN;
        END
        
        -- Verific? dac? numele de utilizator exist? deja
        IF EXISTS (SELECT 1 FROM Utilizator WHERE NumeUtilizator = @NumeUtilizator)
        BEGIN
            RAISERROR('Numele de utilizator exista deja.', 16, 1);
            RETURN;
        END
        
        -- Verific? dac? email-ul exist? deja
        IF EXISTS (SELECT 1 FROM Utilizator WHERE Email = @Email)
        BEGIN
            RAISERROR('Email-ul exista deja.', 16, 1);
            RETURN;
        END
        
        DECLARE @NewId INT;
        
        INSERT INTO Utilizator (
            Guid,
            PersoanaId,
            NumeUtilizator,
            ParolaHash,
            Email,
            Telefon,
            DataCreare,
            DataModificare
        )
        VALUES (
            NEWID(),
            @PersoanaId,
            @NumeUtilizator,
            @ParolaHash,
            @Email,
            @Telefon,
            GETDATE(),
            GETDATE()
        );
        
        SET @NewId = SCOPE_IDENTITY();
        SELECT @NewId AS Id;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- =============================================
-- Utilizator GetById
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'usp_Utilizator_GetById')
    DROP PROCEDURE usp_Utilizator_GetById
GO

CREATE PROCEDURE [dbo].[usp_Utilizator_GetById]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.Id,
        u.Guid,
        u.PersoanaId,
        u.NumeUtilizator,
        u.ParolaHash,
        u.Email,
        u.Telefon,
        u.DataCreare,
        u.DataModificare
    FROM Utilizator u
    WHERE u.Id = @Id;
END
GO

-- =============================================
-- Utilizator GetAll
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'usp_Utilizator_GetAll')
    DROP PROCEDURE usp_Utilizator_GetAll
GO

CREATE PROCEDURE [dbo].[usp_Utilizator_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.Id,
        u.Guid,
        u.PersoanaId,
        u.NumeUtilizator,
        u.ParolaHash,
        u.Email,
        u.Telefon,
        u.DataCreare,
        u.DataModificare,
        p.Nume,
        p.Prenume
    FROM Utilizator u
    INNER JOIN Persoana p ON u.PersoanaId = p.Id
    ORDER BY u.NumeUtilizator;
END
GO

-- =============================================
-- Utilizator Authenticate
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'usp_Utilizator_Authenticate')
    DROP PROCEDURE usp_Utilizator_Authenticate
GO

CREATE PROCEDURE [dbo].[usp_Utilizator_Authenticate]
    @NumeUtilizatorSauEmail NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.Id,
        u.Guid,
        u.PersoanaId,
        u.NumeUtilizator,
        u.ParolaHash,
        u.Email,
        u.Telefon,
        u.DataCreare,
        u.DataModificare,
        p.Nume,
        p.Prenume
    FROM Utilizator u
    INNER JOIN Persoana p ON u.PersoanaId = p.Id
    WHERE u.NumeUtilizator = @NumeUtilizatorSauEmail 
       OR u.Email = @NumeUtilizatorSauEmail;
END
GO

-- =============================================
-- Utilizator Update
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'usp_Utilizator_Update')
    DROP PROCEDURE usp_Utilizator_Update
GO

CREATE PROCEDURE [dbo].[usp_Utilizator_Update]
    @Id INT,
    @NumeUtilizator NVARCHAR(100),
    @Email NVARCHAR(150),
    @Telefon NVARCHAR(50) = NULL,
    @PersoanaId INT,
    @ParolaHash NVARCHAR(512) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Verific? dac? utilizatorul exist?
        IF NOT EXISTS (SELECT 1 FROM Utilizator WHERE Id = @Id)
        BEGIN
            RAISERROR('Utilizatorul nu exista.', 16, 1);
            RETURN;
        END
        
        -- Verific? dac? persoana exist?
        IF NOT EXISTS (SELECT 1 FROM Persoana WHERE Id = @PersoanaId)
        BEGIN
            RAISERROR('Persoana specificata nu exista.', 16, 1);
            RETURN;
        END
        
        -- Verific? duplicat nume utilizator
        IF EXISTS (SELECT 1 FROM Utilizator WHERE NumeUtilizator = @NumeUtilizator AND Id != @Id)
        BEGIN
            RAISERROR('Numele de utilizator exista deja.', 16, 1);
            RETURN;
        END
        
        -- Verific? duplicat email
        IF EXISTS (SELECT 1 FROM Utilizator WHERE Email = @Email AND Id != @Id)
        BEGIN
            RAISERROR('Email-ul exista deja.', 16, 1);
            RETURN;
        END
        
        UPDATE Utilizator 
        SET 
            PersoanaId = @PersoanaId,
            NumeUtilizator = @NumeUtilizator,
            Email = @Email,
            Telefon = @Telefon,
            ParolaHash = CASE WHEN @ParolaHash IS NOT NULL THEN @ParolaHash ELSE ParolaHash END,
            DataModificare = GETDATE()
        WHERE Id = @Id;
        
        SELECT @@ROWCOUNT AS RowsAffected;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- =============================================
-- Utilizator Delete
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'usp_Utilizator_Delete')
    DROP PROCEDURE usp_Utilizator_Delete
GO

CREATE PROCEDURE [dbo].[usp_Utilizator_Delete]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Verific? dac? utilizatorul exist?
        IF NOT EXISTS (SELECT 1 FROM Utilizator WHERE Id = @Id)
        BEGIN
            RAISERROR('Utilizatorul nu exista.', 16, 1);
            RETURN;
        END
        
        DELETE FROM Utilizator WHERE Id = @Id;
        
        SELECT @@ROWCOUNT AS RowsAffected;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- Create new person
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CreatePersoana')
    DROP PROCEDURE sp_CreatePersoana
GO

CREATE PROCEDURE [dbo].[sp_CreatePersoana]
    @Guid uniqueidentifier,
    @Nume nvarchar(100),
    @Prenume nvarchar(100),
    @Judet nvarchar(100) = NULL,
    @Localitate nvarchar(100) = NULL,
    @Strada nvarchar(150) = NULL,
    @NumarStrada nvarchar(50) = NULL,
    @CodPostal nvarchar(20) = NULL,
    @PozitieOrganizatie nvarchar(100) = NULL,
    @DataNasterii date = NULL,
    @CNP nvarchar(13) = NULL,
    @TipActIdentitate nvarchar(5) = NULL,
    @SerieActIdentitate nvarchar(2) = NULL,
    @NumarActIdentitate nvarchar(6) = NULL,
    @StareCivila nvarchar(50) = NULL,
    @Gen nvarchar(50) = NULL
AS
BEGIN
    INSERT INTO Persoana (
        Guid, Nume, Prenume, Judet, Localitate, Strada, 
        NumarStrada, CodPostal, PozitieOrganizatie, DataNasterii,
        DataCreare, DataModificare, CNP, TipActIdentitate, SerieActIdentitate,
        NumarActIdentitate, StareCivila, Gen
    ) VALUES (
        @Guid, @Nume, @Prenume, @Judet, @Localitate, @Strada,
        @NumarStrada, @CodPostal, @PozitieOrganizatie, @DataNasterii,
        GETDATE(), GETDATE(), @CNP, @TipActIdentitate, @SerieActIdentitate,
        @NumarActIdentitate, @StareCivila, @Gen
    )
    
    SELECT CAST(SCOPE_IDENTITY() as int)
END
GO

-- Update person
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_UpdatePersoana')
    DROP PROCEDURE sp_UpdatePersoana
GO

CREATE PROCEDURE [dbo].[sp_UpdatePersoana]
    @Id int,
    @Nume nvarchar(100),
    @Prenume nvarchar(100),
    @Judet nvarchar(100) = NULL,
    @Localitate nvarchar(100) = NULL,
    @Strada nvarchar(150) = NULL,
    @NumarStrada nvarchar(50) = NULL,
    @CodPostal nvarchar(20) = NULL,
    @PozitieOrganizatie nvarchar(100) = NULL,
    @DataNasterii date = NULL,
    @CNP nvarchar(13) = NULL,
    @TipActIdentitate nvarchar(5) = NULL,
    @SerieActIdentitate nvarchar(2) = NULL,
    @NumarActIdentitate nvarchar(6) = NULL,
    @StareCivila nvarchar(50) = NULL,
    @Gen nvarchar(50) = NULL
AS
BEGIN
    UPDATE Persoana 
    SET 
        Nume = @Nume,
        Prenume = @Prenume,
        Judet = @Judet,
        Localitate = @Localitate,
        Strada = @Strada,
        NumarStrada = @NumarStrada,
        CodPostal = @CodPostal,
        PozitieOrganizatie = @PozitieOrganizatie,
        DataNasterii = @DataNasterii,
        DataModificare = GETDATE(),
        CNP = @CNP,
        TipActIdentitate = @TipActIdentitate,
        SerieActIdentitate = @SerieActIdentitate,
        NumarActIdentitate = @NumarActIdentitate,
        StareCivila = @StareCivila,
        Gen = @Gen
    WHERE Id = @Id
END
GO