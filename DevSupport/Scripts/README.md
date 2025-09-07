# ValyanMed Database Management Scripts

Scripturi PowerShell pentru administrarea complet? a bazei de date ValyanMed - opera?iuni de citire ?i scriere.

## ?? Fi?iere disponibile

### 1. **Query-ValyanMedDatabase.ps1** - Script doar citire (READ-ONLY)
Script sigur pentru executarea query-urilor SELECT pe baza de date ValyanMed.

**Caracteristici:**
- ? **Securitate**: Doar query-uri SELECT sunt permise
- ? **Multiple formate**: Text, JSON, CSV  
- ? **Conexiune automat?**: Configurat? pentru serverul TS1828\ERP
- ? **Error handling**: Gestionare erori ?i valid?ri

### 2. **Admin-ValyanMedDatabase.ps1** - Script administrare complet? (FULL CONTROL) ??
Script administrativ pentru executarea tuturor tipurilor de comenzi SQL.

**Caracteristici:**
- ?? **Control complet**: CREATE, ALTER, DROP, INSERT, UPDATE, DELETE
- ? **Verificare risc**: Detecteaz? ?i confirm? comenzi periculoase
- ??? **Protec?ie**: Confirm?ri obligatorii pentru opera?ii cu risc ridicat
- ?? **Logging**: Înregistrare opera?ii administrative
- ?? **Timeout extins**: 60 secunde pentru opera?ii complexe

### 3. **Quick-ValyanMedQuery.ps1** - Query-uri rapide (READ-ONLY)
Shortcuts pentru cele mai folosite interog?ri din sistem.

### 4. **Quick-AdminValyanMed.ps1** - Opera?ii administrative rapide ??
Template-uri pentru crearea rapid? de tabele, SP, func?ii, view-uri.

### 5. **Deploy-SqlScripts.ps1** - Deployment automat ??
Execut? scripturile SQL din DevSupport\SqlScripts în ordinea corect?.

### 6. **ValyanMed-QueryExamples.ps1** - Exemple
Colec?ie de exemple de query-uri utile.

## ?? Utilizare rapid?

### ?? Query-uri de citire (SAFE)
```powershell
# Query basic
.\Query-ValyanMedDatabase.ps1 -Query "SELECT TOP 10 * FROM Persoane"

# Format JSON
.\Query-ValyanMedDatabase.ps1 -Query "SELECT Nume, Prenume FROM PersonalMedical WHERE EsteActiv = 1" -Format Json

# Query-uri rapide
.\Quick-ValyanMedQuery.ps1 -Type persoane -Search "Ion"
.\Quick-ValyanMedQuery.ps1 -Type stats
```

### ?? Opera?ii administrative (DANGEROUS)
```powershell
# Creare tabel?
.\Admin-ValyanMedDatabase.ps1 -Query "CREATE TABLE TestTable (Id INT IDENTITY(1,1), Nume NVARCHAR(100))" -ConfirmExecution

# Inserare date
.\Admin-ValyanMedDatabase.ps1 -Query "INSERT INTO Persoane (Nume, Prenume) VALUES ('Test', 'User')"

# Actualizare date
.\Admin-ValyanMedDatabase.ps1 -Query "UPDATE Persoane SET Email = 'new@email.com' WHERE Id = 1" -ConfirmExecution

# Opera?ii cu risc ridicat (necesit? confirmare)
.\Admin-ValyanMedDatabase.ps1 -Query "DROP TABLE TestTable" -ConfirmExecution

# For?are execu?ie f?r? confirmare
.\Admin-ValyanMedDatabase.ps1 -Query "DELETE FROM TestTable WHERE Id > 100" -Force
```

