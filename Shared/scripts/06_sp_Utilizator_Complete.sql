/*
    Stored Procedures pentru gestionarea utilizatorilor
    Conform planului de refactorizare - eliminare diacritice, rich services, error handling specific
*/

-- =============================================
-- Create User
-- =============================================
IF OBJECT_ID('dbo.usp_Utilizator_Insert','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.usp_Utilizator_Insert AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE dbo.usp_Utilizator_Insert
    @NumeUtilizator NVARCHAR(100),
    @ParolaHash NVARCHAR(512),
    @Email NVARCHAR(150),
    @Telefon NVARCHAR(50) = NULL,
    @PersoanaId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Validate required fields
        IF @NumeUtilizator IS NULL OR LEN(LTRIM(RTRIM(@NumeUtilizator))) = 0
        BEGIN
            RAISERROR('Numele de utilizator este obligatoriu.', 16, 1);
            RETURN;
        END
        
        IF @Email IS NULL OR LEN(LTRIM(RTRIM(@Email))) = 0
        BEGIN
            RAISERROR('Email-ul este obligatoriu.', 16, 1);
            RETURN;
        END
        
        IF @ParolaHash IS NULL OR LEN(LTRIM(RTRIM(@ParolaHash))) = 0
        BEGIN
            RAISERROR('Parola este obligatorie.', 16, 1);
            RETURN;
        END
        
        -- Check if person exists
        IF NOT EXISTS (SELECT 1 FROM dbo.Persoana WHERE Id = @PersoanaId)
        BEGIN
            RAISERROR('Persoana selectata nu exista.', 16, 1);
            RETURN;
        END
        
        -- Check if username already exists
        IF EXISTS (SELECT 1 FROM dbo.Utilizator WHERE NumeUtilizator = @NumeUtilizator)
        BEGIN
            RAISERROR('Un utilizator cu acest nume deja exista.', 16, 1);
            RETURN;
        END
        
        -- Check if email already exists
        IF EXISTS (SELECT 1 FROM dbo.Utilizator WHERE Email = @Email)
        BEGIN
            RAISERROR('Un utilizator cu acest email deja exista.', 16, 1);
            RETURN;
        END
        
        -- Insert new user
        INSERT INTO dbo.Utilizator (
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
        
        -- Return the new ID
        SELECT SCOPE_IDENTITY() AS Id;
        
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
-- Get User by Id
-- =============================================
IF OBJECT_ID('dbo.usp_Utilizator_GetById','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.usp_Utilizator_GetById AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE dbo.usp_Utilizator_GetById
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
        u.DataModificare,
        p.Nume,
        p.Prenume
    FROM dbo.Utilizator u WITH (NOLOCK)
    LEFT JOIN dbo.Persoana p WITH (NOLOCK) ON u.PersoanaId = p.Id
    WHERE u.Id = @Id;
END
GO

-- =============================================
-- Get All Users
-- =============================================
IF OBJECT_ID('dbo.usp_Utilizator_GetAll','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.usp_Utilizator_GetAll AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE dbo.usp_Utilizator_GetAll
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
    FROM dbo.Utilizator u WITH (NOLOCK)
    LEFT JOIN dbo.Persoana p WITH (NOLOCK) ON u.PersoanaId = p.Id
    ORDER BY u.NumeUtilizator;
END
GO

-- =============================================
-- Update User
-- =============================================
IF OBJECT_ID('dbo.usp_Utilizator_Update','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.usp_Utilizator_Update AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE dbo.usp_Utilizator_Update
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
        -- Validate required fields
        IF @NumeUtilizator IS NULL OR LEN(LTRIM(RTRIM(@NumeUtilizator))) = 0
        BEGIN
            RAISERROR('Numele de utilizator este obligatoriu.', 16, 1);
            RETURN;
        END
        
        IF @Email IS NULL OR LEN(LTRIM(RTRIM(@Email))) = 0
        BEGIN
            RAISERROR('Email-ul este obligatoriu.', 16, 1);
            RETURN;
        END
        
        -- Check if user exists
        IF NOT EXISTS (SELECT 1 FROM dbo.Utilizator WHERE Id = @Id)
        BEGIN
            RAISERROR('Utilizatorul nu exista.', 16, 1);
            RETURN;
        END
        
        -- Check if person exists
        IF NOT EXISTS (SELECT 1 FROM dbo.Persoana WHERE Id = @PersoanaId)
        BEGIN
            RAISERROR('Persoana selectata nu exista.', 16, 1);
            RETURN;
        END
        
        -- Check if username already exists for another user
        IF EXISTS (SELECT 1 FROM dbo.Utilizator WHERE NumeUtilizator = @NumeUtilizator AND Id != @Id)
        BEGIN
            RAISERROR('Un alt utilizator cu acest nume deja exista.', 16, 1);
            RETURN;
        END
        
        -- Check if email already exists for another user
        IF EXISTS (SELECT 1 FROM dbo.Utilizator WHERE Email = @Email AND Id != @Id)
        BEGIN
            RAISERROR('Un alt utilizator cu acest email deja exista.', 16, 1);
            RETURN;
        END
        
        -- Update user
        UPDATE dbo.Utilizator 
        SET 
            NumeUtilizator = @NumeUtilizator,
            Email = @Email,
            Telefon = @Telefon,
            PersoanaId = @PersoanaId,
            ParolaHash = ISNULL(@ParolaHash, ParolaHash), -- Only update password if provided
            DataModificare = GETDATE()
        WHERE Id = @Id;
        
        -- Return number of affected rows
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
-- Delete User
-- =============================================
IF OBJECT_ID('dbo.usp_Utilizator_Delete','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.usp_Utilizator_Delete AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE dbo.usp_Utilizator_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Validate required fields
        IF @Id IS NULL OR @Id <= 0
        BEGIN
            RAISERROR('ID-ul utilizatorului este invalid.', 16, 1);
            RETURN;
        END
        
        -- Check if user exists
        IF NOT EXISTS (SELECT 1 FROM dbo.Utilizator WHERE Id = @Id)
        BEGIN
            RAISERROR('Utilizatorul nu exista.', 16, 1);
            RETURN;
        END
        
        -- TODO: Check for dependencies (active sessions, etc.)
        -- This check will be implemented when we have other entities connected
        
        -- Delete user
        DELETE FROM dbo.Utilizator WHERE Id = @Id;
        
        -- Return number of affected rows
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
-- Authenticate User
-- =============================================
IF OBJECT_ID('dbo.usp_Utilizator_Authenticate','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.usp_Utilizator_Authenticate AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE dbo.usp_Utilizator_Authenticate
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
    FROM dbo.Utilizator u WITH (NOLOCK)
    LEFT JOIN dbo.Persoana p WITH (NOLOCK) ON u.PersoanaId = p.Id
    WHERE u.NumeUtilizator = @NumeUtilizatorSauEmail 
       OR u.Email = @NumeUtilizatorSauEmail;
END
GO