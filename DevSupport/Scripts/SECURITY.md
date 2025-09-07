# ?? GHID DE SECURITATE - ValyanMed Database Scripts

## ?? ATEN?IE: RISC DE SECURITATE

Scripturile administrative din acest director au capacitatea de a **MODIFICA, ?TERGE sau DISTRUGE** date în baza de date ValyanMed. Utilizarea lor necorespunz?toare poate cauza **PIERDERI IREVERSIBILE DE DATE**.

## ?? Clasificarea scripturilor dup? nivelul de risc

### ?? **NIVEL VERDE - SIGUR (READ-ONLY)**
Aceste scripturi sunt **100% sigure** ?i pot fi utilizate f?r? risc:

- ? `Query-ValyanMedDatabase.ps1`
- ? `Quick-ValyanMedQuery.ps1` 
- ? `ValyanMed-QueryExamples.ps1`

**Nu pot:**
- Modifica structura bazei de date
- ?terge sau insera date
- Afecta securitatea sistemului

### ?? **NIVEL GALBEN - ADMINISTRARE CONTROLAT?**
Aceste scripturi pot modifica baza de date dar au **SISTEME DE PROTEC?IE**:

- ?? `Admin-ValyanMedDatabase.ps1` (cu `-ConfirmExecution`)
- ?? `Quick-AdminValyanMed.ps1` (template-uri)

**Sisteme de protec?ie activate:**
- Detectare automat? comenzi periculoase
- Confirm?ri obligatorii pentru risc ridicat
- Logging opera?ii administrative
- Timeout-uri pentru prevenirea bloc?rilor

### ?? **NIVEL RO?U - PERICOL MAXIM**
Aceste scripturi pot **DISTRUGE DATE IREVERSIBIL**:

- ?? `Admin-ValyanMedDatabase.ps1` (cu `-Force`)
- ?? `Deploy-SqlScripts.ps1` (cu `-Force`)
- ?? Orice script cu parametrul `-Force`

**?? DOAR PENTRU ADMINISTRATORI EXPERIMENTA?I!**

## ??? Reguli de securitate obligatorii

### ?? ÎNAINTE de orice opera?ie administrativ?:

1. **? BACKUP OBLIGATORIU**
   ```powershell
   .\Quick-AdminValyanMed.ps1 -Operation backup
   ```

2. **? TESTARE PE DEVELOPMENT**
   ```powershell
   # Testeaz? mai întâi pe dev environment
   .\Deploy-SqlScripts.ps1 -Category Tables -DryRun
   ```

3. **? VERIFICARE CON?INUT**
   ```powershell
   # Vezi ce vei executa
   Get-Content script.sql
   ```

4. **? CONFIRMARE ECHIP?**
   - Informeaz? echipa despre modific?ri
   - Prime?te aprobare pentru opera?ii critice
   - Documenteaz? schimb?rile

### ? INTERZIS ABSOLUT:

1. **? Nu folosi `-Force` pe Production**
2. **? Nu executa `DROP` f?r? backup**
3. **? Nu ignora confirm?rile de securitate**
4. **? Nu rula scripturi netestate**
5. **? Nu modifica date în timpul orelor de lucru** (f?r? coordonare)

## ?? Proceduri de urgen??

### În caz de eroare critic?:

1. **OPRE?TE IMEDIAT** execu?ia:
   ```powershell
   # Opre?te toate sesiunile active
   Ctrl+C
   ```

2. **VERIFIC? DAUNELE**:
   ```powershell
   .\Quick-ValyanMedQuery.ps1 -Type stats
   ```

3. **RESTAUREAZ? din BACKUP** dac? este necesar

4. **RAPORTEAZ? incidentul** echipei de administrare

## ?? Contacte de urgen??

- **DBA Principal**: aurelian.iancu@totalsoft.ro
- **DevOps Lead**: [contact-devops]
- **Team Lead**: [contact-team-lead]

## ?? Log-uri ?i audit

Toate opera?iile administrative sunt înregistrate automat:

```powershell
# Exemplu log entry:
2025-01-07 12:54:01 - CREATE - User: aurelian.iancu - Affected: -1 rows
2025-01-07 12:54:24 - DROP - User: aurelian.iancu - Affected: -1 rows
```

### Loca?ii log-uri:
- **PowerShell History**: `Get-History`
- **SQL Server Logs**: SQL Server Management Studio
- **Application Logs**: Event Viewer

## ?? Checklist de securitate

Înainte de a executa opera?ii administrative, verific?:

- [ ] Am backup recent al bazei de date?
- [ ] Am testat scriptul pe development?
- [ ] În?eleg complet ce face scriptul?
- [ ] Am informa?ia echipei despre modific?ri?
- [ ] Sunt în intervalul de timp aprobat pentru modific?ri?
- [ ] Am drepturile necesare pentru opera?ia respectiv??
- [ ] Am un plan de rollback în caz de probleme?

## ?? Exemple de utilizare sigur?

### ? Corect - Opera?ie controlat?:
```powershell
# 1. Backup
.\Quick-AdminValyanMed.ps1 -Operation backup

# 2. Test
.\Admin-ValyanMedDatabase.ps1 -Query "CREATE TABLE Test..." -ConfirmExecution

# 3. Verificare
.\Query-ValyanMedDatabase.ps1 -Query "SELECT name FROM sys.tables WHERE name = 'Test'"
```

### ? Incorect - Opera?ie periculoas?:
```powershell
# NU FACE ASTA!
.\Admin-ValyanMedDatabase.ps1 -Query "DROP DATABASE ValyanMed" -Force
```

## ?? Set?ri de securitate recomandate

### Restric?ii de acces:
```powershell
# Verific? drepturile utilizatorului curent
.\Query-ValyanMedDatabase.ps1 -Query "SELECT USER_NAME() as CurrentUser, IS_SRVROLEMEMBER('sysadmin') as IsSysAdmin"
```

### Parametri de siguran??:
- Folose?te întotdeauna `-ConfirmExecution` pentru opera?ii administrative
- Nu folosi niciodat? `-Force` pe production f?r? aprobare explicit?
- Seteaz? timeout-uri rezonabile pentru opera?ii lungi

## ?? Resurse suplimentare

- [Documenta?ie SQL Server Security](https://docs.microsoft.com/en-us/sql/relational-databases/security/)
- [PowerShell Execution Policies](https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_execution_policies)
- [Database Backup Best Practices](https://docs.microsoft.com/en-us/sql/relational-databases/backup-restore/)

---

**?? IMPORTANT: Orice înc?lcare a acestor reguli de securitate poate rezulta în pierderi de date ?i consecin?e grave pentru sistemul ValyanMed. Utilizeaz? aceste scripturi doar dac? în?elegi complet riscurile ?i consecin?ele.**

---
*Document creat: ianuarie 2025*  
*Autor: DevSupport Team*  
*Revizia: 1.0*