### ??? Template-uri administrative
```powershell
# Creare tabel? cu template standard
.\Quick-AdminValyanMed.ps1 -Operation create-table -Name "TabelNou"

# Creare procedur? stocat? CRUD
.\Quick-AdminValyanMed.ps1 -Operation create-sp -Name "sp_ManageTabel" -Parameters "TabelNou"

# Creare func?ie
.\Quick-AdminValyanMed.ps1 -Operation create-function -Name "fn_CalculateAge"

# Creare view
.\Quick-AdminValyanMed.ps1 -Operation create-view -Name "vw_ActiveUsers"

# Backup baza de date
.\Quick-AdminValyanMed.ps1 -Operation backup

# Mentenan?? automat?
.\Quick-AdminValyanMed.ps1 -Operation maintenance
```

### ?? Deployment scripturi SQL
```powershell
# Deployment complet (toate categoriile)
.\Deploy-SqlScripts.ps1 -Category All

# Deployment specific
.\Deploy-SqlScripts.ps1 -Category Tables
.\Deploy-SqlScripts.ps1 -Category StoredProcedures

# Dry run (simulare f?r? execu?ie)
.\Deploy-SqlScripts.ps1 -Category All -DryRun

# For?are în ciuda erorilor
.\Deploy-SqlScripts.ps1 -Category All -Force
````````

## ?? Niveluri de securitate

### ?? **NIVEL 1 - READ ONLY (SAFE)**
- `Query-ValyanMedDatabase.ps1` - Doar SELECT
- `Quick-ValyanMedQuery.ps1` - Query-uri predefinite
- `ValyanMed-QueryExamples.ps1` - Exemple

**Caracteristici:**
- ? 100% sigur pentru utilizare
- ? Nu poate modifica date
- ? Nu poate ?terge/crea obiecte
- ? Ideal pentru anali?ti, dezvoltatori junior

### ?? **NIVEL 2 - MODERATE ADMIN**
- `Admin-ValyanMedDatabase.ps1` - Toate tipurile de comenzi
- `Quick-AdminValyanMed.ps1` - Template-uri administrative

**Caracteristici:**
- ?? **Confirm?ri obligatorii** pentru opera?ii periculoase
- ?? **Detectare automat?** comenzi cu risc ridicat
- ?? **Protec?ie împotriva erorilor** comune
- ?? Recomandat pentru DBA ?i dezvoltatori senior

### ?? **NIVEL 3 - FULL ADMIN (DANGEROUS)**
- `Deploy-SqlScripts.ps1` - Deployment automat
- Parametrul `-Force` în scripturile admin

**Caracteristici:**
- ?? **Acces complet** la baza de date
- ?? **F?r? confirm?ri** cu `-Force`
- ?? **Poate distruge date** ireversibil
- ?? Doar pentru administratori experimenta?i

## ?? Sisteme de protec?ie

### ??? Detectare comenzi periculoase

Scriptul `Admin-ValyanMedDatabase.ps1` detecteaz? automat:

#### **RISC RIDICAT** (Confirma?ie obligatorie):
- `DROP DATABASE` - ?tergere baz? de date
- `DROP TABLE` - ?tergere tabel
- `TRUNCATE TABLE` - Golire tabel
- `DELETE FROM table` - ?tergere f?r? WHERE
- `UPDATE table SET` - Actualizare f?r? WHERE

#### **RISC MEDIU** (Avertizare):
- `CREATE TABLE` - Creare tabel
- `ALTER TABLE` - Modificare structur?
- `INSERT INTO` - Inserare date
- `UPDATE ... WHERE` - Actualizare cu condi?ii
- `DELETE ... WHERE` - ?tergere cu condi?ii

### ?? Exemple de protec?ie în ac?iune

```powershell
# Comand? periculoas? - va cere confirmarea
PS> .\Admin-ValyanMedDatabase.ps1 -Query "DROP TABLE TestTable" -ConfirmExecution

??  ATEN?IE: COMAND? PERICULOAS? DETECTAT?!
Nivel risc: HIGH
Detalii: Comand? cu risc ridicat detectat?: DROP\s+TABLE

Query de executat:
DROP TABLE TestTable

?? RISC RIDICAT: Aceast? comand? poate ?terge sau modifica date importante!
Asigur?-te c? ai backup ?i c? în?elegi consecin?ele!

