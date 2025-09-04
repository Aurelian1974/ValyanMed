-- Func?ii SQL pentru calcul?ri ?i valid?ri medicale
-- Data: 2025-01-04
-- Autor: DevSupport

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ==============================================
-- Func?ie pentru calcularea vârstei
-- ==============================================
CREATE OR ALTER FUNCTION [dbo].[fn_CalculeazaVarsta]
(
    @DataNasterii DATE
)
RETURNS INT
AS
BEGIN
    DECLARE @Varsta INT;
    
    SET @Varsta = DATEDIFF(YEAR, @DataNasterii, GETDATE()) - 
                  CASE 
                      WHEN MONTH(@DataNasterii) > MONTH(GETDATE()) 
                           OR (MONTH(@DataNasterii) = MONTH(GETDATE()) AND DAY(@DataNasterii) > DAY(GETDATE()))
                      THEN 1 
                      ELSE 0 
                  END;
    
    RETURN @Varsta;
END
GO

-- ==============================================
-- Func?ie pentru validarea CNP-ului
-- ==============================================
CREATE OR ALTER FUNCTION [dbo].[fn_ValidareCNP]
(
    @CNP VARCHAR(13)
)
RETURNS BIT
AS
BEGIN
    DECLARE @EstValid BIT = 0;
    
    -- Verific?m lungimea
    IF LEN(@CNP) != 13
        RETURN 0;
    
    -- Verific?m c? sunt doar cifre
    IF @CNP NOT LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
        RETURN 0;
    
    -- Verific?m prima cifr? (sexul ?i secolul)
    DECLARE @PrimaCifra INT = CAST(LEFT(@CNP, 1) AS INT);
    IF @PrimaCifra NOT IN (1, 2, 3, 4, 5, 6, 7, 8, 9)
        RETURN 0;
    
    -- Calcul?m cifra de control
    DECLARE @CifreControl VARCHAR(12) = '279146358279';
    DECLARE @Suma INT = 0;
    DECLARE @i INT = 1;
    
    WHILE @i <= 12
    BEGIN
        SET @Suma = @Suma + (CAST(SUBSTRING(@CNP, @i, 1) AS INT) * CAST(SUBSTRING(@CifreControl, @i, 1) AS INT));
        SET @i = @i + 1;
    END
    
    DECLARE @Rest INT = @Suma % 11;
    DECLARE @CifraControlCalculata INT = CASE WHEN @Rest < 10 THEN @Rest ELSE 1 END;
    DECLARE @CifraControlCNP INT = CAST(RIGHT(@CNP, 1) AS INT);
    
    IF @CifraControlCalculata = @CifraControlCNP
        SET @EstValid = 1;
    
    RETURN @EstValid;
END
GO

-- ==============================================
-- Func?ie pentru calcularea IMC (Indicele de Mas? Corporal?)
-- ==============================================
CREATE OR ALTER FUNCTION [dbo].[fn_CalculeazaIMC]
(
    @Greutate DECIMAL(5,2), -- în kg
    @Inaltime DECIMAL(5,2)  -- în metri
)
RETURNS DECIMAL(5,2)
AS
BEGIN
    DECLARE @IMC DECIMAL(5,2);
    
    IF @Inaltime IS NULL OR @Inaltime <= 0 OR @Greutate IS NULL OR @Greutate <= 0
        RETURN NULL;
    
    SET @IMC = @Greutate / (@Inaltime * @Inaltime);
    
    RETURN @IMC;
END
GO

-- ==============================================
-- Func?ie pentru interpretarea IMC
-- ==============================================
CREATE OR ALTER FUNCTION [dbo].[fn_InterpretareIMC]
(
    @IMC DECIMAL(5,2)
)
RETURNS NVARCHAR(50)
AS
BEGIN
    DECLARE @Interpretare NVARCHAR(50);
    
    IF @IMC IS NULL
        SET @Interpretare = 'Date insuficiente';
    ELSE IF @IMC < 18.5
        SET @Interpretare = 'Subponderal';
    ELSE IF @IMC < 25
        SET @Interpretare = 'Normal';
    ELSE IF @IMC < 30
        SET @Interpretare = 'Supraponderal';
    ELSE IF @IMC < 35
        SET @Interpretare = 'Obezitate grad I';
    ELSE IF @IMC < 40
        SET @Interpretare = 'Obezitate grad II';
    ELSE
        SET @Interpretare = 'Obezitate grad III (morbid?)';
    
    RETURN @Interpretare;
END
GO

-- ==============================================
-- Func?ie pentru calcularea dozajului în func?ie de greutate
-- ==============================================
CREATE OR ALTER FUNCTION [dbo].[fn_CalculeazaDozaj]
(
    @DozaPeKg DECIMAL(10,4), -- mg/kg
    @Greutate DECIMAL(5,2),  -- kg
    @DozaMaxima DECIMAL(10,2) = NULL -- mg (op?ional)
)
RETURNS DECIMAL(10,2)
AS
BEGIN
    DECLARE @Dozaj DECIMAL(10,2);
    
    IF @DozaPeKg IS NULL OR @DozaPeKg <= 0 OR @Greutate IS NULL OR @Greutate <= 0
        RETURN NULL;
    
    SET @Dozaj = @DozaPeKg * @Greutate;
    
    -- Aplic?m doza maxim? dac? este specificat?
    IF @DozaMaxima IS NOT NULL AND @Dozaj > @DozaMaxima
        SET @Dozaj = @DozaMaxima;
    
    RETURN @Dozaj;
