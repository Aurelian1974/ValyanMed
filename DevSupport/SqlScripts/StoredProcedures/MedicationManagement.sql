-- Proceduri stocate pentru gestionarea medicamentelor
-- Data: 2025-01-04
-- Autor: DevSupport

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ==============================================
-- Procedur? pentru c?utarea medicamentelor
-- ==============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_CautaMedicamente]
    @NumeSearch NVARCHAR(200) = NULL,
    @ProducatorSearch NVARCHAR(200) = NULL,
    @CodATC NVARCHAR(20) = NULL,
    @Status NVARCHAR(50) = NULL,
    @StocMinim INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        m.MedicamentID,
        m.Nume,
        m.DenumireComunaInternationala,
        m.Concentratie,
        m.FormaFarmaceutica,
        m.Producator,
        m.CodATC,
        m.Status,
        m.Pret,
        m.Stoc,
        m.StocSiguranta,
        CASE 
            WHEN m.Stoc <= m.StocSiguranta THEN 'STOC SC?ZUT'
            WHEN m.Stoc = 0 THEN 'EPUIZAT'
            ELSE 'OK'
        END AS StatusStoc,
        m.DataExpirare,
        CASE 
            WHEN m.DataExpirare <= GETDATE() THEN 'EXPIRAT'
            WHEN m.DataExpirare <= DATEADD(MONTH, 3, GETDATE()) THEN 'EXPIR? CURÂND'
            ELSE 'VALID'
        END AS StatusExpirare
    FROM Medicament m
    WHERE 
        (@NumeSearch IS NULL OR m.Nume LIKE '%' + @NumeSearch + '%')
        AND (@ProducatorSearch IS NULL OR m.Producator LIKE '%' + @ProducatorSearch + '%')
        AND (@CodATC IS NULL OR m.CodATC = @CodATC)
        AND (@Status IS NULL OR m.Status = @Status)
        AND (@StocMinim IS NULL OR m.Stoc >= @StocMinim)
        AND m.Activ = 1
    ORDER BY m.Nume;
END
GO

-- ==============================================
-- Procedur? pentru alerta stocuri sc?zute
-- ==============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_AlerteStocuriScazute]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        m.Nume AS Medicament,
        m.Concentratie,
        m.FormaFarmaceutica,
        m.Producator,
        m.Stoc AS StocCurent,
        m.StocSiguranta AS StocMinim,
        (m.StocSiguranta - m.Stoc) AS DeficitStoc,
        m.Pret,
        (m.StocSiguranta - m.Stoc) * m.Pret AS CostReaprovizionare
    FROM Medicament m
    WHERE 
        m.Stoc <= m.StocSiguranta 
        AND m.Activ = 1
        AND m.Status = 'Activ'
    ORDER BY 
        CASE WHEN m.Stoc = 0 THEN 1 ELSE 2 END, -- Epuizate primul
        (m.StocSiguranta - m.Stoc) DESC; -- Cel mai mare deficit
END
GO

-- ==============================================
-- Procedur? pentru actualizarea stocului
-- ==============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_ActualizeazaStocMedicament]
    @MedicamentID INT,
    @Cantitate INT,
    @TipOperatie NVARCHAR(20), -- 'INTRARE' sau 'IESIRE'
    @Utilizator NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        DECLARE @StocCurent INT;
        DECLARE @NumeMedicament NVARCHAR(200);
        
        -- Verific?m dac? medicamentul exist?
        SELECT 
            @StocCurent = Stoc,
            @NumeMedicament = Nume
        FROM Medicament 
        WHERE MedicamentID = @MedicamentID AND Activ = 1;
        
        IF @StocCurent IS NULL
        BEGIN
            RAISERROR('Medicamentul specificat nu exist? sau nu este activ!', 16, 1);
            RETURN;
        END
        
        -- Calcul?m noul stoc
        DECLARE @StocNou INT;
        
        IF @TipOperatie = 'INTRARE'
        BEGIN
            SET @StocNou = @StocCurent + @Cantitate;
        END
        ELSE IF @TipOperatie = 'IESIRE'
        BEGIN
            IF @StocCurent < @Cantitate
            BEGIN
                RAISERROR('Stocul insuficient! Stoc curent: %d, Cantitate solicitat?: %d', 16, 1, @StocCurent, @Cantitate);
                RETURN;
            END
            SET @StocNou = @StocCurent - @Cantitate;
        END
        ELSE
        BEGIN
            RAISERROR('Tip opera?ie invalid! Utiliza?i INTRARE sau IESIRE.', 16, 1);
            RETURN;
        END
        
        -- Actualiz?m stocul
        UPDATE Medicament 
        SET 
            Stoc = @StocNou,
            DataActualizare = GETDATE(),
            UtilizatorActualizare = ISNULL(@Utilizator, SYSTEM_USER)
        WHERE MedicamentID = @MedicamentID;
        
        COMMIT TRANSACTION;
        
        PRINT 'Stocul pentru ' + @NumeMedicament + ' a fost actualizat cu succes!';
        PRINT 'Stoc anterior: ' + CAST(@StocCurent AS NVARCHAR(10));
        PRINT 'Stoc nou: ' + CAST(@StocNou AS NVARCHAR(10));
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ==============================================
-- Procedur? pentru raport expir?ri
-- ==============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_RaportExpirari]
    @LuniAnticipare INT = 6
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DataLimita DATE = DATEADD(MONTH, @LuniAnticipare, GETDATE());
    
    SELECT 
        m.Nume AS Medicament,
        m.Concentratie,
        m.FormaFarmaceutica,
        m.Producator,
        m.DataExpirare,
        DATEDIFF(DAY, GETDATE(), m.DataExpirare) AS ZileRamase,
        m.Stoc,
        m.Pret,
        (m.Stoc * m.Pret) AS ValoareStoc,
        CASE 
            WHEN m.DataExpirare <= GETDATE() THEN 'EXPIRAT'
            WHEN m.DataExpirare <= DATEADD(MONTH, 1, GETDATE()) THEN 'CRITIC (< 1 lun?)'
            WHEN m.DataExpirare <= DATEADD(MONTH, 3, GETDATE()) THEN 'ATEN?IE (< 3 luni)'
            ELSE 'MONITORIZARE (< ' + CAST(@LuniAnticipare AS NVARCHAR(2)) + ' luni)'
        END AS Urgenta
    FROM Medicament m
    WHERE 
        m.DataExpirare <= @DataLimita
        AND m.Stoc > 0
        AND m.Activ = 1
    ORDER BY 
        CASE 
            WHEN m.DataExpirare <= GETDATE() THEN 1
            WHEN m.DataExpirare <= DATEADD(MONTH, 1, GETDATE()) THEN 2
            WHEN m.DataExpirare <= DATEADD(MONTH, 3, GETDATE()) THEN 3
            ELSE 4
        END,
        m.DataExpirare;
