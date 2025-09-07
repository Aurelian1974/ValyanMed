<#
.SYNOPSIS
    Script pentru opera?ii administrative rapide pe baza de date ValyanMed
.DESCRIPTION
    Shortcuts pentru opera?ii administrative comune: creare tabele, SP, func?ii, etc.
#>

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("create-table", "create-sp", "create-function", "create-view", "backup", "restore", "maintenance", "user-management")]
    [string]$Operation,
    
    [Parameter(Mandatory=$false)]
    [string]$Name = "",
    
    [Parameter(Mandatory=$false)]
    [string]$Parameters = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$Force = $false
)

$adminScriptPath = Join-Path $PSScriptRoot "Admin-ValyanMedDatabase.ps1"

function Get-CreateTableTemplate {
    param([string]$TableName)
    
    return @"
CREATE TABLE [$TableName] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Guid] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [Nume] NVARCHAR(100) NOT NULL,
    [Descriere] NVARCHAR(500) NULL,
    [EsteActiv] BIT NOT NULL DEFAULT 1,
    [DataCreare] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [DataModificare] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UtilizatorCreare] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
    [UtilizatorModificare] NVARCHAR(100) NULL DEFAULT SYSTEM_USER
);

-- Index pentru GUID
CREATE UNIQUE NONCLUSTERED INDEX IX_${TableName}_Guid ON [$TableName] ([Guid]);

-- Index pentru nume
CREATE NONCLUSTERED INDEX IX_${TableName}_Nume ON [$TableName] ([Nume]);

-- Index pentru status activ
CREATE NONCLUSTERED INDEX IX_${TableName}_EsteActiv ON [$TableName] ([EsteActiv]);
"@
}

function Get-CreateStoredProcedureTemplate {
    param([string]$SPName, [string]$TableName)
    
    return @"
-- Procedur? stocat? pentru opera?ii CRUD pe tabela $TableName
CREATE PROCEDURE [dbo].[$SPName]
    @Operatie NVARCHAR(10),  -- 'SELECT', 'INSERT', 'UPDATE', 'DELETE'
    @Id INT = NULL,
    @Nume NVARCHAR(100) = NULL,
    @Descriere NVARCHAR(500) = NULL,
    @EsteActiv BIT = 1,
    @UtilizatorModificare NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Seteaz? utilizatorul implicit
    IF @UtilizatorModificare IS NULL
        SET @UtilizatorModificare = SYSTEM_USER;
    
    IF @Operatie = 'SELECT'
    BEGIN
        IF @Id IS NOT NULL
        BEGIN
            -- Selectare dup? ID
            SELECT * FROM [$TableName] WHERE Id = @Id;
        END
        ELSE
        BEGIN
            -- Selectare toate înregistr?rile active
            SELECT * FROM [$TableName] WHERE EsteActiv = 1 ORDER BY DataCreare DESC;
        END
    END
    
    ELSE IF @Operatie = 'INSERT'
    BEGIN
        INSERT INTO [$TableName] (Nume, Descriere, EsteActiv, UtilizatorCreare)
        VALUES (@Nume, @Descriere, @EsteActiv, @UtilizatorModificare);
        
        SELECT SCOPE_IDENTITY() AS NewId;
    END
    
    ELSE IF @Operatie = 'UPDATE'
    BEGIN
        UPDATE [$TableName] 
        SET 
            Nume = ISNULL(@Nume, Nume),
            Descriere = ISNULL(@Descriere, Descriere),
            EsteActiv = ISNULL(@EsteActiv, EsteActiv),
            DataModificare = GETUTCDATE(),
            UtilizatorModificare = @UtilizatorModificare
        WHERE Id = @Id;
        
        SELECT @@ROWCOUNT AS RowsAffected;
    END
    
    ELSE IF @Operatie = 'DELETE'
    BEGIN
        -- Soft delete
        UPDATE [$TableName] 
        SET 
            EsteActiv = 0,
            DataModificare = GETUTCDATE(),
            UtilizatorModificare = @UtilizatorModificare
        WHERE Id = @Id;
        
        SELECT @@ROWCOUNT AS RowsAffected;
    END
    
    ELSE
    BEGIN
        RAISERROR('Opera?ie nevalid?. Folose?te: SELECT, INSERT, UPDATE, DELETE', 16, 1);
    END
END
"@
}

function Get-CreateFunctionTemplate {
    param([string]$FunctionName)
    
    return @"
CREATE FUNCTION [dbo].[$FunctionName]
(
    @InputParameter NVARCHAR(100)
)
RETURNS NVARCHAR(200)
AS
BEGIN
    DECLARE @Result NVARCHAR(200);
    
    -- Logica func?iei aici
    SET @Result = 'Procesat: ' + ISNULL(@InputParameter, 'NULL');
    
    RETURN @Result;
END
"@
}

function Get-CreateViewTemplate {
    param([string]$ViewName)
    
    return @"
CREATE VIEW [dbo].[$ViewName]
AS
SELECT 
    p.Id,
    p.Nume,
    p.Prenume,
    p.Email,
    p.Telefon,
    p.EsteActiva,
    p.DataCreare,
    'Utilizator Standard' AS TipUtilizator
FROM Persoane p
WHERE p.EsteActiva = 1
"@
}

