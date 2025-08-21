/*
    Stored Procedure: dbo.sp_Partener_GetAll
    Purpose: Retrieve all active partners
    Usage: EXEC dbo.sp_Partener_GetAll
*/
GO
IF OBJECT_ID('dbo.sp_Partener_GetAll','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Partener_GetAll AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE dbo.sp_Partener_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        PartenerId,
        PartenerGuid,
        CodIntern,
        Denumire,
        CodFiscal,
        Judet,
        Localitate,
        Adresa,
        DataCreare,
        DataActualizare,
        UtilizatorCreare,
        UtilizatorActualizare,
        Activ
    FROM dbo.Partener WITH (NOLOCK)
    WHERE Activ = 1
    ORDER BY Denumire ASC;
END
GO