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

    DECLARE @sql NVARCHAR(MAX) = N'
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
    FROM PersonalMedical
    WHERE EsteActiv = 1
      AND (@Search IS NULL OR @Search = '''' OR 
           UPPER(Nume) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
           UPPER(Prenume) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
           UPPER(Specializare) LIKE ''%'' + UPPER(@Search) + ''%'' OR
           UPPER(NumarLicenta) LIKE ''%'' + UPPER(@Search) + ''%'')
      AND (@Departament IS NULL OR @Departament = '''' OR @Departament = ''toate'' OR Departament = @Departament)
      AND (@Pozitie IS NULL OR @Pozitie = '''' OR @Pozitie = ''toate'' OR Pozitie = @Pozitie)
    ORDER BY ' + @OrderBy + '
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(1)
    FROM PersonalMedical
    WHERE EsteActiv = 1
      AND (@Search IS NULL OR @Search = '''' OR 
           UPPER(Nume) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
           UPPER(Prenume) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
           UPPER(Specializare) LIKE ''%'' + UPPER(@Search) + ''%'' OR
           UPPER(NumarLicenta) LIKE ''%'' + UPPER(@Search) + ''%'')
      AND (@Departament IS NULL OR @Departament = '''' OR @Departament = ''toate'' OR Departament = @Departament)
      AND (@Pozitie IS NULL OR @Pozitie = '''' OR @Pozitie = ''toate'' OR Pozitie = @Pozitie);';

    EXEC sp_executesql
        @sql,
        N'@Search NVARCHAR(255), @Departament NVARCHAR(100), @Pozitie NVARCHAR(50), @Offset INT, @PageSize INT',
        @Search = @Search, @Departament = @Departament, @Pozitie = @Pozitie, @Offset = @Offset, @PageSize = @PageSize;
END
GO

-- Obținere doctori pentru dropdown-uri
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_PersonalMedical_GetDoctori]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_PersonalMedical_GetDoctori]
GO

CREATE PROCEDURE [dbo].[sp_PersonalMedical_GetDoctori]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        PersonalID,
        CONCAT(Nume, ' ', Prenume) AS NumeComplet,
        Specializare,
        Departament
    FROM PersonalMedical
    WHERE EsteActiv = 1 
      AND Pozitie IN ('Doctor Primar', 'Doctor Specialist')
    ORDER BY Nume, Prenume;
END
GO

-- ===== PROCEDURI PENTRU PROGRAMĂRI =====

-- Obținere programări cu paginare
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Programari_GetPaged]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Programari_GetPaged]
GO

CREATE PROCEDURE [dbo].[sp_Programari_GetPaged]
    @DataStart DATE = NULL,
    @DataEnd DATE = NULL,
    @DoctorID UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50) = NULL,
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
    DECLARE @OrderBy NVARCHAR(MAX) = N'pr.DataProgramare DESC';

    DECLARE @sql NVARCHAR(MAX) = N'
    SELECT 
        pr.ProgramareID,
        pr.DataProgramare,
        pr.TipProgramare,
        pr.Status,
        pr.Observatii,
        pr.DataCreare,
        CONCAT(p.Nume, '' '', p.Prenume) AS NumePacient,
        p.CNP AS CNPPacient,
        p.Telefon AS TelefonPacient,
        CONCAT(pm.Nume, '' '', pm.Prenume) AS NumeDoctor,
        pm.Specializare AS SpecializareDoctor
    FROM Programari pr
    INNER JOIN Pacienti p ON pr.PacientID = p.PacientID
    INNER JOIN PersonalMedical pm ON pr.DoctorID = pm.PersonalID
    WHERE 1=1
      AND (@DataStart IS NULL OR CAST(pr.DataProgramare AS DATE) >= @DataStart)
      AND (@DataEnd IS NULL OR CAST(pr.DataProgramare AS DATE) <= @DataEnd)
      AND (@DoctorID IS NULL OR pr.DoctorID = @DoctorID)
      AND (@Status IS NULL OR @Status = '''' OR @Status = ''toate'' OR pr.Status = @Status)
    ORDER BY ' + @OrderBy + '
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(1)
    FROM Programari pr
    INNER JOIN Pacienti p ON pr.PacientID = p.PacientID
    INNER JOIN PersonalMedical pm ON pr.DoctorID = pm.PersonalID
    WHERE 1=1
      AND (@DataStart IS NULL OR CAST(pr.DataProgramare AS DATE) >= @DataStart)
      AND (@DataEnd IS NULL OR CAST(pr.DataProgramare AS DATE) <= @DataEnd)
      AND (@DoctorID IS NULL OR pr.DoctorID = @DoctorID)
      AND (@Status IS NULL OR @Status = '''' OR @Status = ''toate'' OR pr.Status = @Status);';

    EXEC sp_executesql
        @sql,
        N'@DataStart DATE, @DataEnd DATE, @DoctorID UNIQUEIDENTIFIER, @Status NVARCHAR(50), @Offset INT, @PageSize INT',
        @DataStart = @DataStart, @DataEnd = @DataEnd, @DoctorID = @DoctorID, @Status = @Status, @Offset = @Offset, @PageSize = @PageSize;
END
GO

-- Creare programare nouă
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Programari_Create]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Programari_Create]
GO

