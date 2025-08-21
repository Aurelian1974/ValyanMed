/*
    Enhanced Stored Procedures with Group By and Multi-Sort Support
    For eliminating server-side grids and using only client-side with all data loaded
    Usage: Load ALL data with grouping and sorting applied server-side
*/

-- =============================================================================
-- Enhanced MaterialeSanitare Procedure with GroupBy Support
-- =============================================================================
IF OBJECT_ID('dbo.sp_MaterialeSanitare_GetAllGrouped','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_MaterialeSanitare_GetAllGrouped AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_MaterialeSanitare_GetAllGrouped
    @Search NVARCHAR(255) = NULL,
    @Categorie NVARCHAR(100) = NULL,
    @GroupBy NVARCHAR(200) = NULL,
    @Sort NVARCHAR(400) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Build ORDER BY from GroupBy + Sort (whitelisted)
    DECLARE @OrderBy NVARCHAR(MAX) = N'';
    DECLARE @token NVARCHAR(100), @dir NVARCHAR(4);

    -- Append group-by columns (ASC)
    DECLARE @GroupByWork NVARCHAR(200) = @GroupBy;
    WHILE (@GroupByWork IS NOT NULL AND LEN(@GroupByWork) > 0)
    BEGIN
        DECLARE @pos INT = CHARINDEX(',', @GroupByWork);
        IF @pos > 0
        BEGIN
            SET @token = LTRIM(RTRIM(SUBSTRING(@GroupByWork, 1, @pos - 1)));
            SET @GroupByWork = SUBSTRING(@GroupByWork, @pos + 1, LEN(@GroupByWork));
        END
        ELSE
        BEGIN
            SET @token = LTRIM(RTRIM(@GroupByWork));
            SET @GroupByWork = NULL;
        END

        SET @token = CASE @token
            WHEN N'Id' THEN N'Id'
            WHEN N'Denumire' THEN N'Denumire'
            WHEN N'Categorie' THEN N'Categorie'
            WHEN N'UnitateaMasura' THEN N'UnitateaMasura'
            WHEN N'Sterile' THEN N'Sterile'
            WHEN N'UniUzinta' THEN N'UniUzinta'
            WHEN N'DataCreare' THEN N'DataCreare'
            WHEN N'DataModificare' THEN N'DataModificare'
            ELSE NULL END;

        IF (@token IS NOT NULL)
            SET @OrderBy = CONCAT(@OrderBy, CASE WHEN LEN(@OrderBy)>0 THEN N', ' ELSE N'' END, @token, N' ASC');
    END

    -- Append sort definitions
    DECLARE @SortWork NVARCHAR(400) = @Sort;
    WHILE (@SortWork IS NOT NULL AND LEN(@SortWork) > 0)
    BEGIN
        DECLARE @part NVARCHAR(100);
        DECLARE @pos2 INT = CHARINDEX(',', @SortWork);
        IF @pos2 > 0
        BEGIN
            SET @part = LTRIM(RTRIM(SUBSTRING(@SortWork, 1, @pos2 - 1)));
            SET @SortWork = SUBSTRING(@SortWork, @pos2 + 1, LEN(@SortWork));
        END
        ELSE
        BEGIN
            SET @part = LTRIM(RTRIM(@SortWork));
            SET @SortWork = NULL;
        END

        -- split part as col[:dir]
        DECLARE @col NVARCHAR(100) = @part;
        DECLARE @colon INT = CHARINDEX(':', @part);
        IF @colon > 0
        BEGIN
            SET @col = LTRIM(RTRIM(SUBSTRING(@part, 1, @colon - 1)));
            SET @dir = LTRIM(RTRIM(SUBSTRING(@part, @colon + 1, 10)));
        END
        ELSE SET @dir = N'asc';

        SET @col = CASE @col
            WHEN N'Id' THEN N'Id'
            WHEN N'Denumire' THEN N'Denumire'
            WHEN N'Categorie' THEN N'Categorie'
            WHEN N'UnitateaMasura' THEN N'UnitateaMasura'
            WHEN N'Sterile' THEN N'Sterile'
            WHEN N'UniUzinta' THEN N'UniUzinta'
            WHEN N'DataCreare' THEN N'DataCreare'
            WHEN N'DataModificare' THEN N'DataModificare'
            ELSE NULL END;

        IF (@col IS NOT NULL)
        BEGIN
            SET @dir = CASE WHEN LOWER(@dir) = 'desc' THEN N'DESC' ELSE N'ASC' END;
            SET @OrderBy = CONCAT(@OrderBy, CASE WHEN LEN(@OrderBy)>0 THEN N', ' ELSE N'' END, @col, N' ', @dir);
        END
    END

    IF (LEN(@OrderBy) = 0) SET @OrderBy = N'Denumire ASC';

    DECLARE @sql NVARCHAR(MAX) = N'
SELECT Id, Guid, Denumire, Categorie, Specificatii, UnitateaMasura, Sterile, UniUzinta, DataCreare, DataModificare
FROM dbo.MaterialeSanitare WITH (NOLOCK)
WHERE (@Search IS NULL OR @Search = '''' OR 
       UPPER(Denumire) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
       UPPER(Categorie) LIKE ''%'' + UPPER(@Search) + ''%'' OR
       UPPER(Specificatii) LIKE ''%'' + UPPER(@Search) + ''%'')
  AND (@Categorie IS NULL OR @Categorie = '''' OR @Categorie = ''toate'' OR Categorie = @Categorie)
ORDER BY ' + @OrderBy + '
OPTION (RECOMPILE);';

    EXEC sp_executesql
        @sql,
        N'@Search NVARCHAR(255), @Categorie NVARCHAR(100)',
        @Search = @Search, @Categorie = @Categorie;
END
GO

-- =============================================================================
-- Enhanced DispozitiveMedicale Procedure with GroupBy Support  
-- =============================================================================
IF OBJECT_ID('dbo.sp_DispozitiveMedicale_GetAllGrouped','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_DispozitiveMedicale_GetAllGrouped AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_DispozitiveMedicale_GetAllGrouped
    @Search NVARCHAR(255) = NULL,
    @Categorie NVARCHAR(100) = NULL,
    @ClasaRisc NVARCHAR(10) = NULL,
    @GroupBy NVARCHAR(200) = NULL,
    @Sort NVARCHAR(400) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Build ORDER BY from GroupBy + Sort (whitelisted)
    DECLARE @OrderBy NVARCHAR(MAX) = N'';
    DECLARE @token NVARCHAR(100), @dir NVARCHAR(4);

    -- Append group-by columns (ASC)
    DECLARE @GroupByWork NVARCHAR(200) = @GroupBy;
    WHILE (@GroupByWork IS NOT NULL AND LEN(@GroupByWork) > 0)
    BEGIN
        DECLARE @pos INT = CHARINDEX(',', @GroupByWork);
        IF @pos > 0
        BEGIN
            SET @token = LTRIM(RTRIM(SUBSTRING(@GroupByWork, 1, @pos - 1)));
            SET @GroupByWork = SUBSTRING(@GroupByWork, @pos + 1, LEN(@GroupByWork));
        END
        ELSE
        BEGIN
            SET @token = LTRIM(RTRIM(@GroupByWork));
            SET @GroupByWork = NULL;
        END

        SET @token = CASE @token
            WHEN N'Id' THEN N'Id'
            WHEN N'Denumire' THEN N'Denumire'
            WHEN N'Categorie' THEN N'Categorie'
            WHEN N'ClasaRisc' THEN N'ClasaRisc'
            WHEN N'Producator' THEN N'Producator'
            WHEN N'ModelTip' THEN N'ModelTip'
            WHEN N'CertificareCE' THEN N'CertificareCE'
            WHEN N'DataExpirare' THEN N'DataExpirare'
            WHEN N'DataCreare' THEN N'DataCreare'
            WHEN N'DataModificare' THEN N'DataModificare'
            ELSE NULL END;

        IF (@token IS NOT NULL)
            SET @OrderBy = CONCAT(@OrderBy, CASE WHEN LEN(@OrderBy)>0 THEN N', ' ELSE N'' END, @token, N' ASC');
    END

    -- Append sort definitions
    DECLARE @SortWork NVARCHAR(400) = @Sort;
    WHILE (@SortWork IS NOT NULL AND LEN(@SortWork) > 0)
    BEGIN
        DECLARE @part NVARCHAR(100);
        DECLARE @pos2 INT = CHARINDEX(',', @SortWork);
        IF @pos2 > 0
        BEGIN
            SET @part = LTRIM(RTRIM(SUBSTRING(@SortWork, 1, @pos2 - 1)));
            SET @SortWork = SUBSTRING(@SortWork, @pos2 + 1, LEN(@SortWork));
        END
        ELSE
        BEGIN
            SET @part = LTRIM(RTRIM(@SortWork));
            SET @SortWork = NULL;
        END

        -- split part as col[:dir]
        DECLARE @col NVARCHAR(100) = @part;
        DECLARE @colon INT = CHARINDEX(':', @part);
        IF @colon > 0
        BEGIN
            SET @col = LTRIM(RTRIM(SUBSTRING(@part, 1, @colon - 1)));
            SET @dir = LTRIM(RTRIM(SUBSTRING(@part, @colon + 1, 10)));
        END
        ELSE SET @dir = N'asc';

        SET @col = CASE @col
            WHEN N'Id' THEN N'Id'
            WHEN N'Denumire' THEN N'Denumire'
            WHEN N'Categorie' THEN N'Categorie'
            WHEN N'ClasaRisc' THEN N'ClasaRisc'
            WHEN N'Producator' THEN N'Producator'
            WHEN N'ModelTip' THEN N'ModelTip'
            WHEN N'CertificareCE' THEN N'CertificareCE'
            WHEN N'DataExpirare' THEN N'DataExpirare'
            WHEN N'DataCreare' THEN N'DataCreare'
            WHEN N'DataModificare' THEN N'DataModificare'
            ELSE NULL END;

        IF (@col IS NOT NULL)
        BEGIN
            SET @dir = CASE WHEN LOWER(@dir) = 'desc' THEN N'DESC' ELSE N'ASC' END;
            SET @OrderBy = CONCAT(@OrderBy, CASE WHEN LEN(@OrderBy)>0 THEN N', ' ELSE N'' END, @col, N' ', @dir);
        END
    END

    IF (LEN(@OrderBy) = 0) SET @OrderBy = N'Denumire ASC';

    DECLARE @sql NVARCHAR(MAX) = N'
SELECT Id, Guid, Denumire, Categorie, ClasaRisc, Producator, ModelTip, NumarSerie, CertificareCE, DataExpirare, Specificatii, DataCreare, DataModificare
FROM dbo.DispozitiveMedicale WITH (NOLOCK)
WHERE (@Search IS NULL OR @Search = '''' OR 
       UPPER(Denumire) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
       UPPER(Categorie) LIKE ''%'' + UPPER(@Search) + ''%'' OR
       UPPER(Producator) LIKE ''%'' + UPPER(@Search) + ''%'' OR
       UPPER(ModelTip) LIKE ''%'' + UPPER(@Search) + ''%'' OR
       UPPER(Specificatii) LIKE ''%'' + UPPER(@Search) + ''%'')
  AND (@Categorie IS NULL OR @Categorie = '''' OR @Categorie = ''toate'' OR Categorie = @Categorie)
  AND (@ClasaRisc IS NULL OR @ClasaRisc = '''' OR @ClasaRisc = ''toate'' OR ClasaRisc = @ClasaRisc)
ORDER BY ' + @OrderBy + '
OPTION (RECOMPILE);';

    EXEC sp_executesql
        @sql,
        N'@Search NVARCHAR(255), @Categorie NVARCHAR(100), @ClasaRisc NVARCHAR(10)',
        @Search = @Search, @Categorie = @Categorie, @ClasaRisc = @ClasaRisc;
END
GO

-- =============================================================================
-- Enhanced Medicament Procedure - modify existing to GetAllGrouped
-- =============================================================================
IF OBJECT_ID('dbo.sp_Medicament_GetAllGrouped','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Medicament_GetAllGrouped AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_Medicament_GetAllGrouped
    @Search NVARCHAR(200) = NULL,
    @Status NVARCHAR(50) = NULL,
    @GroupBy NVARCHAR(200) = NULL,
    @Sort NVARCHAR(400) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Build ORDER BY from GroupBy + Sort (whitelisted)
    DECLARE @OrderBy NVARCHAR(MAX) = N'';
    DECLARE @token NVARCHAR(100), @dir NVARCHAR(4);

    -- Append group-by columns (ASC)
    DECLARE @GroupByWork NVARCHAR(200) = @GroupBy;
    WHILE (@GroupByWork IS NOT NULL AND LEN(@GroupByWork) > 0)
    BEGIN
        DECLARE @pos INT = CHARINDEX(',', @GroupByWork);
        IF @pos > 0
        BEGIN
            SET @token = LTRIM(RTRIM(SUBSTRING(@GroupByWork, 1, @pos - 1)));
            SET @GroupByWork = SUBSTRING(@GroupByWork, @pos + 1, LEN(@GroupByWork));
        END
        ELSE
        BEGIN
            SET @token = LTRIM(RTRIM(@GroupByWork));
            SET @GroupByWork = NULL;
        END

        SET @token = CASE @token
            WHEN N'MedicamentID' THEN N'MedicamentID'
            WHEN N'Nume' THEN N'Nume'
            WHEN N'DenumireComunaInternationala' THEN N'DenumireComunaInternationala'
            WHEN N'Concentratie' THEN N'Concentratie'
            WHEN N'FormaFarmaceutica' THEN N'FormaFarmaceutica'
            WHEN N'Producator' THEN N'Producator'
            WHEN N'CodATC' THEN N'CodATC'
            WHEN N'Status' THEN N'Status'
            WHEN N'Stoc' THEN N'Stoc'
            WHEN N'StocSiguranta' THEN N'StocSiguranta'
            WHEN N'DataInregistrare' THEN N'DataInregistrare'
            WHEN N'DataExpirare' THEN N'DataExpirare'
            ELSE NULL END;

        IF (@token IS NOT NULL)
            SET @OrderBy = CONCAT(@OrderBy, CASE WHEN LEN(@OrderBy)>0 THEN N', ' ELSE N'' END, @token, N' ASC');
    END

    -- Append sort definitions
    DECLARE @SortWork NVARCHAR(400) = @Sort;
    WHILE (@SortWork IS NOT NULL AND LEN(@SortWork) > 0)
    BEGIN
        DECLARE @part NVARCHAR(100);
        DECLARE @pos2 INT = CHARINDEX(',', @SortWork);
        IF @pos2 > 0
        BEGIN
            SET @part = LTRIM(RTRIM(SUBSTRING(@SortWork, 1, @pos2 - 1)));
            SET @SortWork = SUBSTRING(@SortWork, @pos2 + 1, LEN(@SortWork));
        END
        ELSE
        BEGIN
            SET @part = LTRIM(RTRIM(@SortWork));
            SET @SortWork = NULL;
        END

        -- split part as col[:dir]
        DECLARE @col NVARCHAR(100) = @part;
        DECLARE @colon INT = CHARINDEX(':', @part);
        IF @colon > 0
        BEGIN
            SET @col = LTRIM(RTRIM(SUBSTRING(@part, 1, @colon - 1)));
            SET @dir = LTRIM(RTRIM(SUBSTRING(@part, @colon + 1, 10)));
        END
        ELSE SET @dir = N'asc';

        SET @col = CASE @col
            WHEN N'MedicamentID' THEN N'MedicamentID'
            WHEN N'Nume' THEN N'Nume'
            WHEN N'DenumireComunaInternationala' THEN N'DenumireComunaInternationala'
            WHEN N'Concentratie' THEN N'Concentratie'
            WHEN N'FormaFarmaceutica' THEN N'FormaFarmaceutica'
            WHEN N'Producator' THEN N'Producator'
            WHEN N'CodATC' THEN N'CodATC'
            WHEN N'Status' THEN N'Status'
            WHEN N'Stoc' THEN N'Stoc'
            WHEN N'StocSiguranta' THEN N'StocSiguranta'
            WHEN N'DataInregistrare' THEN N'DataInregistrare'
            WHEN N'DataExpirare' THEN N'DataExpirare'
            WHEN N'Pret' THEN N'Pret'
            WHEN N'PretProducator' THEN N'PretProducator'
            WHEN N'TVA' THEN N'TVA'
            ELSE NULL END;

        IF (@col IS NOT NULL)
        BEGIN
            SET @dir = CASE WHEN LOWER(@dir) = 'desc' THEN N'DESC' ELSE N'ASC' END;
            SET @OrderBy = CONCAT(@OrderBy, CASE WHEN LEN(@OrderBy)>0 THEN N', ' ELSE N'' END, @col, N' ', @dir);
        END
    END

    IF (LEN(@OrderBy) = 0) SET @OrderBy = N'MedicamentID ASC';

    DECLARE @sql NVARCHAR(MAX) = N'
SELECT MedicamentID, MedicamentGUID, Nume, DenumireComunaInternationala, Concentratie, FormaFarmaceutica, Producator, CodATC, Status, DataInregistrare, NumarAutorizatie, DataAutorizatie, DataExpirare, Ambalaj, Prospect, Contraindicatii, Interactiuni, Pret, PretProducator, TVA, Compensat, PrescriptieMedicala, Stoc, StocSiguranta, DataActualizare, UtilizatorActualizare, Observatii, Activ
FROM dbo.Medicament WITH (NOLOCK)
WHERE (@Search IS NULL OR @Search = '''' OR Nume LIKE ''%'' + @Search + ''%'' OR DenumireComunaInternationala LIKE ''%'' + @Search + ''%'' OR Producator LIKE ''%'' + @Search + ''%'' OR CodATC LIKE ''%'' + @Search + ''%'' )
  AND (@Status IS NULL OR @Status = ''toate'' OR @Status = '''' OR Status = @Status)
ORDER BY ' + @OrderBy + '
OPTION (RECOMPILE);';

    EXEC sp_executesql
        @sql,
        N'@Search NVARCHAR(200), @Status NVARCHAR(50)',
        @Search = @Search, @Status = @Status;
END
GO

PRINT 'Enhanced grouped procedures created successfully!';

-- Test the procedures
-- EXEC dbo.sp_MaterialeSanitare_GetAllGrouped @GroupBy = N'Categorie,Denumire', @Sort = N'DataCreare:desc';
-- EXEC dbo.sp_DispozitiveMedicale_GetAllGrouped @GroupBy = N'Producator,Categorie', @Sort = N'DataCreare:desc';
-- EXEC dbo.sp_Medicament_GetAllGrouped @GroupBy = N'Producator,FormaFarmaceutica', @Sort = N'DataInregistrare:desc';