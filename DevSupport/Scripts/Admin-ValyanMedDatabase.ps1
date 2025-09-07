<#
.SYNOPSIS
    Script PowerShell pentru administrarea complet? a bazei de date ValyanMed
.DESCRIPTION
    Permite executarea tuturor tipurilor de comenzi SQL: SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, DROP, etc.
.PARAMETER Query
    Query-ul SQL de executat
.PARAMETER Format
    Formatul output-ului: Text, Json, Csv (default: Text)
.PARAMETER Server
    Numele serverului SQL (default: TS1828\ERP)
.PARAMETER Database
    Numele bazei de date (default: ValyanMed)
.PARAMETER ConfirmExecution
    Switch pentru confirmarea comenzilor periculoase (DROP, DELETE f?r? WHERE, etc.)
.EXAMPLE
    .\Admin-ValyanMedDatabase.ps1 -Query "SELECT TOP 10 * FROM Persoane"
.EXAMPLE
    .\Admin-ValyanMedDatabase.ps1 -Query "CREATE TABLE Test (Id INT IDENTITY(1,1), Nume NVARCHAR(100))" -ConfirmExecution
.EXAMPLE
    .\Admin-ValyanMedDatabase.ps1 -Query "INSERT INTO Persoane (Nume, Prenume) VALUES ('Test', 'User')"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Query,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("Text", "Json", "Csv")]
    [string]$Format = "Text",
    
    [Parameter(Mandatory=$false)]
    [string]$Server = "TS1828\ERP",
    
    [Parameter(Mandatory=$false)]
    [string]$Database = "ValyanMed",
    
    [Parameter(Mandatory=$false)]
    [switch]$UseIntegratedSecurity = $true,
    
    [Parameter(Mandatory=$false)]
    [string]$Username = "",
    
    [Parameter(Mandatory=$false)]
    [string]$Password = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$ConfirmExecution = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$Force = $false
)

# Func?ie pentru detectarea tipului de comand? SQL
function Get-SqlCommandType {
    param([string]$SqlQuery)
    
    $normalizedQuery = $SqlQuery.Trim().ToUpper()
    
    if ($normalizedQuery.StartsWith("SELECT")) { return "SELECT" }
    if ($normalizedQuery.StartsWith("INSERT")) { return "INSERT" }
    if ($normalizedQuery.StartsWith("UPDATE")) { return "UPDATE" }
    if ($normalizedQuery.StartsWith("DELETE")) { return "DELETE" }
    if ($normalizedQuery.StartsWith("CREATE")) { return "CREATE" }
    if ($normalizedQuery.StartsWith("ALTER")) { return "ALTER" }
    if ($normalizedQuery.StartsWith("DROP")) { return "DROP" }
    if ($normalizedQuery.StartsWith("TRUNCATE")) { return "TRUNCATE" }
    if ($normalizedQuery.StartsWith("EXEC") -or $normalizedQuery.StartsWith("EXECUTE")) { return "EXECUTE" }
    if ($normalizedQuery.StartsWith("MERGE")) { return "MERGE" }
    if ($normalizedQuery.StartsWith("WITH")) { return "CTE" }
    
    return "UNKNOWN"
}

# Func?ie pentru validarea ?i avertizarea pentru comenzi periculoase
function Test-DangerousCommand {
    param([string]$SqlQuery, [string]$CommandType)
    
    $normalizedQuery = $SqlQuery.Trim().ToUpper()
    
    # Comenzi foarte periculoase care cer confirmare obligatorie
    $highRiskPatterns = @(
        "DROP\s+DATABASE",
        "DROP\s+TABLE",
        "DROP\s+VIEW",
        "DROP\s+PROCEDURE",
        "DROP\s+FUNCTION",
        "TRUNCATE\s+TABLE",
        "DELETE\s+FROM\s+\w+\s*$",  # DELETE f?r? WHERE
        "UPDATE\s+\w+\s+SET\s+.*$"  # UPDATE f?r? WHERE
    )
    
    foreach ($pattern in $highRiskPatterns) {
        if ($normalizedQuery -match $pattern) {
            return @{
                IsDangerous = $true
                RiskLevel = "HIGH"
                Message = "Comand? cu risc ridicat detectat?: $pattern"
            }
        }
    }
    
    # Comenzi cu risc mediu
    $mediumRiskPatterns = @(
        "ALTER\s+TABLE",
        "CREATE\s+TABLE",
        "INSERT\s+INTO",
        "UPDATE\s+.*WHERE",
        "DELETE\s+.*WHERE"
    )
    
    foreach ($pattern in $mediumRiskPatterns) {
        if ($normalizedQuery -match $pattern) {
            return @{
                IsDangerous = $true
                RiskLevel = "MEDIUM"
                Message = "Comand? cu risc mediu detectat?: $pattern"
            }
        }
    }
    
    return @{
        IsDangerous = $false
        RiskLevel = "LOW"
        Message = "Comand? sigur?"
    }
}