CREATE PROCEDURE [dbo].[sp_Programari_Create]
    @PacientID UNIQUEIDENTIFIER,
    @DoctorID UNIQUEIDENTIFIER,
    @DataProgramare DATETIME2,
    @TipProgramare NVARCHAR(100),
    @Observatii NVARCHAR(1000) = NULL,
    @CreatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Verificare conflict de programare pentru doctor
        IF EXISTS (
            SELECT 1 FROM Programari 
            WHERE DoctorID = @DoctorID 
              AND ABS(DATEDIFF(MINUTE, DataProgramare, @DataProgramare)) < 30
              AND Status NOT IN ('Anulata')
        )
        BEGIN
            RAISERROR('Doctorul are deja o programare în acest interval de timp.', 16, 1);
            RETURN;
        END
        
        DECLARE @NewProgramareID UNIQUEIDENTIFIER = NEWID();
        
        INSERT INTO Programari (
            ProgramareID,
            PacientID,
            DoctorID,
            DataProgramare,
            TipProgramare,
            Status,
            Observatii,
            DataCreare,
            CreatDe
        )
        VALUES (
            @NewProgramareID,
            @PacientID,
            @DoctorID,
            @DataProgramare,
            @TipProgramare,
            'Programata',
            @Observatii,
            GETDATE(),
            @CreatDe
        );
        
        SELECT @NewProgramareID AS ProgramareID;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- Actualizare status programare
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Programari_UpdateStatus]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Programari_UpdateStatus]
GO

CREATE PROCEDURE [dbo].[sp_Programari_UpdateStatus]
    @ProgramareID UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @ModificatDe UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Validare status
        IF @Status NOT IN ('Programata', 'Confirmata', 'In Asteptare', 'In Consultatie', 'Finalizata', 'Anulata', 'Nu s-a prezentat', 'Amanata')
        BEGIN
            RAISERROR('Status programare invalid.', 16, 1);
            RETURN;
        END
        
        -- Verifică dacă programarea există
        IF NOT EXISTS (SELECT 1 FROM Programari WHERE ProgramareID = @ProgramareID)
        BEGIN
            RAISERROR('Programarea nu a fost găsită.', 16, 1);
            RETURN;
        END
        
        UPDATE Programari 
        SET 
            Status = @Status,
            DataUltimeiModificari = GETDATE()
        WHERE ProgramareID = @ProgramareID;
        
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

-- ===== PROCEDURI PENTRU CONSULTAȚII =====

-- Obținere consultații pacient
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Consultatii_GetByPacient]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Consultatii_GetByPacient]
GO

CREATE PROCEDURE [dbo].[sp_Consultatii_GetByPacient]
    @PacientID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.ConsultatieID,
        c.PlangereaPrincipala,
        c.IstoricBoalaActuala,
        c.ExamenFizic,
        c.Evaluare,
        c.[Plan],
        c.DataConsultatie,
        c.Durata,
        pr.DataProgramare,
        pr.TipProgramare,
        CONCAT(pm.Nume, ' ', pm.Prenume) AS NumeDoctor,
        pm.Specializare AS SpecializareDoctor
    FROM Consultatii c
    INNER JOIN Programari pr ON c.ProgramareID = pr.ProgramareID
    INNER JOIN PersonalMedical pm ON pr.DoctorID = pm.PersonalID
    WHERE pr.PacientID = @PacientID
    ORDER BY c.DataConsultatie DESC;
