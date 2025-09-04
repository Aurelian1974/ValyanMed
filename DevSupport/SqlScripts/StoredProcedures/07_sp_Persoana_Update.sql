/*
    Stored Procedures pentru gestionarea persoanelor
    Conform planului de refactorizare - eliminare diacritice, rich services, error handling specific
*/

-- =============================================
-- Update Person
-- =============================================
IF OBJECT_ID('dbo.sp_UpdatePersoana','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_UpdatePersoana AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE dbo.sp_UpdatePersoana
    @Id INT,
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @Judet NVARCHAR(100) = NULL,
    @Localitate NVARCHAR(100) = NULL,
    @Strada NVARCHAR(150) = NULL,
    @NumarStrada NVARCHAR(50) = NULL,
    @CodPostal NVARCHAR(20) = NULL,
    @PozitieOrganizatie NVARCHAR(100) = NULL,
    @DataNasterii DATE = NULL,
    @CNP NVARCHAR(13) = NULL,
    @TipActIdentitate NVARCHAR(5) = NULL,
    @SerieActIdentitate NVARCHAR(2) = NULL,
    @NumarActIdentitate NVARCHAR(6) = NULL,
    @StareCivila NVARCHAR(50) = NULL,
    @Gen NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Validate required fields
        IF @Id IS NULL OR @Id <= 0
        BEGIN
            RAISERROR('ID-ul persoanei este invalid.', 16, 1);
            RETURN;
        END
        
        IF @Nume IS NULL OR LEN(LTRIM(RTRIM(@Nume))) = 0
        BEGIN
            RAISERROR('Numele este obligatoriu.', 16, 1);
            RETURN;
        END
        
        IF @Prenume IS NULL OR LEN(LTRIM(RTRIM(@Prenume))) = 0
        BEGIN
            RAISERROR('Prenumele este obligatoriu.', 16, 1);
            RETURN;
        END
        
        -- Check if person exists
        IF NOT EXISTS (SELECT 1 FROM dbo.Persoana WHERE Id = @Id)
        BEGIN
            RAISERROR('Persoana nu exista.', 16, 1);
            RETURN;
        END
        
        -- Check if CNP already exists for another person
        IF @CNP IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.Persoana WHERE CNP = @CNP AND Id != @Id)
        BEGIN
            RAISERROR('O alta persoana cu acest CNP deja exista.', 16, 1);
            RETURN;
        END
        
        -- Update person
        UPDATE dbo.Persoana 
        SET 
            Nume = @Nume,
            Prenume = @Prenume,
            Judet = @Judet,
            Localitate = @Localitate,
            Strada = @Strada,
            NumarStrada = @NumarStrada,
            CodPostal = @CodPostal,
            PozitieOrganizatie = @PozitieOrganizatie,
            DataNasterii = @DataNasterii,
            CNP = @CNP,
            TipActIdentitate = @TipActIdentitate,
            SerieActIdentitate = @SerieActIdentitate,
            NumarActIdentitate = @NumarActIdentitate,
            StareCivila = @StareCivila,
            Gen = @Gen,
            DataModificare = GETDATE()
        WHERE Id = @Id;
        
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