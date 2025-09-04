-- Script pentru ad?ugarea mai multor departamente în PersonalMedical
-- Ruleaz? acest script pentru a avea mai multe departamente pentru testarea grup?rii

-- Verific?m câte departamente avem
SELECT Departament, COUNT(*) as NumarPersonal 
FROM PersonalMedical 
GROUP BY Departament 
ORDER BY Departament;

-- Ad?ug?m personal în mai multe departamente dac? nu exist?
-- Neurologie
IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE Departament = 'Neurologie')
BEGIN
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Pozitie, Departament, Specializare, NumarLicenta, Email, Telefon, EsteActiv, DataCreare)
    VALUES 
        (NEWID(), 'Popescu', 'Maria', 'Doctor', 'Neurologie', 'Neurologie', 'LIC-NEU-001', 'maria.popescu@valyanmed.ro', '0751234567', 1, GETDATE()),
        (NEWID(), 'Ionescu', 'Alexandru', 'Asistent', 'Neurologie', 'Neurologie', 'LIC-NEU-002', 'alexandru.ionescu@valyanmed.ro', '0751234568', 1, GETDATE()),
        (NEWID(), 'Georgescu', 'Elena', 'Medic Specialist', 'Neurologie', 'Neurochirurgie', 'LIC-NEU-003', 'elena.georgescu@valyanmed.ro', '0751234569', 1, GETDATE());
END

-- Pediatrie
IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE Departament = 'Pediatrie')
BEGIN
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Pozitie, Departament, Specializare, NumarLicenta, Email, Telefon, EsteActiv, DataCreare)
    VALUES 
        (NEWID(), 'Marinescu', 'Ana', 'Doctor', 'Pediatrie', 'Pediatrie', 'LIC-PED-001', 'ana.marinescu@valyanmed.ro', '0751234570', 1, GETDATE()),
        (NEWID(), 'Constantinescu', 'Mihai', 'Asistent', 'Pediatrie', 'Pediatrie', 'LIC-PED-002', 'mihai.constantinescu@valyanmed.ro', '0751234571', 1, GETDATE()),
        (NEWID(), 'Stefanescu', 'Ioana', 'Medic Specialist', 'Pediatrie', 'Neonatologie', 'LIC-PED-003', 'ioana.stefanescu@valyanmed.ro', '0751234572', 1, GETDATE()),
        (NEWID(), 'Dumitrescu', 'Andrei', 'Asistent', 'Pediatrie', 'Pediatrie', 'LIC-PED-004', 'andrei.dumitrescu@valyanmed.ro', '0751234573', 1, GETDATE());
END

-- Chirurgie
IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE Departament = 'Chirurgie')
BEGIN
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Pozitie, Departament, Specializare, NumarLicenta, Email, Telefon, EsteActiv, DataCreare)
    VALUES 
        (NEWID(), 'Radulescu', 'Cristian', 'Medic Primar', 'Chirurgie', 'Chirurgie Generala', 'LIC-CHI-001', 'cristian.radulescu@valyanmed.ro', '0751234574', 1, GETDATE()),
        (NEWID(), 'Munteanu', 'Roxana', 'Doctor', 'Chirurgie', 'Chirurgie Vasculara', 'LIC-CHI-002', 'roxana.munteanu@valyanmed.ro', '0751234575', 1, GETDATE()),
        (NEWID(), 'Vasilescu', 'Gabriel', 'Asistent', 'Chirurgie', 'Chirurgie', 'LIC-CHI-003', 'gabriel.vasilescu@valyanmed.ro', '0751234576', 1, GETDATE()),
        (NEWID(), 'Nicolescu', 'Daniela', 'Instrumentar', 'Chirurgie', 'Chirurgie', 'LIC-CHI-004', 'daniela.nicolescu@valyanmed.ro', '0751234577', 1, GETDATE()),
        (NEWID(), 'Andreescu', 'Bogdan', 'Medic Specialist', 'Chirurgie', 'Chirurgie Ortopedica', 'LIC-CHI-005', 'bogdan.andreescu@valyanmed.ro', '0751234578', 1, GETDATE());