END
GO

-- ==============================================
-- Procedur? pentru statistici medicamente
-- ==============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_StatisticiMedicamente]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Statistici generale
    SELECT 'STATISTICI GENERALE' AS Categoria, '' AS Detalii, '' AS Valoare
    UNION ALL
    SELECT 'Total medicamente active', '', CAST(COUNT(*) AS NVARCHAR(20))
    FROM Medicament WHERE Activ = 1
    
    UNION ALL
    SELECT 'Medicamente în stoc', '', CAST(COUNT(*) AS NVARCHAR(20))
    FROM Medicament WHERE Activ = 1 AND Stoc > 0
    
    UNION ALL
    SELECT 'Medicamente epuizate', '', CAST(COUNT(*) AS NVARCHAR(20))
    FROM Medicament WHERE Activ = 1 AND Stoc = 0
    
    UNION ALL
    SELECT 'Valoare total? stoc', '', FORMAT(SUM(Stoc * ISNULL(Pret, 0)), 'C', 'ro-RO')
    FROM Medicament WHERE Activ = 1
    
    UNION ALL
    SELECT '', '', ''
    
    UNION ALL
    SELECT 'TOP 5 PRODUC?TORI', '', ''
    
    UNION ALL
    SELECT 
        'Produc?tor',
        Producator,
        CAST(COUNT(*) AS NVARCHAR(10)) + ' medicamente'
    FROM (
        SELECT TOP 5 
            Producator,
            COUNT(*) as Numar
        FROM Medicament 
        WHERE Activ = 1 
        GROUP BY Producator 
        ORDER BY COUNT(*) DESC
    ) t
    
    UNION ALL
    SELECT '', '', ''
    
    UNION ALL
    SELECT 'CATEGORII ATC', '', ''
    
    UNION ALL
    SELECT 
        'Cod ATC',
        LEFT(CodATC, 1) + ' - ' + 
        CASE LEFT(CodATC, 1)
            WHEN 'A' THEN 'Tract alimentar ?i metabolism'
            WHEN 'B' THEN 'Sânge ?i organe hematopoietice'
            WHEN 'C' THEN 'Sistem cardiovascular'
            WHEN 'D' THEN 'Dermatologice'
            WHEN 'G' THEN 'Sistem genitourinar ?i hormoni sexuali'
            WHEN 'H' THEN 'Preparate hormonale sistemice'
            WHEN 'J' THEN 'Antiinfec?ioase generale pentru uz sistemic'
            WHEN 'L' THEN 'Antineoplazice ?i agen?i imunomodulatori'
            WHEN 'M' THEN 'Sistem musculo-scheletic'
            WHEN 'N' THEN 'Sistem nervos'
            WHEN 'P' THEN 'Produse antiparazitare'
            WHEN 'R' THEN 'Sistem respirator'
            WHEN 'S' THEN 'Organe de sim?'
            WHEN 'V' THEN 'Diverse'
            ELSE 'Altele'
        END,
        CAST(COUNT(*) AS NVARCHAR(10))
    FROM Medicament 
    WHERE Activ = 1 
    GROUP BY LEFT(CodATC, 1)
    ORDER BY Categoria, Detalii;
END
GO

PRINT 'Procedurile stocate pentru medicamente au fost create cu succes!'