-- ===================================================================
-- POPULAREA CU DATE DUMMY PENTRU SISTEMUL MEDICAL
-- Conform planului de refactorizare - Date realiste pentru testing
-- ===================================================================

USE [ValyanMed]
GO

-- ===== POPULAREA PERSONALULUI MEDICAL =====

PRINT 'Popularea tabelei PersonalMedical cu date dummy...';

-- Verific? dac? exist? deja date ?i insereaz? doar ce lipse?te
IF NOT EXISTS (SELECT * FROM PersonalMedical WHERE PersonalID = '11111111-1111-1111-1111-111111111111')
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare)
    VALUES ('11111111-1111-1111-1111-111111111111', 'Popescu', 'Adrian', 'Cardiologie', 'DR001234', '0721234567', 'adrian.popescu@valyanmed.ro', 'Cardiologie', 'Doctor Primar', 1, GETDATE());

IF NOT EXISTS (SELECT * FROM PersonalMedical WHERE PersonalID = '22222222-2222-2222-2222-222222222222')
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare)
    VALUES ('22222222-2222-2222-2222-222222222222', 'Ionescu', 'Maria', 'Neurologie', 'DR001235', '0721234568', 'maria.ionescu@valyanmed.ro', 'Neurologie', 'Doctor Specialist', 1, GETDATE());

IF NOT EXISTS (SELECT * FROM PersonalMedical WHERE PersonalID = '33333333-3333-3333-3333-333333333333')
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare)
    VALUES ('33333333-3333-3333-3333-333333333333', 'Marinescu', 'Andrei', 'Ortopedie', 'DR001236', '0721234569', 'andrei.marinescu@valyanmed.ro', 'Ortopedie', 'Doctor Primar', 1, GETDATE());

IF NOT EXISTS (SELECT * FROM PersonalMedical WHERE PersonalID = '44444444-4444-4444-4444-444444444444')
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare)
    VALUES ('44444444-4444-4444-4444-444444444444', 'Georgescu', 'Ana', 'Pediatrie', 'DR001237', '0721234570', 'ana.georgescu@valyanmed.ro', 'Pediatrie', 'Doctor Specialist', 1, GETDATE());

IF NOT EXISTS (SELECT * FROM PersonalMedical WHERE PersonalID = '55555555-5555-5555-5555-555555555555')
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare)
    VALUES ('55555555-5555-5555-5555-555555555555', 'Dumitrescu', 'Mihai', 'Medicina Interna', 'DR001238', '0721234571', 'mihai.dumitrescu@valyanmed.ro', 'Medicina Interna', 'Doctor Primar', 1, GETDATE());

IF NOT EXISTS (SELECT * FROM PersonalMedical WHERE PersonalID = '66666666-6666-6666-6666-666666666666')
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare)
    VALUES ('66666666-6666-6666-6666-666666666666', 'Stanescu', 'Elena', 'Ginecologie', 'DR001239', '0721234572', 'elena.stanescu@valyanmed.ro', 'Ginecologie', 'Doctor Specialist', 1, GETDATE());

IF NOT EXISTS (SELECT * FROM PersonalMedical WHERE PersonalID = '77777777-7777-7777-7777-777777777777')
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare)
    VALUES ('77777777-7777-7777-7777-777777777777', 'Radu', 'Constantin', 'Chirurgie Generala', 'DR001240', '0721234573', 'constantin.radu@valyanmed.ro', 'Chirurgie', 'Doctor Primar', 1, GETDATE());

IF NOT EXISTS (SELECT * FROM PersonalMedical WHERE PersonalID = '88888888-8888-8888-8888-888888888888')
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare)
    VALUES ('88888888-8888-8888-8888-888888888888', 'Vasile', 'Ioana', 'Dermatologie', 'DR001241', '0721234574', 'ioana.vasile@valyanmed.ro', 'Dermatologie', 'Doctor Specialist', 1, GETDATE());

IF NOT EXISTS (SELECT * FROM PersonalMedical WHERE PersonalID = '99999999-9999-9999-9999-999999999999')
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare)
    VALUES ('99999999-9999-9999-9999-999999999999', 'Tudor', 'Cristina', 'Asistenta Medicala', 'AS001001', '0721234575', 'cristina.tudor@valyanmed.ro', 'Cardiologie', 'Asistent Medical Principal', 1, GETDATE());