END
GO

-- ==============================================
-- Func?ie pentru formatarea numelui complet
-- ==============================================
CREATE OR ALTER FUNCTION [dbo].[fn_NumeComplet]
(
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @Titlu NVARCHAR(50) = NULL
)
RETURNS NVARCHAR(300)
AS
BEGIN
    DECLARE @NumeComplet NVARCHAR(300);
    
    SET @NumeComplet = LTRIM(RTRIM(ISNULL(@Nume, '') + ' ' + ISNULL(@Prenume, '')));
    
    IF @Titlu IS NOT NULL AND LEN(@Titlu) > 0
        SET @NumeComplet = @Titlu + ' ' + @NumeComplet;
    
    RETURN @NumeComplet;
END
GO

-- ==============================================
-- Func?ie pentru calcularea perioadei de valabilitate a medicamentelor
-- ==============================================
CREATE OR ALTER FUNCTION [dbo].[fn_StatusValiditateStoc]
(
    @DataExpirare DATE,
    @Stoc INT,
    @StocMinim INT = 0
)
RETURNS NVARCHAR(50)
AS
BEGIN
    DECLARE @Status NVARCHAR(50);
    
    -- Verific?m mai întâi stocul
    IF @Stoc <= 0
        SET @Status = 'EPUIZAT';
    ELSE IF @Stoc <= @StocMinim
        SET @Status = 'STOC SC?ZUT';
    -- Verific?m expirarea
    ELSE IF @DataExpirare <= GETDATE()
        SET @Status = 'EXPIRAT';
    ELSE IF @DataExpirare <= DATEADD(MONTH, 1, GETDATE())
        SET @Status = 'EXPIR? CURÂND';
    ELSE IF @DataExpirare <= DATEADD(MONTH, 6, GETDATE())
        SET @Status = 'MONITORIZARE';
    ELSE
        SET @Status = 'DISPONIBIL';
    
    RETURN @Status;
END
GO

-- ==============================================
-- Func?ie pentru calcularea diferen?ei de zile lucr?toare
-- ==============================================
CREATE OR ALTER FUNCTION [dbo].[fn_ZileLucratoare]
(
    @DataStart DATE,
    @DataEnd DATE
)
RETURNS INT
AS
BEGIN
    DECLARE @ZileLucratoare INT = 0;
    DECLARE @DataCurenta DATE = @DataStart;
    
    WHILE @DataCurenta <= @DataEnd
    BEGIN
        -- Luni=2, Mar?i=3, Miercuri=4, Joi=5, Vineri=6
        -- Sâmb?t?=7, Duminic?=1
        IF DATEPART(WEEKDAY, @DataCurenta) BETWEEN 2 AND 6
            SET @ZileLucratoare = @ZileLucratoare + 1;
        
        SET @DataCurenta = DATEADD(DAY, 1, @DataCurenta);
    END
    
    RETURN @ZileLucratoare;
END
GO

-- ==============================================
-- Func?ie pentru extragerea grupei de vârst?
-- ==============================================
CREATE OR ALTER FUNCTION [dbo].[fn_GrupaVarsta]
(
    @DataNasterii DATE
)
RETURNS NVARCHAR(20)
AS
BEGIN
    DECLARE @Varsta INT = dbo.fn_CalculeazaVarsta(@DataNasterii);
    DECLARE @Grupa NVARCHAR(20);
    
    IF @Varsta IS NULL
        SET @Grupa = 'Nedefinit';
    ELSE IF @Varsta < 1
        SET @Grupa = 'Sugar';
    ELSE IF @Varsta < 3
        SET @Grupa = 'Copil mic';
    ELSE IF @Varsta < 12
        SET @Grupa = 'Copil';
    ELSE IF @Varsta < 18
        SET @Grupa = 'Adolescent';
    ELSE IF @Varsta < 65
        SET @Grupa = 'Adult';
    ELSE
        SET @Grupa = 'Senior';
    
    RETURN @Grupa;
END
GO

-- ==============================================
-- Func?ie pentru validarea adresei de email
-- ==============================================
CREATE OR ALTER FUNCTION [dbo].[fn_ValidareEmail]
(
    @Email NVARCHAR(150)
)
RETURNS BIT
AS
BEGIN
    DECLARE @EstValid BIT = 0;
    
    IF @Email IS NOT NULL 
       AND LEN(@Email) > 5
       AND @Email LIKE '%_@_%.__%'
       AND @Email NOT LIKE '%@%@%'
       AND @Email NOT LIKE '%.@%'
       AND @Email NOT LIKE '%@.%'
        SET @EstValid = 1;
    
    RETURN @EstValid;
END
GO

PRINT 'Func?iile SQL pentru calcul?ri medicale au fost create cu succes!'