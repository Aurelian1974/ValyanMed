-- Procedur? pentru c?utarea pacien?ilor cu filtrare avansat?
-- Permite c?utare dup? nume, CNP, telefon, email
CREATE OR ALTER PROCEDURE sp_SearchPatients
    @SearchTerm NVARCHAR(255) = NULL,
    @IsActive BIT = NULL,
    @AgeFrom INT = NULL,
    @AgeTo INT = NULL,
    @Gender NVARCHAR(10) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    WITH FilteredPatients AS (
        SELECT 
            Id,
            CNP,
            FirstName,
            LastName,
            DateOfBirth,
            Gender,
            Phone,
            Email,
            Address,
            CreatedAt,
            UpdatedAt,
            DATEDIFF(YEAR, DateOfBirth, GETDATE()) AS Age
        FROM Patients
        WHERE 
            (@SearchTerm IS NULL OR 
             FirstName LIKE '%' + @SearchTerm + '%' OR
             LastName LIKE '%' + @SearchTerm + '%' OR
             CNP LIKE '%' + @SearchTerm + '%' OR
             Phone LIKE '%' + @SearchTerm + '%' OR
             Email LIKE '%' + @SearchTerm + '%')
            AND (@Gender IS NULL OR Gender = @Gender)
    )
    SELECT *
    FROM FilteredPatients
    WHERE 
        (@AgeFrom IS NULL OR Age >= @AgeFrom)
        AND (@AgeTo IS NULL OR Age <= @AgeTo)
    ORDER BY LastName, FirstName
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Returneaz? ?i num?rul total de înregistr?ri
    SELECT COUNT(*) AS TotalCount
    FROM FilteredPatients
    WHERE 
        (@AgeFrom IS NULL OR Age >= @AgeFrom)
        AND (@AgeTo IS NULL OR Age <= @AgeTo);
END;