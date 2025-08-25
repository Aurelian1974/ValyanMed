/*
    Stored Procedure: dbo.sp_Pacienti_GetPaged
    Purpose: Server-side filtering, sorting and paging for Pacienti table.
    Usage:
      EXEC dbo.sp_Pacienti_GetPaged @Search = N'Popescu', @Judet = N'Bucuresti', @Gen = N'Masculin', @Page = 1, @PageSize = 25, @Sort = N'NumeComplet:asc'

    Notes:
      - Column names are whitelisted to avoid SQL injection.
      - Search is performed across multiple columns (Nume, Prenume, CNP, Email)
*/
GO
IF OBJECT_ID('dbo.sp_Pacienti_GetPaged','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Pacienti_GetPaged AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE dbo.sp_Pacienti_GetPaged
    @Search   NVARCHAR(255) = NULL,
    @Judet    NVARCHAR(100) = NULL,
    @Gen      NVARCHAR(50)  = NULL,
    @Page     INT           = 1,
    @PageSize INT           = 25,
    @Sort     NVARCHAR(400) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF (@Page IS NULL OR @Page < 1) SET @Page = 1;
    IF (@PageSize IS NULL OR @PageSize < 1) SET @PageSize = 25;
    IF (@PageSize > 1000) SET @PageSize = 1000; -- safety cap

    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    -- Build ORDER BY from Sort parameter (whitelisted)
    DECLARE @OrderBy NVARCHAR(MAX) = N'p.Nume ASC, p.Prenume ASC';

    IF (@Sort IS NOT NULL AND LEN(@Sort) > 0)
    BEGIN
        DECLARE @part NVARCHAR(100);
        DECLARE @pos INT = CHARINDEX(':', @Sort);
        
        IF @pos > 0
        BEGIN
            DECLARE @col NVARCHAR(100) = LTRIM(RTRIM(SUBSTRING(@Sort, 1, @pos - 1)));
            DECLARE @dir NVARCHAR(4) = LTRIM(RTRIM(SUBSTRING(@Sort, @pos + 1, 10)));
            
            SET @col = CASE @col
                WHEN N'NumeComplet' THEN N'p.Nume, p.Prenume'
                WHEN N'CNP' THEN N'p.CNP'
                WHEN N'DataNasterii' THEN N'p.DataNasterii'
                WHEN N'Gen' THEN N'p.Gen'
                WHEN N'Email' THEN N'p.Email'
                WHEN N'DataCreare' THEN N'p.DataCreare'
                ELSE N'p.Nume, p.Prenume' END;

            SET @dir = CASE WHEN LOWER(@dir) = 'desc' THEN N'DESC' ELSE N'ASC' END;
            SET @OrderBy = @col + N' ' + @dir;
        END
    END

    DECLARE @sql NVARCHAR(MAX) = N'
WITH PacientData AS (
    SELECT 
        p.PacientID,
        p.Nume,
        p.Prenume,
        CONCAT(p.Nume, '' '', p.Prenume) AS NumeComplet,
        p.CNP,
        p.DataNasterii,
        p.Gen,
        p.Telefon,
        p.Email,
        p.Oras,
        p.Judet,
        p.DataCreare,
        CASE 
            WHEN p.DataNasterii IS NOT NULL 
            THEN DATEDIFF(YEAR, p.DataNasterii, GETDATE()) - 
                 CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, p.DataNasterii, GETDATE()), p.DataNasterii) > GETDATE() 
                      THEN 1 ELSE 0 END
            ELSE 0 
        END AS Varsta
    FROM dbo.Pacienti p
    WHERE p.EsteActiv = 1
      AND (@Search IS NULL OR @Search = '''' OR 
           UPPER(p.Nume) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
           UPPER(p.Prenume) LIKE ''%'' + UPPER(@Search) + ''%'' OR
           UPPER(CONCAT(p.Nume, '' '', p.Prenume)) LIKE ''%'' + UPPER(@Search) + ''%'' OR
           (p.CNP IS NOT NULL AND p.CNP LIKE ''%'' + @Search + ''%'') OR
           (p.Email IS NOT NULL AND UPPER(p.Email) LIKE ''%'' + UPPER(@Search) + ''%''))
      AND (@Judet IS NULL OR @Judet = '''' OR @Judet = ''toate'' OR p.Judet = @Judet)
      AND (@Gen IS NULL OR @Gen = '''' OR @Gen = ''toate'' OR p.Gen = @Gen)
)
SELECT *
FROM PacientData p
ORDER BY ' + @OrderBy + '
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

-- Get total count
WITH PacientData AS (
    SELECT p.PacientID
    FROM dbo.Pacienti p
    WHERE p.EsteActiv = 1
      AND (@Search IS NULL OR @Search = '''' OR 
           UPPER(p.Nume) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
           UPPER(p.Prenume) LIKE ''%'' + UPPER(@Search) + ''%'' OR
           UPPER(CONCAT(p.Nume, '' '', p.Prenume)) LIKE ''%'' + UPPER(@Search) + ''%'' OR
           (p.CNP IS NOT NULL AND p.CNP LIKE ''%'' + @Search + ''%'') OR
           (p.Email IS NOT NULL AND UPPER(p.Email) LIKE ''%'' + UPPER(@Search) + ''%''))
      AND (@Judet IS NULL OR @Judet = '''' OR @Judet = ''toate'' OR p.Judet = @Judet)
      AND (@Gen IS NULL OR @Gen = '''' OR @Gen = ''toate'' OR p.Gen = @Gen)
)
SELECT COUNT(1) AS TotalCount FROM PacientData;';

    EXEC sp_executesql
        @sql,
        N'@Search NVARCHAR(255), @Judet NVARCHAR(100), @Gen NVARCHAR(50), @Offset INT, @PageSize INT',
        @Search = @Search, @Judet = @Judet, @Gen = @Gen, @Offset = @Offset, @PageSize = @PageSize;
END
GO

PRINT 'Pacienti stored procedure created successfully!';

-- Test the procedure
-- EXEC dbo.sp_Pacienti_GetPaged @Search = N'', @Page = 1, @PageSize = 10;