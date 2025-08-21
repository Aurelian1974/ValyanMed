/*
    Stored Procedure: dbo.sp_Partener_Update
    Purpose: Update an existing partner
    Usage: EXEC dbo.sp_Partener_Update @PartenerId = 1, @CodIntern = 'P001', @Denumire = 'Updated Partner', @CodFiscal = '87654321', @Judet = 'Cluj', @Localitate = 'Cluj-Napoca', @Adresa = 'Str. Updated', @UtilizatorActualizare = 'admin'
    Returns: Number of affected rows
*/
GO
IF OBJECT_ID('dbo.sp_Partener_Update','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Partener_Update AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE dbo.sp_Partener_Update
    @PartenerId           INT,
    @CodIntern            NVARCHAR(50),
    @Denumire             NVARCHAR(200),
    @CodFiscal            NVARCHAR(50) = NULL,
    @Judet                NVARCHAR(100) = NULL,
    @Localitate           NVARCHAR(100) = NULL,
    @Adresa               NVARCHAR(500) = NULL,
    @UtilizatorActualizare NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Validate required fields
        IF @PartenerId IS NULL OR @PartenerId <= 0
        BEGIN
            RAISERROR('PartenerId invalid.', 16, 1);
            RETURN;
        END
        
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
        
        -- Check if partner exists
        IF NOT EXISTS (SELECT 1 FROM dbo.Partener WHERE PartenerId = @PartenerId)
        BEGIN
            RAISERROR('Partenerul nu exist?.', 16, 1);
            RETURN;
        END
        
        -- Check if CodIntern already exists for another partner
        IF EXISTS (SELECT 1 FROM dbo.Partener WHERE CodIntern = @CodIntern AND PartenerId != @PartenerId)
        BEGIN
            RAISERROR('Un alt partener cu acest cod intern deja exist?.', 16, 1);
            RETURN;
        END
        
        -- Update partner
        UPDATE dbo.Partener 
        SET 
            CodIntern = @CodIntern,
            Denumire = @Denumire,
            CodFiscal = @CodFiscal,
            Judet = @Judet,
            Localitate = @Localitate,
            Adresa = @Adresa,
            DataActualizare = GETDATE(),
            UtilizatorActualizare = @UtilizatorActualizare
        WHERE PartenerId = @PartenerId;
        
        -- Return number of affected rows
        SELECT @@ROWCOUNT AS RowsAffected;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO