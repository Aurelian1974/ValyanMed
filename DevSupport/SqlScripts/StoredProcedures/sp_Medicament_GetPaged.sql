/*
    Stored Procedure: dbo.sp_Medicament_GetPaged
    Purpose: Server-side filtering, grouping (ordering by grouped columns), sorting and paging for Medicament table.
    Usage:
      EXEC dbo.sp_Medicament_GetPaged @Search = N'amoxi', @Status = N'Activ', @Page = 1, @PageSize = 25, @Sort = N'Producator:asc,Nume:desc', @GroupBy = N'Producator,FormaFarmaceutica'

    Notes:
      - GroupBy is implemented as an ORDER BY prefix with the specified columns (ASC). It does not aggregate; it orders so that grouped columns are contiguous.
      - @Sort allows multiple definitions separated by comma in the form Column[:asc|desc].
      - Column names are whitelisted to avoid SQL injection.
*/
GO
IF OBJECT_ID('dbo.sp_Medicament_GetPaged','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Medicament_GetPaged AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE dbo.sp_Medicament_GetPaged
    @Search   NVARCHAR(200) = NULL,
    @Status   NVARCHAR(50)  = NULL,
    @Page     INT           = 1,
    @PageSize INT           = 25,
    @Sort     NVARCHAR(400) = NULL,
    @GroupBy  NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF (@Page IS NULL OR @Page < 1) SET @Page = 1;
    IF (@PageSize IS NULL OR @PageSize < 1) SET @PageSize = 25;
    IF (@PageSize > 5000) SET @PageSize = 5000; -- safety cap

    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    -- Build ORDER BY from GroupBy + Sort (whitelisted)
    DECLARE @OrderBy NVARCHAR(MAX) = N'';

    -- helper to append a column token safely
    DECLARE @token NVARCHAR(100), @dir NVARCHAR(4);

    -- Append group-by columns (ASC)
    WHILE (@GroupBy IS NOT NULL AND LEN(@GroupBy) > 0)
    BEGIN
        DECLARE @pos INT = CHARINDEX(',', @GroupBy);
        IF @pos > 0
        BEGIN
            SET @token = LTRIM(RTRIM(SUBSTRING(@GroupBy, 1, @pos - 1)));
            SET @GroupBy = SUBSTRING(@GroupBy, @pos + 1, LEN(@GroupBy));
        END
        ELSE
        BEGIN
            SET @token = LTRIM(RTRIM(@GroupBy));
            SET @GroupBy = NULL;
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
            ELSE NULL END;

        IF (@token IS NOT NULL)
            SET @OrderBy = CONCAT(@OrderBy, CASE WHEN LEN(@OrderBy)>0 THEN N', ' ELSE N'' END, @token, N' ASC');
    END

    -- Append sort definitions
    WHILE (@Sort IS NOT NULL AND LEN(@Sort) > 0)
    BEGIN
        DECLARE @part NVARCHAR(100);
        DECLARE @pos2 INT = CHARINDEX(',', @Sort);
        IF @pos2 > 0
        BEGIN
            SET @part = LTRIM(RTRIM(SUBSTRING(@Sort, 1, @pos2 - 1)));
            SET @Sort = SUBSTRING(@Sort, @pos2 + 1, LEN(@Sort));
        END
        ELSE
        BEGIN
            SET @part = LTRIM(RTRIM(@Sort));
            SET @Sort = NULL;
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
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
OPTION (RECOMPILE);

SELECT COUNT(1)
FROM dbo.Medicament WITH (NOLOCK)
WHERE (@Search IS NULL OR @Search = '''' OR Nume LIKE ''%'' + @Search + ''%'' OR DenumireComunaInternationala LIKE ''%'' + @Search + ''%'' OR Producator LIKE ''%'' + @Search + ''%'' OR CodATC LIKE ''%'' + @Search + ''%'' )
  AND (@Status IS NULL OR @Status = ''toate'' OR @Status = '''' OR Status = @Status)
OPTION (RECOMPILE);';

    EXEC sp_executesql
        @sql,
        N'@Search NVARCHAR(200), @Status NVARCHAR(50), @Offset INT, @PageSize INT',
        @Search = @Search, @Status = @Status, @Offset = @Offset, @PageSize = @PageSize;
END
GO


EXEC dbo.sp_Medicament_GetPaged @Search = N'', @Status = N'toate', @Page = 1, @PageSize = 10;
