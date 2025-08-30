-- ===================================================================
-- PROCEDURI STOCATE PENTRU SISTEMUL MEDICAL DE MANAGEMENT PACIENTI
-- Conform planului de refactorizare cu Rich Services și Result Pattern
-- ===================================================================

USE [ValyanMed]
GO
-- ===== PROCEDURI PENTRU PACIENTI =====

-- Obținere toți pacienții cu paginare
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Pacienti_GetPaged]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Pacienti_GetPaged]
GO

CREATE PROCEDURE [dbo].[sp_Pacienti_GetPaged]
    @Search NVARCHAR(255) = NULL,
    @Judet NVARCHAR(100) = NULL,
    @Gen NVARCHAR(20) = NULL,
    @Page INT = 1,
    @PageSize INT = 25,
    @Sort NVARCHAR(400) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF (@Page IS NULL OR @Page < 1) SET @Page = 1;
    IF (@PageSize IS NULL OR @PageSize < 1) SET @PageSize = 25;
    IF (@PageSize > 1000) SET @PageSize = 1000;

    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    DECLARE @OrderBy NVARCHAR(MAX) = N'Nume ASC, Prenume ASC';

    -- Build ORDER BY from Sort parameter
    IF (@Sort IS NOT NULL AND LEN(@Sort) > 0)
    BEGIN
        -- Simple sort parsing (you can enhance this)
        IF (@Sort LIKE '%nume%desc%')
            SET @OrderBy = N'Nume DESC, Prenume DESC';
        ELSE IF (@Sort LIKE '%data%desc%')
            SET @OrderBy = N'DataCreare DESC';
        ELSE IF (@Sort LIKE '%data%asc%')
            SET @OrderBy = N'DataCreare ASC';
    END

    DECLARE @sql NVARCHAR(MAX) = N'
    SELECT 
        PacientID,
        Nume,
        Prenume,
        CNP,
        DataNasterii,
        Gen,
        Telefon,
        Email,
        Adresa,
        Oras,
        Judet,
        CodPostal,
        FurnizorAsigurare,
        NumarAsigurare,
        DataCreare,
        EsteActiv
    FROM Pacienti
    WHERE EsteActiv = 1
      AND (@Search IS NULL OR @Search = '''' OR 
           UPPER(Nume) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
           UPPER(Prenume) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
           UPPER(CNP) LIKE ''%'' + UPPER(@Search) + ''%'' OR
           UPPER(Email) LIKE ''%'' + UPPER(@Search) + ''%'')
      AND (@Judet IS NULL OR @Judet = '''' OR @Judet = ''toate'' OR Judet = @Judet)
      AND (@Gen IS NULL OR @Gen = '''' OR @Gen = ''toate'' OR Gen = @Gen)
    ORDER BY ' + @OrderBy + '
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(1)
    FROM Pacienti
    WHERE EsteActiv = 1
      AND (@Search IS NULL OR @Search = '''' OR 
           UPPER(Nume) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
           UPPER(Prenume) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
           UPPER(CNP) LIKE ''%'' + UPPER(@Search) + ''%'' OR
           UPPER(Email) LIKE ''%'' + UPPER(@Search) + ''%'')
      AND (@Judet IS NULL OR @Judet = '''' OR @Judet = ''toate'' OR Judet = @Judet)
      AND (@Gen IS NULL OR @Gen = '''' OR @Gen = ''toate'' OR Gen = @Gen);';

    EXEC sp_executesql
        @sql,
        N'@Search NVARCHAR(255), @Judet NVARCHAR(100), @Gen NVARCHAR(20), @Offset INT, @PageSize INT',
        @Search = @Search, @Judet = @Judet, @Gen = @Gen, @Offset = @Offset, @PageSize = @PageSize;
END
GO

-- Căutare pacient după CNP
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Pacienti_GetByCNP]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Pacienti_GetByCNP]
GO