IF NOT EXISTS (SELECT * FROM PersonalMedical WHERE PersonalID = 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA')
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare)
    VALUES ('AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 'Marin', 'Daniela', 'Asistenta Medicala', 'AS001002', '0721234576', 'daniela.marin@valyanmed.ro', 'Neurologie', 'Asistent Medical', 1, GETDATE());

IF NOT EXISTS (SELECT * FROM PersonalMedical WHERE PersonalID = 'BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB')
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare)
    VALUES ('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB', 'Popa', 'Alina', 'Asistenta Medicala', 'AS001003', '0721234577', 'alina.popa@valyanmed.ro', 'Pediatrie', 'Asistent Medical Principal', 1, GETDATE());

IF NOT EXISTS (SELECT * FROM PersonalMedical WHERE PersonalID = 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC')
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare)
    VALUES ('CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC', 'Diaconu', 'Roxana', 'Asistenta Medicala', 'AS001004', '0721234578', 'roxana.diaconu@valyanmed.ro', 'Chirurgie', 'Asistent Medical', 1, GETDATE());

IF NOT EXISTS (SELECT * FROM PersonalMedical WHERE PersonalID = 'DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD')
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare)
    VALUES ('DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD', 'Nicu', 'Gabriel', 'Tehnician Radiologie', 'TH001001', '0721234579', 'gabriel.nicu@valyanmed.ro', 'Imagistica', 'Tehnician Principal', 1, GETDATE());

IF NOT EXISTS (SELECT * FROM PersonalMedical WHERE PersonalID = 'EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE')
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare)
    VALUES ('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE', 'Mocanu', 'Florina', 'Tehnician Laborator', 'TH001002', '0721234580', 'florina.mocanu@valyanmed.ro', 'Laborator', 'Tehnician', 1, GETDATE());

IF NOT EXISTS (SELECT * FROM PersonalMedical WHERE PersonalID = 'FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF')
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare)
    VALUES ('FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF', 'Ciobanu', 'Diana', 'Receptioner', 'RC001001', '0721234581', 'diana.ciobanu@valyanmed.ro', 'Receptie', 'Receptioner Principal', 1, GETDATE());

IF NOT EXISTS (SELECT * FROM PersonalMedical WHERE PersonalID = '12345678-1234-1234-1234-123456789012')
    INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare)
    VALUES ('12345678-1234-1234-1234-123456789012', 'Matei', 'Laura', 'Receptioner', 'RC001002', '0721234582', 'laura.matei@valyanmed.ro', 'Receptie', 'Receptioner', 1, GETDATE());

PRINT 'PersonalMedical verificat ?i populat cu înregistr?rile necesare.';

-- ===== POPULAREA ROLURILOR SISTEM =====

PRINT 'Popularea tabelei RoluriSistem...';

IF NOT EXISTS (SELECT * FROM RoluriSistem)
BEGIN
    INSERT INTO RoluriSistem (RolID, NumeRol, Descriere, EsteActiv, DataCreare)
    VALUES
    (NEWID(), 'Administrator', 'Acces complet la toate func?ionalit??ile sistemului', 1, GETDATE()),
    (NEWID(), 'Doctor Primar', 'Acces complet la consulta?ii, diagnostice ?i prescrip?ii', 1, GETDATE()),
    (NEWID(), 'Doctor Specialist', 'Acces la consulta?ii ?i prescrip?ii în specializarea proprie', 1, GETDATE()),
    (NEWID(), 'Asistent Medical', 'Acces la triaj, semne vitale ?i asisten?? consulta?ii', 1, GETDATE()),
    (NEWID(), 'Tehnician', 'Acces la efectuarea ?i înregistrarea testelor de laborator/imagistic?', 1, GETDATE()),
    (NEWID(), 'Receptioner', 'Acces la program?ri, înregistrare pacien?i ?i informa?ii de baz?', 1, GETDATE()),
    (NEWID(), 'Manager Clinic', 'Acces la rapoarte ?i administrarea clinicsi', 1, GETDATE());
    
    PRINT 'RoluriSistem populat cu 7 roluri.';
END
ELSE
    PRINT 'RoluriSistem con?ine deja date.';

-- ===== POPULAREA UTILIZATORILOR SISTEM =====

PRINT 'Popularea tabelei UtilizatoriSistem...';

