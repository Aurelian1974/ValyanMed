-- Migrare ini?ial? pentru tabelele de baz? ale aplica?iei ValyanMed
-- Data: 2025-01-04
-- Versiune: 001

-- Tabel pentru utilizatori
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Email NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Role NVARCHAR(50) NOT NULL DEFAULT 'User',
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Index pentru c?utare rapid? dup? email
CREATE NONCLUSTERED INDEX IX_Users_Email ON Users (Email);

-- Index pentru filtrare dup? rol
CREATE NONCLUSTERED INDEX IX_Users_Role ON Users (Role);

-- Tabel pentru pacienti
CREATE TABLE Patients (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CNP NVARCHAR(13) NOT NULL UNIQUE,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(10) NOT NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(256),
    Address NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Index pentru c?utare dup? CNP
CREATE NONCLUSTERED INDEX IX_Patients_CNP ON Patients (CNP);

-- Index pentru c?utare dup? nume
CREATE NONCLUSTERED INDEX IX_Patients_Name ON Patients (LastName, FirstName);