/*
    Stored Procedure: dbo.sp_Partener_GetPaged
    Purpose: Server-side filtering, sorting and paging for Partener table
    Usage: EXEC dbo.sp_Partener_GetPaged @Search = N'test', @Judet = N'Bucuresti', @Page = 1, @PageSize = 25, @Sort = N'Denumire:asc'
*/
GO
IF OBJECT_ID('dbo.sp_Partener_GetPaged','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Partener_GetPaged AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE dbo.sp_Partener_GetPaged
    @Search   NVARCHAR(200) = NULL,
    @Judet    NVARCHAR(100) = NULL,
    @Page     INT           = 1,
    @PageSize INT           = 25,
    @Sort     NVARCHAR(400) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF (@Page IS NULL OR @Page < 1) SET @Page = 1;
    IF (@PageSize IS NULL OR @PageSize < 1) SET @PageSize = 25;
    IF (@PageSize > 5000) SET @PageSize = 5000; -- safety cap

    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    -- Build ORDER BY from Sort parameter (whitelisted columns)
    DECLARE @OrderBy NVARCHAR(MAX) = N'';

    IF (@Sort IS NOT NULL AND LEN(@Sort) > 0)
    BEGIN
        DECLARE @token NVARCHAR(100), @dir NVARCHAR(4);
        
        WHILE (@Sort IS NOT NULL AND LEN(@Sort) > 0)
        BEGIN
            DECLARE @pos INT = CHARINDEX(',', @Sort);
            DECLARE @part NVARCHAR(100);
            
            IF @pos > 0
            BEGIN
                SET @part = LTRIM(RTRIM(SUBSTRING(@Sort, 1, @pos - 1)));
                SET @Sort = SUBSTRING(@Sort, @pos + 1, LEN(@Sort));
            END
            ELSE
            BEGIN
                SET @part = LTRIM(RTRIM(@Sort));
                SET @Sort = NULL;
            END

            -- Split part as col[:dir]
            DECLARE @col NVARCHAR(100) = @part;
            DECLARE @colon INT = CHARINDEX(':', @part);
            IF @colon > 0
            BEGIN
                SET @col = LTRIM(RTRIM(SUBSTRING(@part, 1, @colon - 1)));
                SET @dir = LTRIM(RTRIM(SUBSTRING(@part, @colon + 1, 10)));
            END
            ELSE SET @dir = N'asc';

            -- Whitelist columns to prevent SQL injection
            SET @col = CASE @col
                WHEN N'PartenerId' THEN N'PartenerId'
                WHEN N'CodIntern' THEN N'CodIntern'
                WHEN N'Denumire' THEN N'Denumire'
                WHEN N'CodFiscal' THEN N'CodFiscal'
                WHEN N'Judet' THEN N'Judet'
                WHEN N'Localitate' THEN N'Localitate'
                WHEN N'DataCreare' THEN N'DataCreare'
                WHEN N'DataActualizare' THEN N'DataActualizare'
                ELSE NULL END;

            IF (@col IS NOT NULL)
            BEGIN
                SET @dir = CASE WHEN LOWER(@dir) = 'desc' THEN N'DESC' ELSE N'ASC' END;
                SET @OrderBy = CONCAT(@OrderBy, CASE WHEN LEN(@OrderBy)>0 THEN N', ' ELSE N'' END, @col, N' ', @dir);
            END
        END
    END

    IF (LEN(@OrderBy) = 0) SET @OrderBy = N'Denumire ASC';

    DECLARE @sql NVARCHAR(MAX) = N'
SELECT PartenerId, PartenerGuid, CodIntern, Denumire, CodFiscal, Judet, Localitate, Adresa, DataCreare, DataActualizare, UtilizatorCreare, UtilizatorActualizare, Activ
FROM dbo.Partener WITH (NOLOCK)
WHERE Activ = 1
  AND (@Search IS NULL OR @Search = '''' OR 
       UPPER(CodIntern) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
       UPPER(Denumire) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
       UPPER(CodFiscal) LIKE ''%'' + UPPER(@Search) + ''%'' OR
       UPPER(Localitate) LIKE ''%'' + UPPER(@Search) + ''%'')
  AND (@Judet IS NULL OR @Judet = '''' OR @Judet = ''toate'' OR Judet = @Judet)
ORDER BY ' + @OrderBy + '
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
OPTION (RECOMPILE);

SELECT COUNT(1)
FROM dbo.Partener WITH (NOLOCK)
WHERE Activ = 1
  AND (@Search IS NULL OR @Search = '''' OR 
       UPPER(CodIntern) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
       UPPER(Denumire) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
       UPPER(CodFiscal) LIKE ''%'' + UPPER(@Search) + ''%'' OR
       UPPER(Localitate) LIKE ''%'' + UPPER(@Search) + ''%'')
  AND (@Judet IS NULL OR @Judet = '''' OR @Judet = ''toate'' OR Judet = @Judet)
OPTION (RECOMPILE);';

    EXEC sp_executesql
        @sql,
        N'@Search NVARCHAR(200), @Judet NVARCHAR(100), @Offset INT, @PageSize INT',
        @Search = @Search, @Judet = @Judet, @Offset = @Offset, @PageSize = @PageSize;
END
GO