# Func?ie pentru confirmarea comenzilor periculoase
function Confirm-DangerousCommand {
    param($RiskAnalysis, [string]$Query)
    
    if (-not $RiskAnalysis.IsDangerous) {
        return $true
    }
    
    Write-Host ""
    Write-Host "??  ATEN?IE: COMAND? PERICULOAS? DETECTAT?!" -ForegroundColor Red
    Write-Host "Nivel risc: $($RiskAnalysis.RiskLevel)" -ForegroundColor Yellow
    Write-Host "Detalii: $($RiskAnalysis.Message)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Query de executat:" -ForegroundColor Cyan
    Write-Host $Query -ForegroundColor White
    Write-Host ""
    
    if ($RiskAnalysis.RiskLevel -eq "HIGH") {
        Write-Host "?? RISC RIDICAT: Aceast? comand? poate ?terge sau modifica date importante!" -ForegroundColor Red
        Write-Host "Asigur?-te c? ai backup ?i c? în?elegi consecin?ele!" -ForegroundColor Red
    }
    
    if ($Force) {
        Write-Host "? Executare for?at? cu parametrul -Force" -ForegroundColor Yellow
        return $true
    }
    
    $response = Read-Host "E?ti sigur c? vrei s? continui? Tasteaz? 'DA' pentru confirmare"
    return ($response -eq "DA")
}

# Func?ie pentru construirea connection string-ului
function Get-ConnectionString {
    if ($UseIntegratedSecurity) {
        return "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
    }
    else {
        return "Server=$Server;Database=$Database;User Id=$Username;Password=$Password;TrustServerCertificate=True;"
    }
}

# Func?ie pentru executarea query-ului
function Invoke-DatabaseCommand {
    param(
        [string]$ConnectionString,
        [string]$SqlQuery,
        [string]$CommandType
    )
    
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $ConnectionString
    
    try {
        $connection.Open()
        
        $command = $connection.CreateCommand()
        $command.CommandText = $SqlQuery
        $command.CommandTimeout = 60  # Timeout mai mare pentru opera?ii complexe
        
        # Pentru comenzi care returneaz? date
        if ($CommandType -eq "SELECT" -or $CommandType -eq "CTE") {
            $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
            $dataset = New-Object System.Data.DataSet
            
            $recordsAffected = $adapter.Fill($dataset)
            
            if ($dataset.Tables.Count -gt 0) {
                $table = $dataset.Tables[0]
                return @{
                    Type = "ResultSet"
                    Data = $table
                    RecordsAffected = $recordsAffected
                }
            }
        }
        # Pentru comenzi care nu returneaz? date (INSERT, UPDATE, DELETE, CREATE, etc.)
        else {
            $recordsAffected = $command.ExecuteNonQuery()
            return @{
                Type = "NonQuery"
                Data = $null
                RecordsAffected = $recordsAffected
            }
        }
    }
    catch {
        throw "Eroare la executarea comenzii: $($_.Exception.Message)"
    }
    finally {
        if ($connection.State -eq 'Open') {
            $connection.Close()
        }
    }
}

# Func?ie pentru formatarea rezultatelor ca text
function Format-AsText {
    param($DataTable)
    
    if ($null -eq $DataTable -or $DataTable.Rows.Count -eq 0) {
        return "Nu s-au g?sit rezultate."
    }
    
    # Calculeaz? l??imea maxim? pentru fiecare coloan?
    $columnWidths = @{}
    foreach ($column in $DataTable.Columns) {
        $maxWidth = $column.ColumnName.Length
        foreach ($row in $DataTable.Rows) {
            $value = if ($null -eq $row[$column.ColumnName]) { "" } else { $row[$column.ColumnName].ToString() }
            if ($value.Length -gt $maxWidth) {
                $maxWidth = $value.Length
            }
        }
        $columnWidths[$column.ColumnName] = [Math]::Min($maxWidth + 2, 50)
    }
    
    # Construie?te header
    $header = ""
    $separator = ""
    foreach ($column in $DataTable.Columns) {
        $width = $columnWidths[$column.ColumnName]
        $header += $column.ColumnName.PadRight($width)
        $separator += ("-" * ($width - 1)) + " "
    }
    
    # Construie?te output
    $output = @()
    $output += $header.TrimEnd()
    $output += $separator.TrimEnd()
    
    # Adaug? rândurile
    foreach ($row in $DataTable.Rows) {
        $line = ""
        foreach ($column in $DataTable.Columns) {
            $value = if ($null -eq $row[$column.ColumnName]) { "" } else { $row[$column.ColumnName].ToString() }
            if ($value.Length -gt 48) {
                $value = $value.Substring(0, 45) + "..."
            }
            $width = $columnWidths[$column.ColumnName]
            $line += $value.PadRight($width)
        }
        $output += $line.TrimEnd()
    }
    
    $output += ""
    $output += "Total: $($DataTable.Rows.Count) înregistr?ri"
    
    return $output -join "`n"
}