CREATE PROCEDURE [dbo].[sp_Pacienti_GetByCNP]
    @CNP NVARCHAR(13)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        PacientID,
        Nume,
        Prenume,
        CNP,
        DataNasterii,
        Gen,
        Telefon,
        Email,
        Adresa,
        Oras,
        Judet,
        CodPostal,
        NumeContactUrgenta,
        TelefonContactUrgenta,
        FurnizorAsigurare,
        NumarAsigurare,
        DataCreare,
        DataUltimeiModificari,
        EsteActiv
    FROM Pacienti
    WHERE CNP = @CNP AND EsteActiv = 1;
END
GO

-- Crearea unui pacient nou
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Pacienti_Create]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Pacienti_Create]
GO

CREATE PROCEDURE [dbo].[sp_Pacienti_Create]
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @CNP NVARCHAR(13) = NULL,
    @DataNasterii DATE,
    @Gen NVARCHAR(20),
    @Telefon NVARCHAR(20) = NULL,
    @Email NVARCHAR(100) = NULL,
    @Adresa NVARCHAR(500) = NULL,
    @Oras NVARCHAR(100) = NULL,
    @Judet NVARCHAR(100) = NULL,
    @CodPostal NVARCHAR(10) = NULL,
    @NumeContactUrgenta NVARCHAR(200) = NULL,
    @TelefonContactUrgenta NVARCHAR(20) = NULL,
    @FurnizorAsigurare NVARCHAR(100) = NULL,
    @NumarAsigurare NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Validare CNP duplicat
        IF @CNP IS NOT NULL AND EXISTS (SELECT 1 FROM Pacienti WHERE CNP = @CNP AND EsteActiv = 1)
        BEGIN
            RAISERROR('Un pacient cu acest CNP există deja în sistem.', 16, 1);
            RETURN;
        END
        
        DECLARE @NewPacientID UNIQUEIDENTIFIER = NEWID();
        
        INSERT INTO Pacienti (
            PacientID,
            Nume,
            Prenume,
            CNP,
            DataNasterii,
            Gen,
            Telefon,
            Email,
            Adresa,
            Oras,
            Judet,
            CodPostal,
            NumeContactUrgenta,
            TelefonContactUrgenta,
            FurnizorAsigurare,
            NumarAsigurare,
            DataCreare,
            DataUltimeiModificari,
            EsteActiv
        )
        VALUES (
            @NewPacientID,
            @Nume,
            @Prenume,
            @CNP,
            @DataNasterii,
            @Gen,
            @Telefon,
            @Email,
            @Adresa,
            @Oras,
            @Judet,
            @CodPostal,
            @NumeContactUrgenta,
            @TelefonContactUrgenta,
            @FurnizorAsigurare,
            @NumarAsigurare,
            GETDATE(),
            GETDATE(),
            1
        );
        
        SELECT @NewPacientID AS PacientID;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO
-- ===== PROCEDURI PENTRU PERSONAL MEDICAL =====

-- Obținere personal medical cu paginare
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_PersonalMedical_GetPaged]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_PersonalMedical_GetPaged]
GO

