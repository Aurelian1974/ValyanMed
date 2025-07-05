
-- Trigger de audit pentru UPDATE pe tabela Utilizator
CREATE TRIGGER trg_Audit_Utilizator_Update
ON Utilizator
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Inseră în tabela de audit câmpurile modificate
    INSERT INTO Audit_Utilizator (
        UtilizatorId,
        Actiune,
        PersoanaId,
        NumeUtilizator,
        ParolaHash,
        Email,
        Telefon,
        DataCreare,
        DataModificare,
        DataAudit,
        UserAudit
    )
    SELECT
        d.Id,
        'UPDATE',
        d.PersoanaId,
        d.NumeUtilizator,
        d.ParolaHash,
        d.Email,
        d.Telefon,
        d.DataCreare,
        d.DataModificare,
        GETDATE(),
        SYSTEM_USER
    FROM inserted d
    JOIN deleted o ON d.Id = o.Id
    WHERE 
        ISNULL(d.NumeUtilizator, '') <> ISNULL(o.NumeUtilizator, '') OR
        ISNULL(d.Email, '') <> ISNULL(o.Email, '') OR
        ISNULL(d.Telefon, '') <> ISNULL(o.Telefon, '') OR
        ISNULL(d.ParolaHash, '') <> ISNULL(o.ParolaHash, '');

    -- Exemplu extensibil: se pot adăuga coloane separate pentru valoare veche / nouă dacă se dorește
END;
GO

-- Tabel pentru detalii modificare pe câmpuri individuale
CREATE TABLE Audit_UtilizatorDetaliat (
    AuditDetaliuId INT IDENTITY(1,1) PRIMARY KEY,
    UtilizatorId INT,
    Coloana NVARCHAR(100),
    ValoareVeche NVARCHAR(MAX),
    ValoareNoua NVARCHAR(MAX),
    DataAudit DATETIME DEFAULT GETDATE(),
    UserAudit NVARCHAR(100)
);
GO

-- Trigger detaliat pe coloane
CREATE TRIGGER trg_Audit_Utilizator_Coloane
ON Utilizator
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- NumeUtilizator
    INSERT INTO Audit_UtilizatorDetaliat (UtilizatorId, Coloana, ValoareVeche, ValoareNoua, UserAudit)
    SELECT d.Id, 'NumeUtilizator', o.NumeUtilizator, d.NumeUtilizator, SYSTEM_USER
    FROM inserted d
    JOIN deleted o ON d.Id = o.Id
    WHERE ISNULL(d.NumeUtilizator, '') <> ISNULL(o.NumeUtilizator, '');

    -- Email
    INSERT INTO Audit_UtilizatorDetaliat (UtilizatorId, Coloana, ValoareVeche, ValoareNoua, UserAudit)
    SELECT d.Id, 'Email', o.Email, d.Email, SYSTEM_USER
    FROM inserted d
    JOIN deleted o ON d.Id = o.Id
    WHERE ISNULL(d.Email, '') <> ISNULL(o.Email, '');

    -- Telefon
    INSERT INTO Audit_UtilizatorDetaliat (UtilizatorId, Coloana, ValoareVeche, ValoareNoua, UserAudit)
    SELECT d.Id, 'Telefon', o.Telefon, d.Telefon, SYSTEM_USER
    FROM inserted d
    JOIN deleted o ON d.Id = o.Id
    WHERE ISNULL(d.Telefon, '') <> ISNULL(o.Telefon, '');
END;
GO
