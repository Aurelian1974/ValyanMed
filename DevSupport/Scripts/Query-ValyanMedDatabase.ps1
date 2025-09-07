<#
.SYNOPSIS
    Script PowerShell pentru interogarea bazei de date ValyanMed
.DESCRIPTION
    Permite executarea de query-uri SELECT simple ?i returneaz? rezultate în format text sau JSON
.PARAMETER Query
    Query-ul SQL de executat (doar SELECT permis)
.PARAMETER Format
    Formatul output-ului: Text, Json, Csv (default: Text)
.PARAMETER Server
    Numele serverului SQL (default: localhost)
.PARAMETER Database
    Numele bazei de date (default: ValyanMed)
.EXAMPLE
    .\Query-ValyanMedDatabase.ps1 -Query "SELECT TOP 10 * FROM Persoane"
.EXAMPLE
    .\Query-ValyanMedDatabase.ps1 -Query "SELECT Nume, Prenume FROM PersonalMedical WHERE EsteActiv = 1" -Format Json
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
    [string]$Password = ""
)

# Func?ie pentru validarea query-ului (doar SELECT permis)
function Test-SafeQuery {
    param([string]$SqlQuery)
    
    $normalizedQuery = $SqlQuery.Trim().ToUpper()
    
    # Verific? dac? începe cu SELECT
    if (-not $normalizedQuery.StartsWith("SELECT")) {
        throw "Doar query-uri SELECT sunt permise!"
    }
    
    # Verific? pentru cuvinte cheie periculoase
    $dangerousKeywords = @(
        "INSERT", "UPDATE", "DELETE", "DROP", "CREATE", "ALTER", 
        "EXEC", "EXECUTE", "TRUNCATE", "MERGE", "GRANT", "REVOKE"
    )
    
    foreach ($keyword in $dangerousKeywords) {
        if ($normalizedQuery -match "\b$keyword\b") {
            throw "Query-ul con?ine cuvinte cheie nepermise: $keyword"
        }
    }
    
    return $true
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
function Invoke-DatabaseQuery {
    param(
        [string]$ConnectionString,
        [string]$SqlQuery
    )
    
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $ConnectionString
    
    try {
        $connection.Open()
        
        $command = $connection.CreateCommand()
        $command.CommandText = $SqlQuery
        $command.CommandTimeout = 30
        
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
        $dataset = New-Object System.Data.DataSet
        
        $recordsAffected = $adapter.Fill($dataset)
        
        if ($dataset.Tables.Count -gt 0) {
            $table = $dataset.Tables[0]
            # Returneaz? explicit DataTable-ul
            return ,$table  # Virgula for?eaz? returnarea ca array cu un element
        } else {
            throw "Nu s-au returnat tabele în dataset"
        }
    }
    catch {
        throw "Eroare la executarea query-ului: $($_.Exception.Message)"
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
    Write-Host "Validare query..." -ForegroundColor Yellow
    Test-SafeQuery -SqlQuery $Query
    
    Write-Host "Conectare la baza de date..." -ForegroundColor Yellow
    $connectionString = Get-ConnectionString
    
    Write-Host "Executare query..." -ForegroundColor Yellow
    $results = Invoke-DatabaseQuery -ConnectionString $connectionString -SqlQuery $Query
    
    if ($null -eq $results) {
        Write-Host "AVERTIZARE: Rezultatele sunt NULL" -ForegroundColor Yellow
        $output = "Nu s-au g?sit rezultate."
    } else {
        # Asigur?-te c? ai DataTable-ul, nu primul rând
        if ($results -is [System.Data.DataRow]) {
            Write-Host "AVERTIZARE: Am primit DataRow în loc de DataTable" -ForegroundColor Yellow
            $output = "Eroare în procesarea rezultatelor"
        } elseif ($results -is [System.Data.DataTable]) {
            Write-Host "Procesare rezultate..." -ForegroundColor Yellow
            
            switch ($Format) {
                "Json" {
                    if ($results.Rows.Count -eq 0) {
                        $output = "[]"
                    } else {
                        $jsonResults = @()
                        foreach ($row in $results.Rows) {
                            $obj = @{}
                            foreach ($column in $results.Columns) {
                                $value = $row[$column.ColumnName]
                                $obj[$column.ColumnName] = if ($null -eq $value -or [DBNull]::Value.Equals($value)) { $null } else { $value }
                            }
                            $jsonResults += $obj
                        }
                        $output = $jsonResults | ConvertTo-Json -Depth 10
                    }
                }
                
                "Csv" {
                    if ($results.Rows.Count -eq 0) {
                        $output = "Nu s-au g?sit rezultate."
                    } else {
                        $output = $results | ConvertTo-Csv -NoTypeInformation
                    }
                }
                
                default {
                    $output = Format-AsText -DataTable $results
                }
            }
        } else {
            Write-Host "AVERTIZARE: Tip nea?teptat de rezultat: $($results.GetType().Name)" -ForegroundColor Yellow
            $output = "Tip nea?teptat de rezultat"
        }
    }
    
    Write-Host "`nRezultate:" -ForegroundColor Green
    Write-Output $output
}
catch {
    Write-Host "EROARE: $_" -ForegroundColor Red
    exit 1
}