
-- Tabela Persoana
CREATE TABLE Persoana (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Guid UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    Nume NVARCHAR(100) NOT NULL,
    Prenume NVARCHAR(100) NOT NULL,
    Judet NVARCHAR(100),
    Localitate NVARCHAR(100),
    Strada NVARCHAR(150),
    NumarStrada NVARCHAR(50),
    CodPostal NVARCHAR(20),
    PozitieOrganizatie NVARCHAR(100),
    DataNasterii DATE,
    DataCreare DATETIME DEFAULT GETDATE(),
    DataModificare DATETIME NULL
);

-- Tabela Utilizator
CREATE TABLE Utilizator (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Guid UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    PersoanaId INT NOT NULL,
    NumeUtilizator NVARCHAR(100) NOT NULL,
    ParolaHash NVARCHAR(512) NOT NULL,
    Email NVARCHAR(150),
    Telefon NVARCHAR(50),
    DataCreare DATETIME DEFAULT GETDATE(),
    DataModificare DATETIME NULL,
    CONSTRAINT FK_Utilizator_Persoana FOREIGN KEY (PersoanaId) REFERENCES Persoana(Id)
);

-- Tabela de audit pentru Persoana
CREATE TABLE Audit_Persoana (
    AuditId INT IDENTITY(1,1) PRIMARY KEY,
    PersoanaId INT,
    Actiune NVARCHAR(50), -- INSERT, UPDATE, DELETE
    Nume NVARCHAR(100),
    Prenume NVARCHAR(100),
    Judet NVARCHAR(100),
    Localitate NVARCHAR(100),
    Strada NVARCHAR(150),
    NumarStrada NVARCHAR(50),
    CodPostal NVARCHAR(20),
    PozitieOrganizatie NVARCHAR(100),
    DataNasterii DATE,
    DataCreare DATETIME,
    DataModificare DATETIME,
    DataAudit DATETIME DEFAULT GETDATE(),
    UserAudit NVARCHAR(100)
);

-- Tabela de audit pentru Utilizator
CREATE TABLE Audit_Utilizator (
    AuditId INT IDENTITY(1,1) PRIMARY KEY,
    UtilizatorId INT,
    Actiune NVARCHAR(50), -- INSERT, UPDATE, DELETE
    PersoanaId INT,
    NumeUtilizator NVARCHAR(100),
    ParolaHash NVARCHAR(512),
    Email NVARCHAR(150),
    Telefon NVARCHAR(50),
    DataCreare DATETIME,
    DataModificare DATETIME,
    DataAudit DATETIME DEFAULT GETDATE(),
    UserAudit NVARCHAR(100)
);