END

-- Ortopedie
IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE Departament = 'Ortopedie')
BEGIN
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Pozitie, Departament, Specializare, NumarLicenta, Email, Telefon, EsteActiv, DataCreare)
    VALUES 
        (NEWID(), 'Stanescu', 'Lucian', 'Medic Primar', 'Ortopedie', 'Ortopedie si Traumatologie', 'LIC-ORT-001', 'lucian.stanescu@valyanmed.ro', '0751234579', 1, GETDATE()),
        (NEWID(), 'Petrescu', 'Carmen', 'Doctor', 'Ortopedie', 'Ortopedie', 'LIC-ORT-002', 'carmen.petrescu@valyanmed.ro', '0751234580', 1, GETDATE()),
        (NEWID(), 'Florea', 'Radu', 'Asistent', 'Ortopedie', 'Recuperare Medicala', 'LIC-ORT-003', 'radu.florea@valyanmed.ro', '0751234581', 1, GETDATE());
END

-- ATI (Anestezie si Terapie Intensiva)
IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE Departament = 'ATI')
BEGIN
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Pozitie, Departament, Specializare, NumarLicenta, Email, Telefon, EsteActiv, DataCreare)
    VALUES 
        (NEWID(), 'Popa', 'Adrian', 'Medic Primar', 'ATI', 'Anestezie si Reanimare', 'LIC-ATI-001', 'adrian.popa@valyanmed.ro', '0751234582', 1, GETDATE()),
        (NEWID(), 'Stoica', 'Raluca', 'Doctor', 'ATI', 'Anestezie si Reanimare', 'LIC-ATI-002', 'raluca.stoica@valyanmed.ro', '0751234583', 1, GETDATE()),
        (NEWID(), 'Diaconu', 'Marius', 'Asistent', 'ATI', 'Anestezie si Reanimare', 'LIC-ATI-003', 'marius.diaconu@valyanmed.ro', '0751234584', 1, GETDATE()),
        (NEWID(), 'Badea', 'Corina', 'Asistent', 'ATI', 'Anestezie si Reanimare', 'LIC-ATI-004', 'corina.badea@valyanmed.ro', '0751234585', 1, GETDATE());
END

-- Ginecologie
IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE Departament = 'Ginecologie')
BEGIN
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Pozitie, Departament, Specializare, NumarLicenta, Email, Telefon, EsteActiv, DataCreare)
    VALUES 
        (NEWID(), 'Manole', 'Silvia', 'Medic Primar', 'Ginecologie', 'Obstetrica-Ginecologie', 'LIC-GIN-001', 'silvia.manole@valyanmed.ro', '0751234586', 1, GETDATE()),
        (NEWID(), 'Tudor', 'Dan', 'Doctor', 'Ginecologie', 'Obstetrica-Ginecologie', 'LIC-GIN-002', 'dan.tudor@valyanmed.ro', '0751234587', 1, GETDATE()),
        (NEWID(), 'Ilie', 'Monica', 'Moasa', 'Ginecologie', 'Obstetrica-Ginecologie', 'LIC-GIN-003', 'monica.ilie@valyanmed.ro', '0751234588', 1, GETDATE()),
        (NEWID(), 'Serban', 'Catalina', 'Asistent', 'Ginecologie', 'Obstetrica-Ginecologie', 'LIC-GIN-004', 'catalina.serban@valyanmed.ro', '0751234589', 1, GETDATE());
END