END
GO

-- ===== PROCEDURI PENTRU UTILIZATORI SISTEM =====

-- Autentificare utilizator
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UtilizatoriSistem_Authenticate]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UtilizatoriSistem_Authenticate]
GO

CREATE PROCEDURE [dbo].[sp_UtilizatoriSistem_Authenticate]
    @NumeUtilizatorSauEmail NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UtilizatorID,
        u.NumeUtilizator,
        u.HashParola,
        u.Email,
        u.PersonalID,
        u.EsteActiv,
        u.DataUltimeiAutentificari,
        u.IncercariEsuateAutentificare,
        u.BlocatPanaLa,
        CONCAT(pm.Nume, ' ', pm.Prenume) AS NumeComplet,
        pm.Pozitie,
        pm.Departament,
        STRING_AGG(rs.NumeRol, ',') AS Roluri
    FROM UtilizatoriSistem u
    LEFT JOIN PersonalMedical pm ON u.PersonalID = pm.PersonalID
    LEFT JOIN UtilizatorRoluri ur ON u.UtilizatorID = ur.UtilizatorID
    LEFT JOIN RoluriSistem rs ON ur.RolID = rs.RolID AND rs.EsteActiv = 1
    WHERE (u.NumeUtilizator = @NumeUtilizatorSauEmail OR u.Email = @NumeUtilizatorSauEmail)
      AND u.EsteActiv = 1
    GROUP BY 
        u.UtilizatorID, u.NumeUtilizator, u.HashParola, u.Email, u.PersonalID, 
        u.EsteActiv, u.DataUltimeiAutentificari, u.IncercariEsuateAutentificare, 
        u.BlocatPanaLa, pm.Nume, pm.Prenume, pm.Pozitie, pm.Departament;
END
GO

-- Actualizare ultima autentificare
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UtilizatoriSistem_UpdateLastLogin]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UtilizatoriSistem_UpdateLastLogin]
GO

CREATE PROCEDURE [dbo].[sp_UtilizatoriSistem_UpdateLastLogin]
    @UtilizatorID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE UtilizatoriSistem 
    SET 
        DataUltimeiAutentificari = GETDATE(),
        IncercariEsuateAutentificare = 0,
        BlocatPanaLa = NULL
    WHERE UtilizatorID = @UtilizatorID;
END
GO

-- ===== PROCEDURI PENTRU DASHBOARD ȘI STATISTICI =====

-- Dashboard principal - statistici generale
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Dashboard_GetStatistici]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Dashboard_GetStatistici]
GO

CREATE PROCEDURE [dbo].[sp_Dashboard_GetStatistici]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Statistici generale
    SELECT 
        'PacientiTotal' AS Tip,
        COUNT(*) AS Valoare
    FROM Pacienti 
    WHERE EsteActiv = 1
    
    UNION ALL
    
    SELECT 
        'ProgramariAzi' AS Tip,
        COUNT(*) AS Valoare
    FROM Programari 
    WHERE CAST(DataProgramare AS DATE) = CAST(GETDATE() AS DATE)
      AND Status = 'Programata'
    
    UNION ALL
    
    SELECT 
        'ConsultatiiLunaAceasta' AS Tip,
        COUNT(*) AS Valoare
    FROM Consultatii 
    WHERE YEAR(DataConsultatie) = YEAR(GETDATE()) 
      AND MONTH(DataConsultatie) = MONTH(GETDATE())
    
    UNION ALL
    
    SELECT 
        'PersonalActiv' AS Tip,
        COUNT(*) AS Valoare
    FROM PersonalMedical 
    WHERE EsteActiv = 1;
    
    -- Programările de astăzi
    SELECT 
        pr.ProgramareID,
        pr.DataProgramare,
        pr.TipProgramare,
        pr.Status,
        CONCAT(p.Nume, ' ', p.Prenume) AS NumePacient,
        p.Telefon AS TelefonPacient,
        CONCAT(pm.Nume, ' ', pm.Prenume) AS NumeDoctor
    FROM Programari pr
    INNER JOIN Pacienti p ON pr.PacientID = p.PacientID
    INNER JOIN PersonalMedical pm ON pr.DoctorID = pm.PersonalID
    WHERE CAST(pr.DataProgramare AS DATE) = CAST(GETDATE() AS DATE)
    ORDER BY pr.DataProgramare;