switch ($Operation) {
    "create-table" {
        if ([string]::IsNullOrEmpty($Name)) {
            $Name = Read-Host "Introdu numele tabelei"
        }
        
        $query = Get-CreateTableTemplate -TableName $Name
        Write-Host "?? Crearea tabelei '$Name'..." -ForegroundColor Yellow
        
        & $adminScriptPath -Query $query -ConfirmExecution -Force:$Force
    }
    
    "create-sp" {
        if ([string]::IsNullOrEmpty($Name)) {
            $Name = Read-Host "Introdu numele procedurii stocate"
        }
        
        $tableName = if ([string]::IsNullOrEmpty($Parameters)) {
            Read-Host "Introdu numele tabelei pentru CRUD"
        } else { $Parameters }
        
        $query = Get-CreateStoredProcedureTemplate -SPName $Name -TableName $tableName
        Write-Host "?? Crearea procedurii stocate '$Name'..." -ForegroundColor Yellow
        
        & $adminScriptPath -Query $query -ConfirmExecution -Force:$Force
    }
    
    "create-function" {
        if ([string]::IsNullOrEmpty($Name)) {
            $Name = Read-Host "Introdu numele func?iei"
        }
        
        $query = Get-CreateFunctionTemplate -FunctionName $Name
        Write-Host "?? Crearea func?iei '$Name'..." -ForegroundColor Yellow
        
        & $adminScriptPath -Query $query -ConfirmExecution -Force:$Force
    }
    
    "create-view" {
        if ([string]::IsNullOrEmpty($Name)) {
            $Name = Read-Host "Introdu numele view-ului"
        }
        
        $query = Get-CreateViewTemplate -ViewName $Name
        Write-Host "??? Crearea view-ului '$Name'..." -ForegroundColor Yellow
        
        & $adminScriptPath -Query $query -ConfirmExecution -Force:$Force
    }
    
    "backup" {
        $backupPath = "C:\Backup\ValyanMed_Backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').bak"
        $query = "BACKUP DATABASE [ValyanMed] TO DISK = '$backupPath' WITH FORMAT, CHECKSUM"
        
        Write-Host "?? Crearea backup-ului bazei de date..." -ForegroundColor Yellow
        Write-Host "Loca?ie: $backupPath" -ForegroundColor Cyan
        
        & $adminScriptPath -Query $query -ConfirmExecution -Force:$Force
    }
    
    "maintenance" {
        Write-Host "?? Executarea opera?iilor de mentenan??..." -ForegroundColor Yellow
        
        $maintenanceQueries = @(
            "-- Actualizare statistici pentru toate tabelele",
            "EXEC sp_updatestats;",
            "",
            "-- Reorganizare indexuri fragmentate",
            "DECLARE @sql NVARCHAR(MAX) = '';
            SELECT @sql = @sql + 'ALTER INDEX ALL ON [' + SCHEMA_NAME(t.schema_id) + '].[' + t.name + '] REORGANIZE;' + CHAR(13)
            FROM sys.tables t
            WHERE t.is_ms_shipped = 0;
            EXEC sp_executesql @sql;",
            "",
            "-- Cur??are plan cache vechi",
            "DBCC FREEPROCCACHE;"
        )
        
        foreach ($query in $maintenanceQueries) {
            if (-not [string]::IsNullOrWhiteSpace($query) -and -not $query.StartsWith("--")) {
                & $adminScriptPath -Query $query -Force:$Force
            }
        }
    }
    
    "user-management" {
        Write-Host "?? Managementul utilizatorilor..." -ForegroundColor Yellow
        
        $userQueries = @(
            "-- Utilizatori conecta?i",
            "SELECT 
                session_id,
                login_name,
                host_name,
                program_name,
                login_time,
                last_request_start_time
            FROM sys.dm_exec_sessions 
            WHERE is_user_process = 1 AND database_id = DB_ID('ValyanMed')"
        )
        
        & $adminScriptPath -Query $userQueries[1] -Force:$Force
    }
}

Write-Host "`n? Opera?ia '$Operation' a fost completat?!" -ForegroundColor Green

# Exemple de utilizare
Write-Host "`n?? Exemple de utilizare:" -ForegroundColor Cyan
Write-Host "# Creare tabel? nou?:"
Write-Host ".\Quick-AdminValyanMed.ps1 -Operation create-table -Name 'TabelNou'"
Write-Host ""
Write-Host "# Creare procedur? stocat?:"
Write-Host ".\Quick-AdminValyanMed.ps1 -Operation create-sp -Name 'sp_ManageTabelNou' -Parameters 'TabelNou'"
Write-Host ""
Write-Host "# Backup baza de date:"
Write-Host ".\Quick-AdminValyanMed.ps1 -Operation backup"
Write-Host ""
Write-Host "# Mentenan?? automat?:"
Write-Host ".\Quick-AdminValyanMed.ps1 -Operation maintenance"