SELECT 
    pm.Nume + ' ' + pm.Prenume AS NumeComplet,
    pm.Departament AS DepartamentVechi,
    pm.Specializare AS SpecializareVeche,
    c.Nume AS CategorieNoua,
    s.Nume AS SpecializareNoua,
    sub.Nume AS SubspecializareNoua
FROM PersonalMedical pm
LEFT JOIN Departamente c ON pm.CategorieID = c.DepartamentID
LEFT JOIN Departamente s ON pm.SpecializareID = s.DepartamentID
LEFT JOIN Departamente sub ON pm.SubspecializareID = sub.DepartamentID
ORDER BY pm.Nume, pm.Prenume;

select * from PersonalMedical_Backup_Migration
select * from PersonalMedical