END
GO

-- ===== PROCEDURI PENTRU SEMNE VITALE =====

-- Crearea unei înregistrări de semne vitale
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SemneVitale_Create]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_SemneVitale_Create]
GO

CREATE PROCEDURE [dbo].[sp_SemneVitale_Create]
    @PacientID UNIQUEIDENTIFIER,
    @TensiuneArterialaMax INT = NULL,
    @TensiuneArterialaMin INT = NULL,
    @FrecariaCardiaca INT = NULL,
    @Temperatura DECIMAL(4,1) = NULL,
    @Greutate DECIMAL(5,2) = NULL,
    @Inaltime INT = NULL,
    @FrecariaRespiratorie INT = NULL,
    @SaturatieOxigen DECIMAL(5,2) = NULL,
    @MasuratDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        DECLARE @NewSemneVitaleID UNIQUEIDENTIFIER = NEWID();
        
        INSERT INTO SemneVitale (
            SemneVitaleID,
            PacientID,
            TensiuneArterialaMax,
            TensiuneArterialaMin,
            FrecariaCardiaca,
            Temperatura,
            Greutate,
            Inaltime,
            FrecariaRespiratorie,
            SaturatieOxigen,
            MasuratDe,
            DataMasurare
        )
        VALUES (
            @NewSemneVitaleID,
            @PacientID,
            @TensiuneArterialaMax,
            @TensiuneArterialaMin,
            @FrecariaCardiaca,
            @Temperatura,
            @Greutate,
            @Inaltime,
            @FrecariaRespiratorie,
            @SaturatieOxigen,
            @MasuratDe,
            GETDATE()
        );
        
        SELECT @NewSemneVitaleID AS SemneVitaleID;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- Obținere semne vitale pentru pacient
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SemneVitale_GetByPacient]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_SemneVitale_GetByPacient]
GO

CREATE PROCEDURE [dbo].[sp_SemneVitale_GetByPacient]
    @PacientID UNIQUEIDENTIFIER,
    @Limit INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Limit)
        sv.SemneVitaleID,
        sv.PacientID,
        sv.TensiuneArterialaMax,
        sv.TensiuneArterialaMin,
        sv.FrecariaCardiaca,
        sv.Temperatura,
        sv.Greutate,
        sv.Inaltime,
        sv.FrecariaRespiratorie,
        sv.SaturatieOxigen,
        sv.DataMasurare,
        CONCAT(pm.Nume, ' ', pm.Prenume) AS MasuratDe
    FROM SemneVitale sv
    LEFT JOIN PersonalMedical pm ON sv.MasuratDe = pm.PersonalID
    WHERE sv.PacientID = @PacientID
    ORDER BY sv.DataMasurare DESC;
END
GO

-- ===== PROCEDURI PENTRU TRIAJ =====

-- Crearea unei înregistrări de triaj
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_TriajPacienti_Create]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_TriajPacienti_Create]
GO

CREATE PROCEDURE [dbo].[sp_TriajPacienti_Create]
    @ProgramareID UNIQUEIDENTIFIER,
    @NivelTriaj INT,
    @PlangereaPrincipala NVARCHAR(1000),
    @AsistentTriajID UNIQUEIDENTIFIER,
    @Observatii NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Validare nivel triaj
        IF @NivelTriaj NOT BETWEEN 1 AND 5
        BEGIN
            RAISERROR('Nivelul de triaj trebuie să fie între 1 și 5.', 16, 1);
            RETURN;
        END
        
        DECLARE @NewTriajID UNIQUEIDENTIFIER = NEWID();
        
        INSERT INTO TriajPacienti (
            TriajID,
            ProgramareID,
            NivelTriaj,
            PlangereaPrincipala,
            AsistentTriajID,
            DataTriaj,
            Observatii
        )
        VALUES (
            @NewTriajID,
            @ProgramareID,
            @NivelTriaj,
            @PlangereaPrincipala,
            @AsistentTriajID,
            GETDATE(),
            @Observatii
        );
        
        SELECT @NewTriajID AS TriajID;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- Obținere triaj pentru astăzi
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_TriajPacienti_GetToday]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_TriajPacienti_GetToday]
GO

