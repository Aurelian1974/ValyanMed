-- Script SIMPLIFICAT pentru corectarea sp_PersonalMedical_GetPaged
-- Doar corectarea procedurilor existente, f?r? crearea unora noi

USE [ValyanMed]
GO

-- =====================================================
-- Corectare sp_PersonalMedical_GetPaged
-- Problema: duplicate în ORDER BY ?i mappingul gre?it
-- =====================================================
ALTER PROCEDURE [dbo].[sp_PersonalMedical_GetPaged]
    @Search NVARCHAR(255) = NULL,
    @CategorieID UNIQUEIDENTIFIER = NULL,
    @SpecializareID UNIQUEIDENTIFIER = NULL,
    @SubspecializareID UNIQUEIDENTIFIER = NULL,
    @Departament NVARCHAR(100) = NULL,
    @Pozitie NVARCHAR(50) = NULL,
    @EsteActiv BIT = NULL,
    @Nume NVARCHAR(100) = NULL,
    @Prenume NVARCHAR(100) = NULL,
    @Specializare NVARCHAR(150) = NULL,
    @NumarLicenta NVARCHAR(50) = NULL,
    @Telefon NVARCHAR(30) = NULL,
    @Email NVARCHAR(150) = NULL,
    @Page INT = 1,
    @PageSize INT = 25,
    @Sort NVARCHAR(400) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF (@Page IS NULL OR @Page < 1) SET @Page = 1;
    IF (@PageSize IS NULL OR @PageSize < 1) SET @PageSize = 25;
    IF (@PageSize > 100000) SET @PageSize = 100000;

    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    -- Parse Sort - FIX pentru duplicate în ORDER BY
    DECLARE @OrderCol NVARCHAR(200) = N'pm.Nume';
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
        
        -- MAPPINGUL CORECT - evit?m duplicate ?i folosim LOWER pentru case-insensitive
        SET @col = CASE LOWER(@col)
            WHEN N'nume' THEN N'pm.Nume'
            WHEN N'prenume' THEN N'pm.Prenume'
            WHEN N'specializare' THEN N'ISNULL(spec.Nume, pm.Specializare)'
            WHEN N'specializarenume' THEN N'spec.Nume'
            WHEN N'numarlicenta' THEN N'pm.NumarLicenta'
            WHEN N'departament' THEN N'ISNULL(cat.Nume, pm.Departament)'
            WHEN N'categorienume' THEN N'cat.Nume'
            WHEN N'pozitie' THEN N'pm.Pozitie'
            WHEN N'telefon' THEN N'pm.Telefon'
            WHEN N'email' THEN N'pm.Email'
            WHEN N'esteactiv' THEN N'pm.EsteActiv'
            WHEN N'datacreare' THEN N'pm.DataCreare'
            ELSE N'pm.Nume' END;
        
        SET @OrderCol = @col;
        SET @OrderDir = CASE WHEN LOWER(@dir) = 'desc' THEN N'DESC' ELSE N'ASC' END;
    END

    -- Main query cu JOIN-uri pentru datele ierarhice
    -- ELIMINAT duplicate din ORDER BY
    SELECT 
        pm.PersonalID,
        pm.Nume,
        pm.Prenume,
        ISNULL(spec.Nume, pm.Specializare) AS Specializare,
        pm.NumarLicenta,
        pm.Telefon,
        pm.Email,
        ISNULL(cat.Nume, pm.Departament) AS Departament,
        pm.Pozitie,
        pm.EsteActiv,
        pm.DataCreare,
        pm.CategorieID,
        pm.SpecializareID,
        pm.SubspecializareID,
        cat.Nume AS CategorieNume,
        spec.Nume AS SpecializareNume,
        sub.Nume AS SubspecializareNume
    FROM PersonalMedical pm WITH (NOLOCK)
    LEFT JOIN Departamente cat ON pm.CategorieID = cat.DepartamentID AND cat.Tip = 'Categorie'
    LEFT JOIN Departamente spec ON pm.SpecializareID = spec.DepartamentID AND spec.Tip = 'Specialitate'
    LEFT JOIN Departamente sub ON pm.SubspecializareID = sub.DepartamentID AND sub.Tip = 'Subspecialitate'
    WHERE 1 = 1
      AND (@EsteActiv IS NULL OR pm.EsteActiv = @EsteActiv)
      AND (@CategorieID IS NULL OR pm.CategorieID = @CategorieID)
      AND (@SpecializareID IS NULL OR pm.SpecializareID = @SpecializareID)
      AND (@SubspecializareID IS NULL OR pm.SubspecializareID = @SubspecializareID)
      -- Compatibilitate cu c?ut?rile pe textul vechi
      AND (@Departament IS NULL OR @Departament = '' OR @Departament = 'toate' 
           OR UPPER(ISNULL(cat.Nume, pm.Departament)) LIKE '%' + UPPER(@Departament) + '%')
      AND (@Pozitie IS NULL OR @Pozitie = '' OR @Pozitie = 'toate' OR UPPER(pm.Pozitie) LIKE '%' + UPPER(@Pozitie) + '%')
      AND (@Nume IS NULL OR @Nume = '' OR UPPER(pm.Nume) LIKE '%' + UPPER(@Nume) + '%')
      AND (@Prenume IS NULL OR @Prenume = '' OR UPPER(pm.Prenume) LIKE '%' + UPPER(@Prenume) + '%')
      AND (@Specializare IS NULL OR @Specializare = '' 
           OR UPPER(ISNULL(spec.Nume, pm.Specializare)) LIKE '%' + UPPER(@Specializare) + '%')
      AND (@NumarLicenta IS NULL OR @NumarLicenta = '' OR UPPER(pm.NumarLicenta) LIKE '%' + UPPER(@NumarLicenta) + '%')
      AND (@Telefon IS NULL OR @Telefon = '' OR pm.Telefon LIKE '%' + @Telefon + '%')
      AND (@Email IS NULL OR @Email = '' OR UPPER(pm.Email) LIKE '%' + UPPER(@Email) + '%')
      AND (
            @Search IS NULL OR @Search = ''
            OR UPPER(pm.Nume) LIKE '%' + UPPER(@Search) + '%'
            OR UPPER(pm.Prenume) LIKE '%' + UPPER(@Search) + '%'
            OR UPPER(CONCAT(pm.Nume, ' ', pm.Prenume)) LIKE '%' + UPPER(@Search) + '%'
            OR (ISNULL(spec.Nume, pm.Specializare) IS NOT NULL AND UPPER(ISNULL(spec.Nume, pm.Specializare)) LIKE '%' + UPPER(@Search) + '%')
            OR (pm.NumarLicenta IS NOT NULL AND UPPER(pm.NumarLicenta) LIKE '%' + UPPER(@Search) + '%')
            OR (ISNULL(cat.Nume, pm.Departament) IS NOT NULL AND UPPER(ISNULL(cat.Nume, pm.Departament)) LIKE '%' + UPPER(@Search) + '%')
            OR (pm.Pozitie IS NOT NULL AND UPPER(pm.Pozitie) LIKE '%' + UPPER(@Search) + '%')
            OR (pm.Telefon IS NOT NULL AND pm.Telefon LIKE '%' + @Search + '%')
            OR (pm.Email IS NOT NULL AND UPPER(pm.Email) LIKE '%' + UPPER(@Search) + '%')
          )
    ORDER BY 
        CASE 
            WHEN @OrderCol = 'pm.Nume' AND @OrderDir = 'ASC' THEN pm.Nume
        END ASC,
        CASE 
            WHEN @OrderCol = 'pm.Nume' AND @OrderDir = 'DESC' THEN pm.Nume
        END DESC,
        CASE 
            WHEN @OrderCol = 'pm.Prenume' AND @OrderDir = 'ASC' THEN pm.Prenume
        END ASC,
        CASE 
            WHEN @OrderCol = 'pm.Prenume' AND @OrderDir = 'DESC' THEN pm.Prenume
        END DESC,
        CASE 
            WHEN @OrderCol = 'ISNULL(spec.Nume, pm.Specializare)' AND @OrderDir = 'ASC' THEN ISNULL(spec.Nume, pm.Specializare)
        END ASC,
        CASE 
            WHEN @OrderCol = 'ISNULL(spec.Nume, pm.Specializare)' AND @OrderDir = 'DESC' THEN ISNULL(spec.Nume, pm.Specializare)
        END DESC,
        CASE 
            WHEN @OrderCol = 'pm.Pozitie' AND @OrderDir = 'ASC' THEN pm.Pozitie
        END ASC,
        CASE 
            WHEN @OrderCol = 'pm.Pozitie' AND @OrderDir = 'DESC' THEN pm.Pozitie
        END DESC,
        CASE 
            WHEN @OrderCol = 'pm.EsteActiv' AND @OrderDir = 'ASC' THEN pm.EsteActiv
        END ASC,
        CASE 
            WHEN @OrderCol = 'pm.EsteActiv' AND @OrderDir = 'DESC' THEN pm.EsteActiv
        END DESC,
        CASE 
            WHEN @OrderCol = 'pm.DataCreare' AND @OrderDir = 'ASC' THEN pm.DataCreare
        END ASC,
        CASE 
            WHEN @OrderCol = 'pm.DataCreare' AND @OrderDir = 'DESC' THEN pm.DataCreare
        END DESC,
        pm.Nume, pm.Prenume -- Fallback sorting
    OFFSET @Offset ROWS 
    FETCH NEXT @PageSize ROWS ONLY;

    -- Count query cu acelea?i filtre
    SELECT COUNT(1)
    FROM PersonalMedical pm WITH (NOLOCK)
    LEFT JOIN Departamente cat ON pm.CategorieID = cat.DepartamentID AND cat.Tip = 'Categorie'
    LEFT JOIN Departamente spec ON pm.SpecializareID = spec.DepartamentID AND spec.Tip = 'Specialitate'
    LEFT JOIN Departamente sub ON pm.SubspecializareID = sub.DepartamentID AND sub.Tip = 'Subspecialitate'
    WHERE 1 = 1
      AND (@EsteActiv IS NULL OR pm.EsteActiv = @EsteActiv)
      AND (@CategorieID IS NULL OR pm.CategorieID = @CategorieID)
      AND (@SpecializareID IS NULL OR pm.SpecializareID = @SpecializareID)
      AND (@SubspecializareID IS NULL OR pm.SubspecializareID = @SubspecializareID)
      AND (@Departament IS NULL OR @Departament = '' OR @Departament = 'toate' 
           OR UPPER(ISNULL(cat.Nume, pm.Departament)) LIKE '%' + UPPER(@Departament) + '%')
      AND (@Pozitie IS NULL OR @Pozitie = '' OR @Pozitie = 'toate' OR UPPER(pm.Pozitie) LIKE '%' + UPPER(@Pozitie) + '%')
      AND (@Nume IS NULL OR @Nume = '' OR UPPER(pm.Nume) LIKE '%' + UPPER(@Nume) + '%')
      AND (@Prenume IS NULL OR @Prenume = '' OR UPPER(pm.Prenume) LIKE '%' + UPPER(@Prenume) + '%')
      AND (@Specializare IS NULL OR @Specializare = '' 
           OR UPPER(ISNULL(spec.Nume, pm.Specializare)) LIKE '%' + UPPER(@Specializare) + '%')
      AND (@NumarLicenta IS NULL OR @NumarLicenta = '' OR UPPER(pm.NumarLicenta) LIKE '%' + UPPER(@NumarLicenta) + '%')
      AND (@Telefon IS NULL OR @Telefon = '' OR pm.Telefon LIKE '%' + @Telefon + '%')
      AND (@Email IS NULL OR @Email = '' OR UPPER(pm.Email) LIKE '%' + UPPER(@Email) + '%')
      AND (
            @Search IS NULL OR @Search = ''
            OR UPPER(pm.Nume) LIKE '%' + UPPER(@Search) + '%'
            OR UPPER(pm.Prenume) LIKE '%' + UPPER(@Search) + '%'
            OR UPPER(CONCAT(pm.Nume, ' ', pm.Prenume)) LIKE '%' + UPPER(@Search) + '%'
            OR (ISNULL(spec.Nume, pm.Specializare) IS NOT NULL AND UPPER(ISNULL(spec.Nume, pm.Specializare)) LIKE '%' + UPPER(@Search) + '%')
            OR (pm.NumarLicenta IS NOT NULL AND UPPER(pm.NumarLicenta) LIKE '%' + UPPER(@Search) + '%')
            OR (ISNULL(cat.Nume, pm.Departament) IS NOT NULL AND UPPER(ISNULL(cat.Nume, pm.Departament)) LIKE '%' + UPPER(@Search) + '%')
            OR (pm.Pozitie IS NOT NULL AND UPPER(pm.Pozitie) LIKE '%' + UPPER(@Search) + '%')
            OR (pm.Telefon IS NOT NULL AND pm.Telefon LIKE '%' + @Search + '%')
            OR (pm.Email IS NOT NULL AND UPPER(pm.Email) LIKE '%' + UPPER(@Search) + '%')
          );
END
GO

PRINT 'sp_PersonalMedical_GetPaged a fost corectat cu succes!'