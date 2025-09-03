---------------------------------------------------
-- Procedura de inserare corectată
---------------------------------------------------
--IF OBJECT_ID('InsertDepartament', 'P') IS NOT NULL DROP PROCEDURE InsertDepartament;
--GO

CREATE PROCEDURE InsertDepartament
    @Nume NVARCHAR(200),
    @Tip NVARCHAR(50),
    @ParentID UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NewID UNIQUEIDENTIFIER;

    -- Inserăm nodul și lăsăm DEFAULT să genereze ID-ul
    INSERT INTO Departamente (Nume, Tip)
    VALUES (@Nume, @Tip);

    -- Obținem ID-ul generat
    SET @NewID = (SELECT DepartamentID FROM Departamente WHERE Nume = @Nume AND Tip = @Tip);

    -- Fiecare nod este descendentul lui însuși
    INSERT INTO DepartamenteIerarhie (AncestorID, DescendantID, Nivel)
    VALUES (@NewID, @NewID, 0);

    -- Dacă are părinte, adăugăm legături pentru toți strămoșii părintelui
    IF @ParentID IS NOT NULL
    BEGIN
        INSERT INTO DepartamenteIerarhie (AncestorID, DescendantID, Nivel)
        SELECT AncestorID, @NewID, Nivel + 1
        FROM DepartamenteIerarhie
        WHERE DescendantID = @ParentID;
    END
END;
GO