-- Oftalmologie
IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE Departament = 'Oftalmologie')
BEGIN
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Pozitie, Departament, Specializare, NumarLicenta, Email, Telefon, EsteActiv, DataCreare)
    VALUES 
        (NEWID(), 'Preda', 'Catalin', 'Medic Specialist', 'Oftalmologie', 'Oftalmologie', 'LIC-OFT-001', 'catalin.preda@valyanmed.ro', '0751234590', 1, GETDATE()),
        (NEWID(), 'Neagu', 'Adriana', 'Doctor', 'Oftalmologie', 'Oftalmologie', 'LIC-OFT-002', 'adriana.neagu@valyanmed.ro', '0751234591', 1, GETDATE()),
        (NEWID(), 'Lazar', 'Oana', 'Optometrist', 'Oftalmologie', 'Oftalmologie', 'LIC-OFT-003', 'oana.lazar@valyanmed.ro', '0751234592', 1, GETDATE());
END

-- Radiologie
IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE Departament = 'Radiologie')
BEGIN
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Pozitie, Departament, Specializare, NumarLicenta, Email, Telefon, EsteActiv, DataCreare)
    VALUES 
        (NEWID(), 'Cristea', 'Bogdan', 'Medic Specialist', 'Radiologie', 'Radiologie si Imagistica', 'LIC-RAD-001', 'bogdan.cristea@valyanmed.ro', '0751234593', 1, GETDATE()),
        (NEWID(), 'Enache', 'Laura', 'Tehnician Radiolog', 'Radiologie', 'Radiologie', 'LIC-RAD-002', 'laura.enache@valyanmed.ro', '0751234594', 1, GETDATE()),
        (NEWID(), 'Mocanu', 'Florin', 'Asistent', 'Radiologie', 'Radiologie', 'LIC-RAD-003', 'florin.mocanu@valyanmed.ro', '0751234595', 1, GETDATE());
END

-- Laborator
IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE Departament = 'Laborator')
BEGIN
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Pozitie, Departament, Specializare, NumarLicenta, Email, Telefon, EsteActiv, DataCreare)
    VALUES 
        (NEWID(), 'Mihai', 'Diana', 'Biolog', 'Laborator', 'Analize Medicale', 'LIC-LAB-001', 'diana.mihai@valyanmed.ro', '0751234596', 1, GETDATE()),
        (NEWID(), 'Radu', 'Sorin', 'Chimist', 'Laborator', 'Analize Medicale', 'LIC-LAB-002', 'sorin.radu@valyanmed.ro', '0751234597', 1, GETDATE()),
        (NEWID(), 'Zamfir', 'Alina', 'Tehnician Laborator', 'Laborator', 'Analize Medicale', 'LIC-LAB-003', 'alina.zamfir@valyanmed.ro', '0751234598', 1, GETDATE()),
        (NEWID(), 'Ciobanu', 'Nicolae', 'Asistent Laborator', 'Laborator', 'Analize Medicale', 'LIC-LAB-004', 'nicolae.ciobanu@valyanmed.ro', '0751234599', 1, GETDATE());
END

-- Farmacie
IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE Departament = 'Farmacie')
BEGIN
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Pozitie, Departament, Specializare, NumarLicenta, Email, Telefon, EsteActiv, DataCreare)
    VALUES 
        (NEWID(), 'Barbu', 'Elisabeta', 'Farmacist Sef', 'Farmacie', 'Farmacie', 'LIC-FAR-001', 'elisabeta.barbu@valyanmed.ro', '0751234600', 1, GETDATE()),
        (NEWID(), 'Rusu', 'Alin', 'Farmacist', 'Farmacie', 'Farmacie', 'LIC-FAR-002', 'alin.rusu@valyanmed.ro', '0751234601', 1, GETDATE()),
        (NEWID(), 'Balan', 'Gabriela', 'Asistent Farmacie', 'Farmacie', 'Farmacie', 'LIC-FAR-003', 'gabriela.balan@valyanmed.ro', '0751234602', 1, GETDATE());
END

-- Verific?m din nou departamentele dup? inserare
SELECT Departament, COUNT(*) as NumarPersonal 
FROM PersonalMedical 
GROUP BY Departament 
ORDER BY Departament;

PRINT 'Au fost ad?ugate mai multe departamente în PersonalMedical pentru testarea grup?rii.';