CREATE PROCEDURE [dbo].[sp_PersonalMedical_GetPaged]
    @Search NVARCHAR(255) = NULL,
    @Departament NVARCHAR(100) = NULL,
    @Pozitie NVARCHAR(50) = NULL,
    @EsteActiv BIT = NULL,
    @Nume NVARCHAR(100) = NULL,
    @Prenume NVARCHAR(100) = NULL,
    @Specializare NVARCHAR(150) = NULL,
    @NumarLicenta NVARCHAR(50) = NULL,
    @Telefon NVARCHAR(30) = NULL,
    @Email NVARCHAR(150) = NULL,
    @SpecializariCsv NVARCHAR(MAX) = NULL,
    @DepartamenteCsv NVARCHAR(MAX) = NULL,
    @PozitiiCsv NVARCHAR(MAX) = NULL,
    @Page INT = 1,
    @PageSize INT = 25,
    @Sort NVARCHAR(400) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF (@Page IS NULL OR @Page < 1) SET @Page = 1;
    IF (@PageSize IS NULL OR @PageSize < 1) SET @PageSize = 25;
    IF (@PageSize > 100000) SET @PageSize = 100000; -- increased cap to allow full-page loads when grouping summaries are needed

    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    -- Parse Sort into @OrderCol and @OrderDir (whitelisted)
    DECLARE @OrderCol NVARCHAR(100) = N'Nume';
    DECLARE @OrderDir NVARCHAR(4) = N'ASC';
    IF (@Sort IS NOT NULL AND LEN(@Sort) > 0)
    BEGIN
        DECLARE @pos INT = CHARINDEX(':', @Sort);
        DECLARE @col NVARCHAR(100) = LTRIM(RTRIM(@Sort));
        DECLARE @dir NVARCHAR(10) = N'asc';
        IF @pos > 0
        BEGIN
            SET @col = LTRIM(RTRIM(SUBSTRING(@Sort, 1, @pos - 1)));
            SET @dir = LTRIM(RTRIM(SUBSTRING(@Sort, @pos + 1, 10)));
        END
        SET @col = CASE @col
            WHEN N'Nume' THEN N'Nume'
            WHEN N'Prenume' THEN N'Prenume'
            WHEN N'Specializare' THEN N'Specializare'
            WHEN N'NumarLicenta' THEN N'NumarLicenta'
            WHEN N'Departament' THEN N'Departament'
            WHEN N'Pozitie' THEN N'Pozitie'
            WHEN N'Telefon' THEN N'Telefon'
            WHEN N'Email' THEN N'Email'
            WHEN N'EsteActiv' THEN N'EsteActiv'
            WHEN N'DataCreare' THEN N'DataCreare'
            ELSE N'Nume' END;
        SET @OrderCol = @col;
        SET @OrderDir = CASE WHEN LOWER(@dir) = 'desc' THEN N'DESC' ELSE N'ASC' END;
    END

    -- Prepare CSV filters into table variables
    DECLARE @tSpec TABLE(val NVARCHAR(150));
    IF (@SpecializariCsv IS NOT NULL AND LTRIM(RTRIM(@SpecializariCsv)) <> '')
        INSERT INTO @tSpec SELECT LTRIM(RTRIM(value)) FROM STRING_SPLIT(@SpecializariCsv, ',');

    DECLARE @tDep TABLE(val NVARCHAR(100));
    IF (@DepartamenteCsv IS NOT NULL AND LTRIM(RTRIM(@DepartamenteCsv)) <> '')
        INSERT INTO @tDep SELECT LTRIM(RTRIM(value)) FROM STRING_SPLIT(@DepartamenteCsv, ',');

    DECLARE @tPoz TABLE(val NVARCHAR(50));
    IF (@PozitiiCsv IS NOT NULL AND LTRIM(RTRIM(@PozitiiCsv)) <> '')
        INSERT INTO @tPoz SELECT LTRIM(RTRIM(value)) FROM STRING_SPLIT(@PozitiiCsv, ',');

    -- Data query
    SELECT 
        PersonalID,
        Nume,
        Prenume,
        Specializare,
        NumarLicenta,
        Telefon,
        Email,
        Departament,
        Pozitie,
        EsteActiv,
        DataCreare
    FROM PersonalMedical WITH (NOLOCK)
    WHERE 1 = 1
      AND (@EsteActiv IS NULL OR EsteActiv = @EsteActiv)
      AND (
            @DepartamenteCsv IS NULL OR NOT EXISTS (SELECT 1 FROM @tDep)
            OR EXISTS (SELECT 1 FROM @tDep d WHERE UPPER(d.val) = UPPER(LTRIM(RTRIM(Departament))))
          )
      AND (
            @PozitiiCsv IS NULL OR NOT EXISTS (SELECT 1 FROM @tPoz)
            OR EXISTS (SELECT 1 FROM @tPoz p WHERE UPPER(p.val) = UPPER(LTRIM(RTRIM(Pozitie))))
          )
      AND (@Departament IS NULL OR @Departament = '' OR @Departament = 'toate' OR UPPER(Departament) LIKE '%' + UPPER(@Departament) + '%')
      AND (@Pozitie IS NULL OR @Pozitie = '' OR @Pozitie = 'toate' OR UPPER(Pozitie) LIKE '%' + UPPER(@Pozitie) + '%')
      AND (@Nume IS NULL OR @Nume = '' OR UPPER(Nume) LIKE '%' + UPPER(@Nume) + '%')
      AND (@Prenume IS NULL OR @Prenume = '' OR UPPER(Prenume) LIKE '%' + UPPER(@Prenume) + '%')
      AND (
            (NOT EXISTS (SELECT 1 FROM @tSpec) AND (@Specializare IS NULL OR @Specializare = '' OR UPPER(Specializare) LIKE '%' + UPPER(@Specializare) + '%'))
            OR EXISTS (SELECT 1 FROM @tSpec s WHERE UPPER(s.val) = UPPER(LTRIM(RTRIM(Specializare))))
          )
      AND (@NumarLicenta IS NULL OR @NumarLicenta = '' OR UPPER(NumarLicenta) LIKE '%' + UPPER(@NumarLicenta) + '%')
      AND (@Telefon IS NULL OR @Telefon = '' OR Telefon LIKE '%' + @Telefon + '%')
      AND (@Email IS NULL OR @Email = '' OR UPPER(Email) LIKE '%' + UPPER(@Email) + '%')
      AND (
            @Search IS NULL OR @Search = ''
            OR UPPER(Nume) LIKE '%' + UPPER(@Search) + '%'
            OR UPPER(Prenume) LIKE '%' + UPPER(@Search) + '%'
            OR UPPER(CONCAT(Nume, ' ', Prenume)) LIKE '%' + UPPER(@Search) + '%'
            OR (Specializare IS NOT NULL AND UPPER(Specializare) LIKE '%' + UPPER(@Search) + '%')
            OR (NumarLicenta IS NOT NULL AND UPPER(NumarLicenta) LIKE '%' + UPPER(@Search) + '%')
            OR (Departament IS NOT NULL AND UPPER(Departament) LIKE '%' + UPPER(@Search) + '%')
            OR (Pozitie IS NOT NULL AND UPPER(Pozitie) LIKE '%' + UPPER(@Search) + '%')
            OR (Telefon IS NOT NULL AND Telefon LIKE '%' + @Search + '%')
            OR (Email IS NOT NULL AND UPPER(Email) LIKE '%' + UPPER(@Search) + '%')
          )
    ORDER BY
        CASE WHEN @OrderCol = 'Nume' AND @OrderDir = 'ASC' THEN Nume END ASC,
        CASE WHEN @OrderCol = 'Nume' AND @OrderDir = 'DESC' THEN Nume END DESC,
        CASE WHEN @OrderCol = 'Prenume' AND @OrderDir = 'ASC' THEN Prenume END ASC,
        CASE WHEN @OrderCol = 'Prenume' AND @OrderDir = 'DESC' THEN Prenume END DESC,
        CASE WHEN @OrderCol = 'Specializare' AND @OrderDir = 'ASC' THEN Specializare END ASC,
        CASE WHEN @OrderCol = 'Specializare' AND @OrderDir = 'DESC' THEN Specializare END DESC,
        CASE WHEN @OrderCol = 'NumarLicenta' AND @OrderDir = 'ASC' THEN NumarLicenta END ASC,
        CASE WHEN @OrderCol = 'NumarLicenta' AND @OrderDir = 'DESC' THEN NumarLicenta END DESC,
        CASE WHEN @OrderCol = 'Departament' AND @OrderDir = 'ASC' THEN Departament END ASC,
        CASE WHEN @OrderCol = 'Departament' AND @OrderDir = 'DESC' THEN Departament END DESC,
        CASE WHEN @OrderCol = 'Pozitie' AND @OrderDir = 'ASC' THEN Pozitie END ASC,
        CASE WHEN @OrderCol = 'Pozitie' AND @OrderDir = 'DESC' THEN Pozitie END DESC,
        CASE WHEN @OrderCol = 'Telefon' AND @OrderDir = 'ASC' THEN Telefon END ASC,
        CASE WHEN @OrderCol = 'Telefon' AND @OrderDir = 'DESC' THEN Telefon END DESC,
        CASE WHEN @OrderCol = 'Email' AND @OrderDir = 'ASC' THEN Email END ASC,
        CASE WHEN @OrderCol = 'Email' AND @OrderDir = 'DESC' THEN Email END DESC,
        CASE WHEN @OrderCol = 'EsteActiv' AND @OrderDir = 'ASC' THEN CONVERT(INT, EsteActiv) END ASC,
        CASE WHEN @OrderCol = 'EsteActiv' AND @OrderDir = 'DESC' THEN CONVERT(INT, EsteActiv) END DESC,
        CASE WHEN @OrderCol = 'DataCreare' AND @OrderDir = 'ASC' THEN DataCreare END ASC,
        CASE WHEN @OrderCol = 'DataCreare' AND @OrderDir = 'DESC' THEN DataCreare END DESC,
        Nume ASC, Prenume ASC -- fallback stable order
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    -- Count query
    SELECT COUNT(1)
    FROM PersonalMedical WITH (NOLOCK)
    WHERE 1 = 1
      AND (@EsteActiv IS NULL OR EsteActiv = @EsteActiv)
      AND (
            @DepartamenteCsv IS NULL OR NOT EXISTS (SELECT 1 FROM @tDep)
            OR EXISTS (SELECT 1 FROM @tDep d WHERE UPPER(d.val) = UPPER(LTRIM(RTRIM(Departament))))
          )
      AND (
            @PozitiiCsv IS NULL OR NOT EXISTS (SELECT 1 FROM @tPoz)
            OR EXISTS (SELECT 1 FROM @tPoz p WHERE UPPER(p.val) = UPPER(LTRIM(RTRIM(Pozitie))))
          )
      AND (@Departament IS NULL OR @Departament = '' OR @Departament = 'toate' OR UPPER(Departament) LIKE '%' + UPPER(@Departament) + '%')
      AND (@Pozitie IS NULL OR @Pozitie = '' OR @Pozitie = 'toate' OR UPPER(Pozitie) LIKE '%' + UPPER(@Pozitie) + '%')
      AND (@Nume IS NULL OR @Nume = '' OR UPPER(Nume) LIKE '%' + UPPER(@Nume) + '%')
      AND (@Prenume IS NULL OR @Prenume = '' OR UPPER(Prenume) LIKE '%' + UPPER(@Prenume) + '%')
      AND (
            (NOT EXISTS (SELECT 1 FROM @tSpec) AND (@Specializare IS NULL OR @Specializare = '' OR UPPER(Specializare) LIKE '%' + UPPER(@Specializare) + '%'))
            OR EXISTS (SELECT 1 FROM @tSpec s WHERE UPPER(s.val) = UPPER(LTRIM(RTRIM(Specializare))))
          )
      AND (@NumarLicenta IS NULL OR @NumarLicenta = '' OR UPPER(NumarLicenta) LIKE '%' + UPPER(@NumarLicenta) + '%')
      AND (@Telefon IS NULL OR @Telefon = '' OR Telefon LIKE '%' + @Telefon + '%')
      AND (@Email IS NULL OR @Email = '' OR UPPER(Email) LIKE '%' + UPPER(@Email) + '%')
      AND (
            @Search IS NULL OR @Search = ''
            OR UPPER(Nume) LIKE '%' + UPPER(@Search) + '%'
            OR UPPER(Prenume) LIKE '%' + UPPER(@Search) + '%'
            OR UPPER(CONCAT(Nume, ' ', Prenume)) LIKE '%' + UPPER(@Search) + '%'
            OR (Specializare IS NOT NULL AND UPPER(Specializare) LIKE '%' + UPPER(@Search) + '%')
            OR (NumarLicenta IS NOT NULL AND UPPER(NumarLicenta) LIKE '%' + UPPER(@Search) + '%')
            OR (Departament IS NOT NULL AND UPPER(Departament) LIKE '%' + UPPER(@Search) + '%')
            OR (Pozitie IS NOT NULL AND UPPER(Pozitie) LIKE '%' + UPPER(@Search) + '%')
            OR (Telefon IS NOT NULL AND Telefon LIKE '%' + @Search + '%')
            OR (Email IS NOT NULL AND UPPER(Email) LIKE '%' + UPPER(@Search) + '%')
          );
