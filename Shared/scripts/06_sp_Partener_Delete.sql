/*
    Stored Procedure: dbo.sp_Partener_Delete
    Purpose: Soft delete a partner (set Activ = 0)
    Usage: EXEC dbo.sp_Partener_Delete @PartenerId = 1
    Returns: Number of affected rows
*/
GO
IF OBJECT_ID('dbo.sp_Partener_Delete','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Partener_Delete AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE dbo.sp_Partener_Delete
    @PartenerId INT
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
        
        -- Check if partner exists and is active
        IF NOT EXISTS (SELECT 1 FROM dbo.Partener WHERE PartenerId = @PartenerId AND Activ = 1)
        BEGIN
            RAISERROR('Partenerul nu exist? sau este deja inactiv.', 16, 1);
            RETURN;
        END
        
        -- Soft delete (set Activ = 0)
        UPDATE dbo.Partener 
        SET 
            Activ = 0,
            DataActualizare = GETDATE()
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