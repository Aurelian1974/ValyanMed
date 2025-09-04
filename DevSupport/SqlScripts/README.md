# SQL Scripts Organization

Aceast? structur? organizat? con?ine toate scripturile SQL necesare pentru aplica?ia ValyanMed, organizate în categorii logice pentru o mai bun? mentenan?? ?i în?elegere.

## ?? Structura Directorelor

### ??? **Setup/**
Scripturile de configurare ini?ial? ?i setup-ul bazei de date:
- `00_MASTER_Setup_Partener.sql` - Setup pentru entitatea Partener
- `00_MASTER_Setup_Persoane.sql` - Setup pentru entitatea Persoane
- `Medical_AddMoreDepartments.sql` - Ad?ugare departamente medicale
- `scriptsforDepartment.sql` - Scripturi pentru configurarea departamentelor

### ??? **Tables/**
Scripturile pentru crearea ?i structura tabelelor:
- `Medical_Tables_Structure.sql` - Structura complet? a tabelelor medicale
- `01_CREATE_TABLE_Partener.sql` - Crearea tabelei Partener
- `CREATE_Pacienti_Table.sql` - Crearea tabelei Pacienti
- `Persoane_Table_Setup.sql` - Setup pentru tabela Persoane
- `Tabele_Utilizator_Persoana_Audit.sql` - Tabele pentru audit

### ?? **StoredProcedures/**
Toate procedurile stocate organizate pe func?ionalit??i:
- `Medical_StoredProcedures.sql` - Proceduri pentru entit??ile medicale
- `Persoane_StoredProcedures.sql` - Proceduri pentru entitatea Persoane
- `PersonalMedical_StoredProcedures_Complete.sql` - Proceduri pentru Personal Medical
- `Enhanced_Grouped_Procedures.sql` - Proceduri îmbun?t??ite grupate
- `Enhanced_Paginated_Grouped_Procedures.sql` - Proceduri cu paginare
- Proceduri specifice: `sp_*.sql` - Proceduri individuale

### ?? **Functions/**
Func?iile SQL pentru calcul?ri ?i valid?ri:
- `MedicalCalculations.sql` - Func?ii pentru calcul?ri medicale (IMC, vârst?, valid?ri CNP, etc.)

### ?? **SeedData/**
Scripturile pentru popularea ini?ial? cu date:
- `Medical_Data_Population.sql` - Date ini?iale pentru entit??ile medicale

### ?? **Migrations/**
Scripturile de migrare ?i reparare a datelor:
- `99_CleanupLegacyData.sql` - Cur??area datelor legacy
- `99_CleanupLegacyData_Advanced.sql` - Cur??are avansat?
- `Persoane_Repair_CNP_Issue.sql` - Repararea problemelor cu CNP

### ??? **Maintenance/**
Scripturile de mentenan?? ?i optimizare:
- `95_QuickFixTruncation.sql` - Fix rapid pentru probleme de trunchiere
- `96_CompleteFixTruncationError.sql` - Fix complet pentru erori de trunchiere
- `97_FixColumnSizes.sql` - Ajustarea dimensiunilor coloanelor
- `98_DiagnosticLegacyData.sql` - Diagnostic pentru date legacy

### ?? **Triggers/**
Triggere pentru audit ?i valid?ri:
- `Triggers_Audit_Utilizator.sql` - Triggere pentru auditarea utilizatorilor

### ??? **Views/**
Viewurile pentru raportare ?i afi?are:
- (În dezvoltare)

### ?? **Tests/**
Scripturile de testare ?i diagnostic:
- `Test_GUID_Insert.sql` - Test pentru inserturi cu GUID
- `Test_sp_CheckCNPExists.sql` - Test pentru verificarea existen?ei CNP
- `Simple_Test_Insert.sql` - Test simplu de inserare
- `Quick_Diagnostic_Persoane.sql` - Diagnostic rapid pentru Persoane
- `09_Analyze_Table_Structure.sql` - Analiz? structur? tabele

### ?? **Legacy/**
Scripturi vechi ?i de backup:
- `script.sql` - Script complet legacy (544KB)
- `SP.sql` - Proceduri stocate legacy (160KB)

## ?? Ordinea de Execu?ie Recomandat?

Pentru o instalare complet?, executa?i scripturile în urm?toarea ordine:

1. **Setup** - Configurarea ini?ial?
2. **Tables** - Crearea structurii
3. **Functions** - Func?iile de calcul
4. **StoredProcedures** - Procedurile stocate
5. **Triggers** - Triggerele pentru audit
6. **Views** - Viewurile pentru raportare
7. **SeedData** - Datele ini?iale
8. **Tests** - Scripturile de testare

## ?? Conven?ii de Denumire

- **Prefixe numerice**: Indic? ordinea de execu?ie (00_, 01_, etc.)
- **sp_**: Stored Procedures
- **fn_**: Functions
- **vw_**: Views
- **tr_**: Triggers

## ?? C?utare ?i Navigare

Folosi?i urm?toarele criterii pentru a g?si scripturile potrivite:
- **Func?ionalitate**: C?uta?i în directorul corespunz?tor tipului de script
- **Entitate**: C?uta?i dup? numele entit??ii (Partener, Persoane, Medical, etc.)
- **Ac?iune**: C?uta?i dup? ac?iunea necesar? (Create, Update, Delete, Get, etc.)

## ?? Not? pentru Dezvoltatori

Toate scripturile sunt configurate ca **EmbeddedResource** în proiectul DevSupport ?i pot fi accesate programatic din aplica?ia .NET pentru deployment automatizat.

---
*Ultima actualizare: 4 septembrie 2025*
*Autor: DevSupport Team*