END
GO

-- Creare utilizator nou
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Utilizatori_Create]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Utilizatori_Create]
GO

CREATE PROCEDURE [dbo].[sp_Utilizatori_Create]
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @Email NVARCHAR(255),
    @Parola NVARCHAR(255),
    @Telefon NVARCHAR(20) = NULL,
    @Departament NVARCHAR(100) = NULL,
    @Pozitie NVARCHAR(50) = NULL,
    @Specializare NVARCHAR(150) = NULL,
    @NumarLicenta NVARCHAR(50) = NULL,
    @EsteActiv BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserID UNIQUEIDENTIFIER = NEWID();
    DECLARE @Salt NVARCHAR(255) = NEWID();
    DECLARE @HashedParola NVARCHAR(255) = HASHBYTES('SHA2_256', @Parola + @Salt);
    
    INSERT INTO Utilizatori (UserID, Nume, Prenume, Email, Parola, Telefon, Departament, Pozitie, Specializare, NumarLicenta, EsteActiv, DataCreare)
    VALUES (@UserID, @Nume, @Prenume, @Email, @HashedParola, @Telefon, @Departament, @Pozitie, @Specializare, @NumarLicenta, @EsteActiv, GETDATE());
    
    SELECT @UserID AS UserID;
END
GO

-- Obținere utilizator după ID
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Utilizatori_GetByID]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Utilizatori_GetByID]
GO