# Main execution
try {
    Write-Host "?? Analizare comand? SQL..." -ForegroundColor Yellow
    $commandType = Get-SqlCommandType -SqlQuery $Query
    Write-Host "Tip comand? detectat: $commandType" -ForegroundColor Cyan
    
    Write-Host "???  Verificare securitate..." -ForegroundColor Yellow
    $riskAnalysis = Test-DangerousCommand -SqlQuery $Query -CommandType $commandType
    
    if ($riskAnalysis.IsDangerous -and $ConfirmExecution) {
        $confirmed = Confirm-DangerousCommand -RiskAnalysis $riskAnalysis -Query $Query
        if (-not $confirmed) {
            Write-Host "? Execu?ie anulat? de utilizator." -ForegroundColor Red
            exit 0
        }
    }
    
    Write-Host "?? Conectare la baza de date..." -ForegroundColor Yellow
    $connectionString = Get-ConnectionString
    
    Write-Host "? Executare comand?..." -ForegroundColor Yellow
    $result = Invoke-DatabaseCommand -ConnectionString $connectionString -SqlQuery $Query -CommandType $commandType
    
    Write-Host "? Procesare rezultate..." -ForegroundColor Yellow
    
    if ($result.Type -eq "ResultSet") {
        # Formateaz? rezultatele pentru SELECT
        switch ($Format) {
            "Json" {
                if ($result.Data.Rows.Count -eq 0) {
                    $output = "[]"
                } else {
                    $jsonResults = @()
                    foreach ($row in $result.Data.Rows) {
                        $obj = @{}
                        foreach ($column in $result.Data.Columns) {
                            $value = $row[$column.ColumnName]
                            $obj[$column.ColumnName] = if ($null -eq $value -or [DBNull]::Value.Equals($value)) { $null } else { $value }
                        }
                        $jsonResults += $obj
                    }
                    $output = $jsonResults | ConvertTo-Json -Depth 10
                }
            }
            
            "Csv" {
                if ($result.Data.Rows.Count -eq 0) {
                    $output = "Nu s-au g?sit rezultate."
                } else {
                    $output = $result.Data | ConvertTo-Csv -NoTypeInformation
                }
            }
            
            default {
                $output = Format-AsText -DataTable $result.Data
            }
        }
    } else {
        # Pentru comenzi care nu returneaz? date
        $output = "? Comand? executat? cu succes!"
        if ($result.RecordsAffected -ge 0) {
            $output += "`nRânduri afectate: $($result.RecordsAffected)"
        }
        
        # Mesaje specifice pentru tipuri de comenzi
        switch ($commandType) {
            "CREATE" { $output += "`n???  Obiect creat în baza de date." }
            "ALTER" { $output += "`n?? Obiect modificat în baza de date." }
            "DROP" { $output += "`n???  Obiect ?ters din baza de date." }
            "INSERT" { $output += "`n? Date inserare în tabel?." }
            "UPDATE" { $output += "`n?? Date actualizate în tabel?." }
            "DELETE" { $output += "`n???  Date ?terse din tabel?." }
            "TRUNCATE" { $output += "`n?? Tabel? golit? complet." }
        }
    }
    
    Write-Host "`n?? Rezultate:" -ForegroundColor Green
    Write-Output $output
    
    # Log pentru opera?ii importante
    if ($commandType -ne "SELECT" -and $commandType -ne "CTE") {
        $logEntry = "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - $commandType - User: $env:USERNAME - Affected: $($result.RecordsAffected) rows"
        Write-Host "`n?? Log: $logEntry" -ForegroundColor Gray
    }
}
catch {
    Write-Host "? EROARE: $_" -ForegroundColor Red
    exit 1
}