E?ti sigur c? vrei s? continui? Tasteaz? 'DA' pentru confirmare: _
```

## ??? Structura bazei de date

### Tabele principale disponibile:
- **Persoane** - Date persoane fizice (10 înregistr?ri)
- **PersonalMedical** - Personal medical (1019 înregistr?ri)
- **Pacienti** - Pacien?i registra?i (10 înregistr?ri)
- **Utilizator** - Utilizatori sistem (3 înregistr?ri)
- **Departamente** - Departamente medicale (80 înregistr?ri)
- **DepartamenteIerarhie** - Ierarhia departamentelor
- **Medicament** - Medicamente disponibile
- **DispozitiveMedicale** - Echipamente medicale
- **MaterialeSanitare** - Materiale sanitare
- **Programari** - Program?ri pacien?i
- **Consultatii** - Consulta?ii medicale
- **Prescriptii** - Prescrip?ii medicale
- **RezultateTeste** - Rezultate teste medicale

### Structura departamentelor:
- **6 Categorii** (Chirurgicale, Medicale, Pediatrice, etc.)
- **53 Specialit??i** (Cardiologie, Neurologie, etc.) 
- **21 Subspecialit??i**

## ?? Configurare

### Server de baz? de date:
- **Server**: TS1828\ERP
- **Database**: ValyanMed  
- **Authentication**: Windows Integrated Security
- **Connection**: Trusted Connection with TrustServerCertificate=True

### Parametri disponibili:

#### Query-ValyanMedDatabase.ps1 (READ-ONLY)
- `-Query` (obligatoriu): Query-ul SQL de executat (doar SELECT)
- `-Format`: Text (default), Json, Csv
- `-Server`: Server baz? de date (default: TS1828\ERP)
- `-Database`: Nume baz? de date (default: ValyanMed)

#### Admin-ValyanMedDatabase.ps1 (FULL CONTROL)
- `-Query` (obligatoriu): Comanda SQL de executat
- `-Format`: Text (default), Json, Csv  
- `-ConfirmExecution`: Switch pentru confirmarea comenzilor periculoase
- `-Force`: Execu?ie f?r? confirm?ri (PERICULOS!)
- `-Server`: Server baz? de date (default: TS1828\ERP)
- `-Database`: Nume baz? de date (default: ValyanMed)

#### Quick-AdminValyanMed.ps1
- `-Operation` (obligatoriu): create-table, create-sp, create-function, create-view, backup, maintenance
- `-Name`: Numele obiectului de creat
- `-Parameters`: Parametri suplimentari
- `-Force`: Execu?ie f?r? confirm?ri

#### Deploy-SqlScripts.ps1
- `-Category`: All (default), Setup, Tables, Functions, StoredProcedures, Views, SeedData, Migrations
- `-Force`: Continu? în ciuda erorilor
- `-DryRun`: Simuleaz? f?r? execu?ie

## ?? Exemple practice cu rezultate

### 1. Template tabel standard
```powershell
PS> .\Quick-AdminValyanMed.ps1 -Operation create-table -Name "Exemple"

