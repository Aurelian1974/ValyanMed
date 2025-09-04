/*
    Stored Procedure: dbo.sp_Partener_Create
    Purpose: Insert a new partner
    Usage: EXEC dbo.sp_Partener_Create @CodIntern = 'P001', @Denumire = 'Test Partner', @CodFiscal = '12345678', @Judet = 'Bucuresti', @Localitate = 'Bucuresti', @Adresa = 'Str. Test', @UtilizatorCreare = 'admin'
    Returns: The new PartenerId
*/
GO
IF OBJECT_ID('dbo.sp_Partener_Create','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Partener_Create AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE dbo.sp_Partener_Create
    @CodIntern            NVARCHAR(50),
    @Denumire             NVARCHAR(200),
    @CodFiscal            NVARCHAR(50) = NULL,
    @Judet                NVARCHAR(100) = NULL,
    @Localitate           NVARCHAR(100) = NULL,
    @Adresa               NVARCHAR(500) = NULL,
    @UtilizatorCreare     NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Validate required fields
        IF @CodIntern IS NULL OR LEN(LTRIM(RTRIM(@CodIntern))) = 0
        BEGIN
            RAISERROR('CodIntern este obligatoriu.', 16, 1);
            RETURN;
        END
        
        IF @Denumire IS NULL OR LEN(LTRIM(RTRIM(@Denumire))) = 0
        BEGIN
            RAISERROR('Denumire este obligatorie.', 16, 1);
            RETURN;
        END
        
        -- Check if CodIntern already exists
        IF EXISTS (SELECT 1 FROM dbo.Partener WHERE CodIntern = @CodIntern)
        BEGIN
            RAISERROR('Un partener cu acest cod intern deja exist?.', 16, 1);
            RETURN;
        END
        
        -- Insert new partner
        INSERT INTO dbo.Partener (
            CodIntern,
            Denumire,
            CodFiscal,
            Judet,
            Localitate,
            Adresa,
            UtilizatorCreare,
            UtilizatorActualizare
        )
        VALUES (
            @CodIntern,
            @Denumire,
            @CodFiscal,
            @Judet,
            @Localitate,
            @Adresa,
            @UtilizatorCreare,
            @UtilizatorCreare
        );
        
        -- Return the new ID
        SELECT SCOPE_IDENTITY() AS PartenerId;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO