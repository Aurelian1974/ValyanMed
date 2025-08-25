/*
    Create Pacienti table if it doesn't exist
    Purpose: Store patient information for the medical system
*/

-- Check if Pacienti table exists, create if not
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Pacienti' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Pacienti (
        PacientID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        PacientGUID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        Nume NVARCHAR(100) NOT NULL,
        Prenume NVARCHAR(100) NOT NULL,
        CNP NVARCHAR(13) NULL,
        DataNasterii DATE NULL,
        Gen NVARCHAR(20) NULL,
        Telefon NVARCHAR(20) NULL,
        Email NVARCHAR(255) NULL,
        Adresa NVARCHAR(500) NULL,
        Oras NVARCHAR(100) NULL,
        Judet NVARCHAR(100) NULL,
        CodPostal NVARCHAR(10) NULL,
        DataCreare DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        DataModificare DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    -- Add indexes for better performance
    CREATE INDEX IX_Pacienti_CNP ON dbo.Pacienti(CNP);
    CREATE INDEX IX_Pacienti_Email ON dbo.Pacienti(Email);
    CREATE INDEX IX_Pacienti_Nume_Prenume ON dbo.Pacienti(Nume, Prenume);
    CREATE INDEX IX_Pacienti_DataCreare ON dbo.Pacienti(DataCreare);
    CREATE INDEX IX_Pacienti_Judet ON dbo.Pacienti(Judet);

    PRINT 'Pacienti table created successfully!';
END
ELSE
BEGIN
    PRINT 'Pacienti table already exists.';
END

-- Add some sample data if table is empty
IF NOT EXISTS (SELECT 1 FROM dbo.Pacienti)
BEGIN
    INSERT INTO dbo.Pacienti (PacientID, PacientGUID, Nume, Prenume, CNP, DataNasterii, Gen, Telefon, Email, Oras, Judet, DataCreare, DataModificare)
    VALUES 
        (NEWID(), NEWID(), 'Popescu', 'Ion', '1800101123456', '1980-01-01', 'Masculin', '0721123456', 'ion.popescu@email.ro', 'Bucuresti', 'Bucuresti', GETUTCDATE(), GETUTCDATE()),
        (NEWID(), NEWID(), 'Georgescu', 'Maria', '2851202234567', '1985-12-02', 'Feminin', '0721234567', 'maria.georgescu@email.ro', 'Cluj-Napoca', 'Cluj', GETUTCDATE(), GETUTCDATE()),
        (NEWID(), NEWID(), 'Ionescu', 'Andrei', '1750615345678', '1975-06-15', 'Masculin', '0721345678', 'andrei.ionescu@email.ro', 'Timisoara', 'Timis', GETUTCDATE(), GETUTCDATE()),
        (NEWID(), NEWID(), 'Marinescu', 'Ana', '2901025456789', '1990-10-25', 'Feminin', '0721456789', 'ana.marinescu@email.ro', 'Iasi', 'Iasi', GETUTCDATE(), GETUTCDATE()),
        (NEWID(), NEWID(), 'Dumitrescu', 'George', '1820308567890', '1982-03-08', 'Masculin', '0721567890', 'george.dumitrescu@email.ro', 'Constanta', 'Constanta', GETUTCDATE(), GETUTCDATE()),
        (NEWID(), NEWID(), 'Vasilescu', 'Elena', '2751115678901', '1975-11-15', 'Feminin', '0721678901', 'elena.vasilescu@email.ro', 'Brasov', 'Brasov', GETUTCDATE(), GETUTCDATE()),
        (NEWID(), NEWID(), 'Radu', 'Mihai', '1880422789012', '1988-04-22', 'Masculin', '0721789012', 'mihai.radu@email.ro', 'Craiova', 'Dolj', GETUTCDATE(), GETUTCDATE()),
        (NEWID(), NEWID(), 'Popa', 'Alexandra', '2920730890123', '1992-07-30', 'Feminin', '0721890123', 'alexandra.popa@email.ro', 'Galati', 'Galati', GETUTCDATE(), GETUTCDATE()),
        (NEWID(), NEWID(), 'Stancu', 'Florin', '1850614901234', '1985-06-14', 'Masculin', '0721901234', 'florin.stancu@email.ro', 'Ploiesti', 'Prahova', GETUTCDATE(), GETUTCDATE()),
        (NEWID(), NEWID(), 'Munteanu', 'Cristina', '2780920012345', '1978-09-20', 'Feminin', '0722012345', 'cristina.munteanu@email.ro', 'Oradea', 'Bihor', GETUTCDATE(), GETUTCDATE());

    PRINT 'Sample patient data inserted successfully!';
END
ELSE
BEGIN
    PRINT 'Pacienti table already contains data.';
END

GO