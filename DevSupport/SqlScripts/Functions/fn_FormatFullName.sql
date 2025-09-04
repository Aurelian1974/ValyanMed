-- Func?ie pentru formatarea numelui complet
CREATE OR ALTER FUNCTION fn_FormatFullName(@FirstName NVARCHAR(100), @LastName NVARCHAR(100))
RETURNS NVARCHAR(201)
AS
BEGIN
    RETURN LTRIM(RTRIM(@LastName)) + ', ' + LTRIM(RTRIM(@FirstName));
END;