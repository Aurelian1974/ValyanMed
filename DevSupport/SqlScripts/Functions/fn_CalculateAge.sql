-- Func?ie pentru calcularea vârstei precise
CREATE OR ALTER FUNCTION fn_CalculateAge(@DateOfBirth DATE, @ReferenceDate DATE = NULL)
RETURNS INT
AS
BEGIN
    IF @ReferenceDate IS NULL
        SET @ReferenceDate = GETDATE();
    
    DECLARE @Age INT;
    
    SET @Age = DATEDIFF(YEAR, @DateOfBirth, @ReferenceDate);
    
    -- Ajustare dac? ziua de na?tere nu a trecut înc? în anul curent
    IF (MONTH(@DateOfBirth) > MONTH(@ReferenceDate)) OR
       (MONTH(@DateOfBirth) = MONTH(@ReferenceDate) AND DAY(@DateOfBirth) > DAY(@ReferenceDate))
        SET @Age = @Age - 1;
    
    RETURN @Age;
END;