CREATE PROCEDURE [dbo].[sp_TriajPacienti_GetToday]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        t.TriajID,
        t.ProgramareID,
        t.NivelTriaj,
        t.PlangereaPrincipala,
        t.DataTriaj,
        t.Observatii,
        CONCAT(p.Nume, ' ', p.Prenume) AS NumePacient,
        p.CNP AS CNPPacient,
        p.DataNasterii,
        pr.DataProgramare,
        CONCAT(pm.Nume, ' ', pm.Prenume) AS NumeDoctor,
        CONCAT(ast.Nume, ' ', ast.Prenume) AS AsistentTriaj
    FROM TriajPacienti t
    INNER JOIN Programari pr ON t.ProgramareID = pr.ProgramareID
    INNER JOIN Pacienti p ON pr.PacientID = p.PacientID
    INNER JOIN PersonalMedical pm ON pr.DoctorID = pm.PersonalID
    LEFT JOIN PersonalMedical ast ON t.AsistentTriajID = ast.PersonalID
    WHERE CAST(pr.DataProgramare AS DATE) = CAST(GETDATE() AS DATE)
    ORDER BY t.NivelTriaj ASC, t.DataTriaj ASC;
END
GO

PRINT '';
PRINT '===============================================';
PRINT 'PROCEDURI STOCATE PENTRU SISTEMUL MEDICAL COMPLETE!';
PRINT '===============================================';
PRINT '';
PRINT 'PROCEDURI CREATE:';
PRINT '=================';
PRINT '• sp_Pacienti_GetPaged - Căutare și paginare pacienți';
PRINT '• sp_Pacienti_GetByCNP - Căutare pacient după CNP';
PRINT '• sp_Pacienti_Create - Creare pacient nou';
PRINT '• sp_PersonalMedical_GetPaged - Căutare personal medical';
PRINT '• sp_PersonalMedical_GetDoctori - Lista doctorilor pentru dropdown-uri';
PRINT '• sp_Programari_GetPaged - Căutare și paginare programări';
PRINT '• sp_Programari_Create - Creare programare nouă cu validări';
PRINT '• sp_Programari_UpdateStatus - Actualizare status programare';
PRINT '• sp_Consultatii_GetByPacient - Istoric consultații pacient';
PRINT '• sp_SemneVitale_Create - Creare înregistrare semne vitale';
PRINT '• sp_SemneVitale_GetByPacient - Istoric semne vitale pacient';
PRINT '• sp_TriajPacienti_Create - Creare înregistrare triaj';
PRINT '• sp_TriajPacienti_GetToday - Triaj pentru astăzi';
PRINT '• sp_UtilizatoriSistem_Authenticate - Autentificare utilizatori';
PRINT '• sp_UtilizatoriSistem_UpdateLastLogin - Actualizare ultima autentificare';
PRINT '• sp_Dashboard_GetStatistici - Statistici pentru dashboard';
PRINT '';
PRINT 'FUNCȚIONALITĂȚI INCLUSE:';
PRINT '========================';
PRINT '• Rich business logic în proceduri (nu doar forwarding)';
PRINT '• Validări pentru conflicte de programare și nivel triaj';
PRINT '• Căutare avansată cu filtre multiple';
PRINT '• Paginare optimizată pentru performance';
PRINT '• Join-uri pentru afișare date complete';
PRINT '• Error handling specific și informativ';
PRINT '• Suport pentru semne vitale și triaj medical';
PRINT '• Actualizare status programări cu validări';
PRINT '';
PRINT 'NEXT STEPS:';
PRINT '===========';
PRINT '1. Rulează Medical_Tables_Structure.sql pentru structura tabelelor';
PRINT '2. Rulează Medical_Data_Population.sql pentru datele dummy';
PRINT '3. Rulează Medical_StoredProcedures.sql (acest fișier)';
PRINT '4. Implementează API-urile care folosesc aceste proceduri';
PRINT '5. Testează aplicația Blazor cu datele reale';
PRINT '';
PRINT 'CORECTĂRI APLICATE:';
PRINT '===================';
PRINT '✅ NEWID() în loc de NEWSEQUENTIALID() în proceduri';
PRINT '✅ Adăugată coloana DataUltimeiModificari în Programari';
PRINT '✅ Adăugată coloana PacientID în SemneVitale';
PRINT '✅ Proceduri pentru toate funcționalitățile Blazor';
PRINT '✅ Validări business și error handling complet';