# Genereaz?:
CREATE TABLE [Exemple] (
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
-- + indexuri automate
```

### 2. Procedur? stocat? CRUD complet?
```powershell
PS> .\Quick-AdminValyanMed.ps1 -Operation create-sp -Name "sp_ManageExemple" -Parameters "Exemple"

# Genereaz? procedur? cu opera?ii:
# - SELECT (cu/f?r? ID)
# - INSERT (cu returnare ID nou)
# - UPDATE (cu audit trail)
# - DELETE (soft delete)
```

### 3. Deployment controlat
```powershell
PS> .\Deploy-SqlScripts.ps1 -Category Tables -DryRun

?? === CATEGORIA: Tables ===
?? [DRY RUN] Medical_Tables_Structure.sql
Con?inut:
CREATE TABLE [dbo].[MedicalRecords](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [PatientId] [uniqueidentifier] NOT NULL,
... (truncat)

? Succes: Medical_Tables_Structure.sql
?? G?site 5 scripturi în categoria 'Tables'
```

## ?? Depanare

### Erori comune pentru opera?ii administrative:

1. **"Object already exists"** - Obiectul exist? deja
   ```powershell
   # Verific? existen?a
   .\Query-ValyanMedDatabase.ps1 -Query "SELECT name FROM sys.tables WHERE name = 'NumeTabel'"
   
   # ?terge dac? este necesar
   .\Admin-ValyanMedDatabase.ps1 -Query "DROP TABLE NumeTabel" -ConfirmExecution
   ```

2. **"Permission denied"** - Drepturi insuficiente
   ```powershell
   # Verific? drepturile utilizatorului curent
   .\Query-ValyanMedDatabase.ps1 -Query "SELECT USER_NAME() as CurrentUser, IS_SRVROLEMEMBER('sysadmin') as IsSysAdmin"
   ```

3. **"Foreign key constraint"** - Constrângeri de integritate
   ```powershell
   # Vezi dependen?ele tabelei
   .\Query-ValyanMedDatabase.ps1 -Query "SELECT OBJECT_NAME(parent_object_id) as ParentTable, OBJECT_NAME(referenced_object_id) as ReferencedTable FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('NumeTabel')"
   ```

### Query-uri de diagnostic administrative:
```powershell
# Vezi toate obiectele create recent
.\Query-ValyanMedDatabase.ps1 -Query "SELECT name, type_desc, create_date FROM sys.objects WHERE create_date > DATEADD(day, -1, GETDATE()) ORDER BY create_date DESC"

# Verific? spa?iul ocupat de tabele
.\Query-ValyanMedDatabase.ps1 -Query "SELECT t.name AS TableName, s.used_page_count * 8 AS SizeKB FROM sys.dm_db_partition_stats s JOIN sys.tables t ON s.object_id = t.object_id WHERE s.index_id < 2 ORDER BY SizeKB DESC"

# Monitorizeaz? sesiunile active
.\Query-ValyanMedDatabase.ps1 -Query "SELECT session_id, login_name, program_name, host_name, login_time FROM sys.dm_exec_sessions WHERE is_user_process = 1"
```

## ?? Best Practices pentru administrare

### ? DO (Recomand?ri):
1. **Întotdeauna f? backup** înainte de opera?ii majore
2. **Testeaz? pe Development** înainte de Production  
3. **Folose?te `-DryRun`** pentru a verifica scripturile
4. **Confirm? opera?iile periculoase** cu `-ConfirmExecution`
5. **Verific? dependen?ele** înainte de ?tergeri
6. **Documenteaz? schimb?rile** importante

### ? DON'T (S? evi?i):
1. **Nu folosi `-Force`** decât dac? e?ti absolut sigur
2. **Nu rula `DROP` sau `DELETE`** f?r? backup
3. **Nu ignora confirm?rile** de securitate
4. **Nu executa scripturi neconfirmate** pe Production
5. **Nu uita s? verifici rezultatele** dup? execu?ie

### ?? Checklist pentru opera?ii critice:
```powershell
# 1. Backup
.\Quick-AdminValyanMed.ps1 -Operation backup

# 2. Verificare con?inut
.\Deploy-SqlScripts.ps1 -Category Tables -DryRun

# 3. Execu?ie controlat?
.\Deploy-SqlScripts.ps1 -Category Tables -ConfirmExecution

# 4. Verificare rezultate
.\Quick-ValyanMedQuery.ps1 -Type stats
```

## ?? Not? pentru dezvoltatori

**?? IMPORTANT:** Scripturile administrative au puterea de a modifica sau ?terge date ireversibil. Folose?te cu precau?ie ?i întotdeauna:

1. **F? backup** înainte de modific?ri majore
2. **Testeaz? mai întâi** cu `-DryRun`
3. **În?elege complet** ce face fiecare comand?
4. **Nu folosi `-Force`** decât în situa?ii de urgen??

Pentru opera?ii de rutin? ?i analize, folose?te întotdeauna scripturile READ-ONLY (`Query-ValyanMedDatabase.ps1`, `Quick-ValyanMedQuery.ps1`).