CREATE PROCEDURE [dbo].[sp_Utilizatori_GetByID]
    @UserID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT UserID, Nume, Prenume, Email, Telefon, Departament, Pozitie, Specializare, NumarLicenta, EsteActiv, DataCreare
    FROM Utilizatori
    WHERE UserID = @UserID;
END
GO

-- Obținere utilizator după email
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Utilizatori_GetByEmail]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Utilizatori_GetByEmail]
GO

CREATE PROCEDURE [dbo].[sp_Utilizatori_GetByEmail]
    @Email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT UserID, Nume, Prenume, Email, Telefon, Departament, Pozitie, Specializare, NumarLicenta, EsteActiv, DataCreare
    FROM Utilizatori
    WHERE Email = @Email;
END
GO

-- Modificare utilizator existent
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Utilizatori_Update]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Utilizatori_Update]
GO

CREATE PROCEDURE [dbo].[sp_Utilizatori_Update]
    @UserID UNIQUEIDENTIFIER,
    @Nume NVARCHAR(100) = NULL,
    @Prenume NVARCHAR(100) = NULL,
    @Email NVARCHAR(255) = NULL,
    @Parola NVARCHAR(255) = NULL,
    @Telefon NVARCHAR(20) = NULL,
    @Departament NVARCHAR(100) = NULL,
    @Pozitie NVARCHAR(50) = NULL,
    @Specializare NVARCHAR(150) = NULL,
    @NumarLicenta NVARCHAR(50) = NULL,
    @EsteActiv BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Sql NVARCHAR(MAX) = N'UPDATE Utilizatori SET ';
    DECLARE @Params NVARCHAR(MAX) = N'';
    
    IF @Nume IS NOT NULL
    BEGIN
        SET @Sql += N'Nume = @Nume, ';
        SET @Params += N'@Nume NVARCHAR(100), ';
    END
    
    IF @Prenume IS NOT NULL
    BEGIN
        SET @Sql += N'Prenume = @Prenume, ';
        SET @Params += N'@Prenume NVARCHAR(100), ';
    END
    
    IF @Email IS NOT NULL
    BEGIN
        SET @Sql += N'Email = @Email, ';
        SET @Params += N'@Email NVARCHAR(255), ';
    END
    
    IF @Parola IS NOT NULL
    BEGIN
        SET @Sql += N'Parola = HASHBYTES(''SHA2_256'', @Parola + ''salt''), ';
        SET @Params += N'@Parola NVARCHAR(255), ';
    END
    
    IF @Telefon IS NOT NULL
    BEGIN
        SET @Sql += N'Telefon = @Telefon, ';
        SET @Params += N'@Telefon NVARCHAR(20), ';
    END
    
    IF @Departament IS NOT NULL
    BEGIN
        SET @Sql += N'Departament = @Departament, ';
        SET @Params += N'@Departament NVARCHAR(100), ';
    END
    
    IF @Pozitie IS NOT NULL
    BEGIN
        SET @Sql += N'Pozitie = @Pozitie, ';
        SET @Params += N'@Pozitie NVARCHAR(50), ';
    END
    
    IF @Specializare IS NOT NULL
    BEGIN
        SET @Sql += N'Specializare = @Specializare, ';
        SET @Params += N'@Specializare NVARCHAR(150), ';
    END
    
    IF @NumarLicenta IS NOT NULL
    BEGIN
        SET @Sql += N'NumarLicenta = @NumarLicenta, ';
        SET @Params += N'@NumarLicenta NVARCHAR(50), ';
    END
    
    IF @EsteActiv IS NOT NULL
    BEGIN
        SET @Sql += N'EsteActiv = @EsteActiv, ';
        SET @Params += N'@EsteActiv BIT, ';
    END
    
    -- Înlătură ultima virgulă și spațiu
    SET @Sql = LEFT(@Sql, LEN(@Sql) - 1);
    SET @Sql += N' WHERE UserID = @UserID';
    
    SET @Params += N'@UserID UNIQUEIDENTIFIER';

    EXEC sp_executesql @Sql, @Params,
        @Nume = @Nume,
        @Prenume = @Prenume,
        @Email = @Email,
        @Parola = @Parola,
        @Telefon = @Telefon,
        @Departament = @Departament,
        @Pozitie = @Pozitie,
        @Specializare = @Specializare,
        @NumarLicenta = @NumarLicenta,
        @EsteActiv = @EsteActiv,
        @UserID = @UserID;
END
GO

-- Schimbare parolă utilizator
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Utilizatori_ChangePassword]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Utilizatori_ChangePassword]
GO

CREATE PROCEDURE [dbo].[sp_Utilizatori_ChangePassword]
    @UserID UNIQUEIDENTIFIER,
    @OldParola NVARCHAR(255),
    @NewParola NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Hash inline to avoid quoting issues and keep consistency with sp_Utilizatori_Update
    UPDATE Utilizatori
    SET Parola = HASHBYTES('SHA2_256', @NewParola + 'salt')
    WHERE UserID = @UserID 
      AND Parola = HASHBYTES('SHA2_256', @OldParola + 'salt');
END
GO