IF NOT EXISTS (SELECT * FROM UtilizatoriSistem)
BEGIN
    -- Hash pentru parola "Password123!" (va fi înlocuit în aplica?ie cu BCrypt)
    DECLARE @DefaultPasswordHash NVARCHAR(255) = '$2a$11$K2LetBnZ1ZIjLQGfOW1HUOqjJKixOjRUgIcTgQHT7EGE1AUJMGWnG';
    
    INSERT INTO UtilizatoriSistem (UtilizatorID, NumeUtilizator, HashParola, Email, PersonalID, EsteActiv, DataCreare)
    VALUES
    -- Administrator sistem
    (NEWID(), 'admin', @DefaultPasswordHash, 'admin@valyanmed.ro', NULL, 1, GETDATE()),
    
    -- Doctori
    (NEWID(), 'dr.popescu', @DefaultPasswordHash, 'adrian.popescu@valyanmed.ro', '11111111-1111-1111-1111-111111111111', 1, GETDATE()),
    (NEWID(), 'dr.ionescu', @DefaultPasswordHash, 'maria.ionescu@valyanmed.ro', '22222222-2222-2222-2222-222222222222', 1, GETDATE()),
    (NEWID(), 'dr.marinescu', @DefaultPasswordHash, 'andrei.marinescu@valyanmed.ro', '33333333-3333-3333-3333-333333333333', 1, GETDATE()),
    (NEWID(), 'dr.georgescu', @DefaultPasswordHash, 'ana.georgescu@valyanmed.ro', '44444444-4444-4444-4444-444444444444', 1, GETDATE()),
    (NEWID(), 'dr.dumitrescu', @DefaultPasswordHash, 'mihai.dumitrescu@valyanmed.ro', '55555555-5555-5555-5555-555555555555', 1, GETDATE()),
    (NEWID(), 'dr.stanescu', @DefaultPasswordHash, 'elena.stanescu@valyanmed.ro', '66666666-6666-6666-6666-666666666666', 1, GETDATE()),
    (NEWID(), 'dr.radu', @DefaultPasswordHash, 'constantin.radu@valyanmed.ro', '77777777-7777-7777-7777-777777777777', 1, GETDATE()),
    (NEWID(), 'dr.vasile', @DefaultPasswordHash, 'ioana.vasile@valyanmed.ro', '88888888-8888-8888-8888-888888888888', 1, GETDATE()),
    
    -- Asistente medicale
    (NEWID(), 'as.tudor', @DefaultPasswordHash, 'cristina.tudor@valyanmed.ro', '99999999-9999-9999-9999-999999999999', 1, GETDATE()),
    (NEWID(), 'as.marin', @DefaultPasswordHash, 'daniela.marin@valyanmed.ro', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 1, GETDATE()),
    (NEWID(), 'as.popa', @DefaultPasswordHash, 'alina.popa@valyanmed.ro', 'BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB', 1, GETDATE()),
    (NEWID(), 'as.diaconu', @DefaultPasswordHash, 'roxana.diaconu@valyanmed.ro', 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC', 1, GETDATE()),
    
    -- Tehnicieni
    (NEWID(), 'th.nicu', @DefaultPasswordHash, 'gabriel.nicu@valyanmed.ro', 'DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD', 1, GETDATE()),
    (NEWID(), 'th.mocanu', @DefaultPasswordHash, 'florina.mocanu@valyanmed.ro', 'EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE', 1, GETDATE()),
    
    -- Recep?ioneri
    (NEWID(), 'rc.ciobanu', @DefaultPasswordHash, 'diana.ciobanu@valyanmed.ro', 'FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF', 1, GETDATE()),
    (NEWID(), 'rc.matei', @DefaultPasswordHash, 'laura.matei@valyanmed.ro', '12345678-1234-1234-1234-123456789012', 1, GETDATE());
    
    PRINT 'UtilizatoriSistem populat cu 17 utilizatori.';
END
ELSE
    PRINT 'UtilizatoriSistem con?ine deja date.';

-- ===== ATRIBUIREA ROLURILOR =====

PRINT 'Atribuirea rolurilor utilizatorilor...';

IF NOT EXISTS (SELECT * FROM UtilizatorRoluri)
BEGIN
    -- Pentru atribuirea rolurilor, trebuie s? ob?inem ID-urile generate dinamic
    DECLARE @AdminRolID UNIQUEIDENTIFIER = (SELECT RolID FROM RoluriSistem WHERE NumeRol = 'Administrator');
    DECLARE @DoctorPrimarRolID UNIQUEIDENTIFIER = (SELECT RolID FROM RoluriSistem WHERE NumeRol = 'Doctor Primar');
    DECLARE @DoctorSpecialistRolID UNIQUEIDENTIFIER = (SELECT RolID FROM RoluriSistem WHERE NumeRol = 'Doctor Specialist');
    DECLARE @AsistentRolID UNIQUEIDENTIFIER = (SELECT RolID FROM RoluriSistem WHERE NumeRol = 'Asistent Medical');
    DECLARE @TehnicianRolID UNIQUEIDENTIFIER = (SELECT RolID FROM RoluriSistem WHERE NumeRol = 'Tehnician');
    DECLARE @ReceptionerRolID UNIQUEIDENTIFIER = (SELECT RolID FROM RoluriSistem WHERE NumeRol = 'Receptioner');
    
    -- Administrator
    DECLARE @AdminUserID UNIQUEIDENTIFIER = (SELECT UtilizatorID FROM UtilizatoriSistem WHERE NumeUtilizator = 'admin');
    
    INSERT INTO UtilizatorRoluri (UtilizatorRolID, UtilizatorID, RolID, DataAtribuire, AtribuitDe)
    VALUES (NEWID(), @AdminUserID, @AdminRolID, GETDATE(), NULL);
    
    -- Doctori Primari
    INSERT INTO UtilizatorRoluri (UtilizatorRolID, UtilizatorID, RolID, DataAtribuire, AtribuitDe)
    SELECT NEWID(), UtilizatorID, @DoctorPrimarRolID, GETDATE(), @AdminUserID
    FROM UtilizatoriSistem 
    WHERE NumeUtilizator IN ('dr.popescu', 'dr.marinescu', 'dr.dumitrescu', 'dr.radu');
    
    -- Doctori Speciali?ti
    INSERT INTO UtilizatorRoluri (UtilizatorRolID, UtilizatorID, RolID, DataAtribuire, AtribuitDe)
    SELECT NEWID(), UtilizatorID, @DoctorSpecialistRolID, GETDATE(), @AdminUserID
    FROM UtilizatoriSistem 
    WHERE NumeUtilizator IN ('dr.ionescu', 'dr.georgescu', 'dr.stanescu', 'dr.vasile');
    
    -- Asistente medicale
    INSERT INTO UtilizatorRoluri (UtilizatorRolID, UtilizatorID, RolID, DataAtribuire, AtribuitDe)
    SELECT NEWID(), UtilizatorID, @AsistentRolID, GETDATE(), @AdminUserID
    FROM UtilizatoriSistem 
    WHERE NumeUtilizator IN ('as.tudor', 'as.marin', 'as.popa', 'as.diaconu');
    
    -- Tehnicieni
    INSERT INTO UtilizatorRoluri (UtilizatorRolID, UtilizatorID, RolID, DataAtribuire, AtribuitDe)
    SELECT NEWID(), UtilizatorID, @TehnicianRolID, GETDATE(), @AdminUserID
    FROM UtilizatoriSistem 
    WHERE NumeUtilizator IN ('th.nicu', 'th.mocanu');
    
    -- Recep?ioneri
    INSERT INTO UtilizatorRoluri (UtilizatorRolID, UtilizatorID, RolID, DataAtribuire, AtribuitDe)
    SELECT NEWID(), UtilizatorID, @ReceptionerRolID, GETDATE(), @AdminUserID
    FROM UtilizatoriSistem 
    WHERE NumeUtilizator IN ('rc.ciobanu', 'rc.matei');
    
    PRINT 'Rolurile au fost atribuite cu succes.';
END
ELSE
    PRINT 'UtilizatorRoluri con?ine deja date.';

-- ===== POPULAREA CU PACIEN?I DUMMY =====

PRINT 'Popularea tabelei Pacienti cu date dummy...';

IF NOT EXISTS (SELECT * FROM Pacienti)
BEGIN
    INSERT INTO Pacienti (PacientID, Nume, Prenume, CNP, DataNasterii, Gen, Telefon, Email, Adresa, Oras, Judet, CodPostal, NumeContactUrgenta, TelefonContactUrgenta, FurnizorAsigurare, NumarAsigurare, DataCreare, EsteActiv)
    VALUES
    (NEWID(), 'Popa', 'Ion', '1800101123456', '1980-01-01', 'Masculin', '0721123456', 'ion.popa@email.ro', 'Str. Libertatii nr. 15', 'Bucuresti', 'Bucuresti', '010101', 'Popa Maria', '0721123457', 'CNAS', 'AS123456789', GETDATE(), 1),
    (NEWID(), 'Georgescu', 'Maria', '2851202234567', '1985-12-02', 'Feminin', '0721234567', 'maria.georgescu@email.ro', 'Bd. Unirii nr. 45', 'Cluj-Napoca', 'Cluj', '400001', 'Georgescu Andrei', '0721234568', 'Regina Maria', 'RM987654321', GETDATE(), 1),
    (NEWID(), 'Ionescu', 'Andrei', '1750615345678', '1975-06-15', 'Masculin', '0721345678', 'andrei.ionescu@email.ro', 'Str. Mihai Viteazu nr. 22', 'Timisoara', 'Timis', '300001', 'Ionescu Elena', '0721345679', 'CNAS', 'AS111222333', GETDATE(), 1),
    (NEWID(), 'Marinescu', 'Ana', '2901025456789', '1990-10-25', 'Feminin', '0721456789', 'ana.marinescu@email.ro', 'Aleea Rozelor nr. 8', 'Iasi', 'Iasi', '700001', 'Marinescu Mihai', '0721456790', 'Medicover', 'MC555666777', GETDATE(), 1),
    (NEWID(), 'Dumitrescu', 'George', '1820308567890', '1982-03-08', 'Masculin', '0721567890', 'george.dumitrescu@email.ro', 'Str. Florilor nr. 33', 'Constanta', 'Constanta', '900001', 'Dumitrescu Ioana', '0721567891', 'CNAS', 'AS222333444', GETDATE(), 1),
    (NEWID(), 'Vasilescu', 'Elena', '2870720678901', '1987-07-20', 'Feminin', '0721678901', 'elena.vasilescu@email.ro', 'Bd. Independentei nr. 67', 'Brasov', 'Brasov', '500001', 'Vasilescu Adrian', '0721678902', 'Regina Maria', 'RM444555666', GETDATE(), 1),
    (NEWID(), 'Radu', 'Cristian', '1920512789012', '1992-05-12', 'Masculin', '0721789012', 'cristian.radu@email.ro', 'Str. Pacii nr. 12', 'Craiova', 'Dolj', '200001', 'Radu Simona', '0721789013', 'CNAS', 'AS333444555', GETDATE(), 1),
    (NEWID(), 'Stoica', 'Ioana', '2941118890123', '1994-11-18', 'Feminin', '0721890123', 'ioana.stoica@email.ro', 'Aleea Primaverii nr. 5', 'Galati', 'Galati', '800001', 'Stoica Marius', '0721890124', 'Medicover', 'MC777888999', GETDATE(), 1),
    (NEWID(), 'Munteanu', 'Florin', '1780402901234', '1978-04-02', 'Masculin', '0721901234', 'florin.munteanu@email.ro', 'Str. Victoriei nr. 89', 'Ploiesti', 'Prahova', '100001', 'Munteanu Daniela', '0721901235', 'CNAS', 'AS444555666', GETDATE(), 1),
    (NEWID(), 'Constantin', 'Mihaela', '2830914012345', '1983-09-14', 'Feminin', '0721012345', 'mihaela.constantin@email.ro', 'Bd. Carol I nr. 156', 'Oradea', 'Bihor', '410001', 'Constantin Radu', '0721012346', 'Regina Maria', 'RM101112131', GETDATE(), 1);
    
    PRINT 'Pacienti populat cu 10 inregistrari dummy.';
END
ELSE
    PRINT 'Pacienti contine deja date.';

-- ===== POPULAREA TIPURILOR DE TESTE =====

PRINT 'Popularea tabelei TipuriTeste...';

IF NOT EXISTS (SELECT * FROM TipuriTeste)
BEGIN
    INSERT INTO TipuriTeste (TipTestID, NumeTest, Categorie, Departament, IntervalNormal, UnitateaMasura, EsteActiv)
    VALUES
    (NEWID(), 'Hemograma completa', 'Laborator', 'Laborator Analize', '4.5-11.0', '10^3/?L', 1),
    (NEWID(), 'Glicemia', 'Laborator', 'Laborator Analize', '70-100', 'mg/dL', 1),
    (NEWID(), 'Colesterol total', 'Laborator', 'Laborator Analize', '<200', 'mg/dL', 1),
    (NEWID(), 'TSH', 'Laborator', 'Laborator Analize', '0.4-4.0', 'mIU/L', 1),
    (NEWID(), 'Radiografie pulmonara', 'Imagistica', 'Radiologie', 'Normal', '', 1),
    (NEWID(), 'Ecografie abdominala', 'Imagistica', 'Radiologie', 'Normal', '', 1),
    (NEWID(), 'EKG', 'Functional', 'Cardiologie', 'Ritm sinusal normal', '', 1),
    (NEWID(), 'Spirometrie', 'Functional', 'Pneumologie', 'Normal', '', 1),
    (NEWID(), 'Creatinina', 'Laborator', 'Laborator Analize', '0.6-1.2', 'mg/dL', 1),
    (NEWID(), 'Uree', 'Laborator', 'Laborator Analize', '10-50', 'mg/dL', 1);
    
    PRINT 'TipuriTeste populat cu 10 tipuri de teste.';
END
ELSE
    PRINT 'TipuriTeste con?ine deja date.';

-- ===== POPULAREA MEDICAMENTELOR NOI =====

PRINT 'Popularea tabelei MedicamenteNoi...';

IF NOT EXISTS (SELECT * FROM MedicamenteNoi)
BEGIN
    INSERT INTO MedicamenteNoi (MedicamentID, NumeMedicament, NumeGeneric, Concentratie, Forma, Producator, EsteActiv)
    VALUES
    (NEWID(), 'Paracetamol Biofarm', 'Paracetamol', '500mg', 'Tableta', 'Biofarm', 1),
    (NEWID(), 'Ibuprofen Zentiva', 'Ibuprofen', '400mg', 'Tableta', 'Zentiva', 1),
    (NEWID(), 'Amoxicilina Sandoz', 'Amoxicilina', '500mg', 'Capsula', 'Sandoz', 1),
    (NEWID(), 'Metformina Teva', 'Metformina', '850mg', 'Tableta', 'Teva', 1),
    (NEWID(), 'Atorvastatina Pfizer', 'Atorvastatina', '20mg', 'Tableta', 'Pfizer', 1),
    (NEWID(), 'Losartan Gedeon Richter', 'Losartan', '50mg', 'Tableta', 'Gedeon Richter', 1),
    (NEWID(), 'Omeprazol Krka', 'Omeprazol', '20mg', 'Capsula', 'Krka', 1),
    (NEWID(), 'Levothyroxina Henning', 'Levothyroxina', '50mcg', 'Tableta', 'Henning', 1),
    (NEWID(), 'Aspenterica Zentiva', 'Aspirina', '75mg', 'Tableta', 'Zentiva', 1),
    (NEWID(), 'Vitamin D3 Biofarm', 'Colecalciferol', '1000UI', 'Tableta', 'Biofarm', 1);
    
    PRINT 'MedicamenteNoi populat cu 10 medicamente.';
END
ELSE
    PRINT 'MedicamenteNoi con?ine deja date.';

PRINT '';
PRINT '==============================';
PRINT 'POPULAREA CU DATE DUMMY COMPLET?!';
PRINT '==============================';
PRINT '';
PRINT 'DATE PENTRU LOGIN:';
PRINT '==================';
PRINT 'Administrator: admin / Password123!';
PRINT 'Doctor Primar: dr.popescu / Password123!';
PRINT 'Doctor Specialist: dr.ionescu / Password123!';
PRINT 'Asistent Medical: as.tudor / Password123!';
PRINT 'Receptioner: rc.ciobanu / Password123!';
PRINT '';
PRINT 'UTILIZATORI DISPONIBILI:';
PRINT '========================';
PRINT '• 17 utilizatori sistem cu roluri definite dinamic';
PRINT '• 16 membri personal medical';
PRINT '• 10 pacien?i dummy';
PRINT '• 10 tipuri de teste medicale';
PRINT '• 10 medicamente de baz?';
PRINT '• 7 roluri sistem pentru diferite pozi?ii medicale';
PRINT '';
PRINT 'STRUCTURA ROLURILOR:';
PRINT '====================';
PRINT '• Administrator - Acces complet sistem';
PRINT '• Doctor Primar - Acces complet medical (4 utilizatori)';
PRINT '• Doctor Specialist - Acces specializare proprie (4 utilizatori)';
PRINT '• Asistent Medical - Triaj ?i semne vitale (4 utilizatori)';
PRINT '• Tehnician - Laborator ?i imagistic? (2 utilizatori)';
PRINT '• Receptioner - Program?ri ?i înregistrare (2 utilizatori)';
PRINT '• Manager Clinic - Rapoarte ?i administrare (rol disponibil)';
PRINT '';
PRINT 'Urm?torul pas: Crearea procedurilor stocate ?i testarea aplica?iei Blazor.';