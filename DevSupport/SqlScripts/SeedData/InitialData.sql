-- Script pentru popularea tabelei de jude?e din România
-- Data: 2025-01-04
-- Surs?: Codurile oficiale jude?e România

-- Popul?m tabela Judet cu toate jude?ele României
INSERT INTO [Judet] ([CodJudet], [Nume], [Siruta], [CodAuto], [Ordine]) VALUES
(N'1', N'Alba', 10, N'AB', 1),
(N'2', N'Arad', 29, N'AR', 2),
(N'3', N'Arges', 38, N'AG', 3),
(N'4', N'Bacau', 47, N'BC', 4),
(N'5', N'Bihor', 56, N'BH', 5),
(N'6', N'Bistrita-Nasaud', 65, N'BN', 6),
(N'7', N'Botosani', 74, N'BT', 7),
(N'8', N'Brasov', 83, N'BV', 8),
(N'9', N'Braila', 92, N'BR', 9),
(N'10', N'Buzau', 109, N'BZ', 10),
(N'11', N'Caras-Severin', 118, N'CS', 11),
(N'51', N'Calarasi', 519, N'CL', 12),
(N'12', N'Cluj', 127, N'CJ', 13),
(N'13', N'Constanta', 136, N'CT', 14),
(N'14', N'Covasna', 145, N'CV', 15),
(N'15', N'Dâmbovita', 154, N'DB', 16),
(N'16', N'Dolj', 163, N'DJ', 17),
(N'17', N'Galati', 172, N'GL', 18),
(N'52', N'Giurgiu', 528, N'GR', 19),
(N'18', N'Gorj', 181, N'GJ', 20),
(N'19', N'Harghita', 190, N'HR', 21),
(N'20', N'Hunedoara', 207, N'HD', 22),
(N'21', N'Ialomita', 216, N'IL', 23),
(N'22', N'Iasi', 225, N'IS', 24),
(N'23', N'Ilfov', 234, N'IF', 25),
(N'24', N'Maramures', 243, N'MM', 26),
(N'25', N'Mehedinti', 252, N'MH', 27),
(N'26', N'Mures', 261, N'MS', 28),
(N'27', N'Neamt', 270, N'NT', 29),
(N'28', N'Olt', 289, N'OT', 30),
(N'29', N'Prahova', 298, N'PH', 31),
(N'30', N'Satu Mare', 305, N'SM', 32),
(N'31', N'Salaj', 314, N'SJ', 33),
(N'32', N'Sibiu', 323, N'SB', 34),
(N'33', N'Suceava', 332, N'SV', 35),
(N'34', N'Teleorman', 341, N'TR', 36),
(N'35', N'Timis', 350, N'TM', 37),
(N'36', N'Tulcea', 369, N'TL', 38),
(N'37', N'Vaslui', 378, N'VS', 39),
(N'38', N'Vâlcea', 387, N'VL', 40),
(N'39', N'Vrancea', 396, N'VN', 41),
(N'40', N'Municipiul Bucuresti', 179132, N'B', 42);

-- Popul?m tipurile de localit??i
INSERT INTO [TipLocalitate] ([CodTipLocalitate], [DenumireTipLocalitate]) VALUES
(N'Loc', N'LOCALITATE'),
(N'Mun', N'MUNICIPIU'),
(N'Ors', N'ORAS'),
(N'Sat', N'SAT'),
(N'Sec', N'SECTOR'),
(N'Com', N'COMUNA'),
(N'MRJ', N'Municipiu Resedinta de Judet');

-- Date ini?iale pentru utilizatori de test
INSERT INTO [Persoana] ([Nume], [Prenume], [Judet], [Localitate], [Strada], [NumarStrada], [CodPostal], [PozitieOrganizatie], [DataNasterii], [CNP], [TipActIdentitate], [SerieActIdentitate], [NumarActIdentitate], [StareCivila], [Gen]) VALUES
(N'Popescu', N'Ion', N'Bucuresti', N'Sector 1', N'Calea Victoriei', N'100', N'010071', N'Administrator', CAST(N'1980-01-15' AS Date), N'1800115401234', N'CI', N'AB', N'123456', N'Casatorit', N'Masculin'),
(N'Ionescu', N'Maria', N'Cluj', N'Cluj-Napoca', N'Str. Republicii', N'25', N'400015', N'Medic', CAST(N'1985-03-20' AS Date), N'2850320071234', N'CI', N'CJ', N'789012', N'Casatorit', N'Feminin');

-- Date ini?iale pentru medicamente comune
INSERT INTO [Medicament] ([Nume], [DenumireComunaInternationala], [Concentratie], [FormaFarmaceutica], [Producator], [CodATC], [Status], [DataExpirare], [Pret], [Stoc], [StocSiguranta], [Activ]) VALUES
(N'Paracetamol', N'Paracetamolum', N'500 mg', N'Tablete', N'Terapia', N'N02BE01', N'Activ', CAST(N'2026-12-31' AS Date), 5.50, 1000, 100, 1),
(N'Ibuprofen', N'Ibuprofenum', N'400 mg', N'Tablete', N'Zentiva', N'M01AE01', N'Activ', CAST(N'2026-11-30' AS Date), 8.75, 500, 50, 1),
(N'Aspirina', N'Acidum acetylsalicylicum', N'500 mg', N'Tablete', N'Bayer', N'N02BA01', N'Activ', CAST(N'2026-10-31' AS Date), 12.00, 300, 30, 1);

-- Date ini?iale pentru materiale sanitare
INSERT INTO [MaterialeSanitare] ([Denumire], [Categorie], [Specificatii], [UnitateaMasura], [Sterile], [UniUzinta]) VALUES
(N'Seringi 10ml', N'Administrare', N'Seringi de unica folosinta 10ml cu ac', N'bucata', 1, 1),
(N'Comprese sterile', N'Pansamente', N'Comprese sterile 10x10cm', N'pachet/10buc', 1, 1),
(N'Manusi examinare', N'Protectie', N'Manusi latex nepudrate', N'cutie/100buc', 0, 1);

-- Date ini?iale pentru dispozitive medicale
INSERT INTO [DispozitiveMedicale] ([Denumire], [Categorie], [ClasaRisc], [Producator], [ModelTip], [CertificareCE], [Specificatii]) VALUES
(N'Tensiometru digital', N'Monitorizare', N'IIa', N'Omron', N'M6-AC', 1, N'Tensiometru automat pentru bra?'),
(N'Stetoscop', N'Diagnostic', N'I', N'Littmann', N'Classic III', 1, N'Stetoscop pentru auscultatie'),
(N'Termometru infrarosu', N'Masurare', N'IIa', N'Braun', N'ThermoScan 7', 1, N'Termometru timpanic digital');

-- Date pentru parteneri de test
INSERT INTO [Partener] ([CodIntern], [Denumire], [CodFiscal], [Judet], [Localitate], [Adresa], [UtilizatorCreare]) VALUES
(N'PART001', N'Farmacia Centrala SRL', N'RO12345678', N'Bucuresti', N'Sector 1', N'Bd. Carol I, nr. 15', N'admin'),
(N'PART002', N'Cabinet Medical Dr. Popescu', N'RO87654321', N'Cluj', N'Cluj-Napoca', N'Str. Memorandumului, nr. 8', N'admin');

-- Date ini?iale pentru departamente ?i pozi?ii
-- Inserare departamente standard

-- Inserare utilizatori de test
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, IsActive)
VALUES 
    ('admin@valyanmed.ro', '$2a$11$Wl7Z3KZiP3Q7YXN8zKvZo.sHrS/5L5bxRKvQp4X3T9Y8A4U2P7J1e', 'Administrator', 'Sistem', 'Admin', 1),
    ('doctor@valyanmed.ro', '$2a$11$Wl7Z3KZiP3Q7YXN8zKvZo.sHrS/5L5bxRKvQp4X3T9Y8A4U2P7J1e', 'Ion', 'Popescu', 'Doctor', 1),
    ('receptioner@valyanmed.ro', '$2a$11$Wl7Z3KZiP3Q7YXN8zKvZo.sHrS/5L5bxRKvQp4X3T9Y8A4U2P7J1e', 'Maria', 'Ionescu', 'Receptioner', 1);

-- Inserare personal medical de test
INSERT INTO MedicalStaff (FirstName, LastName, Position, Specialization, Department, LicenseNumber, Phone, Email, IsActive)
VALUES 
    ('Ion', 'Popescu', 'Doctor Primar', 'Cardiologie', 'Cardiologie', 'MED001234', '0721234567', 'ion.popescu@valyanmed.ro', 1),
    ('Ana', 'Marinescu', 'Doctor Specialist', 'Neurologie', 'Neurologie', 'MED001235', '0721234568', 'ana.marinescu@valyanmed.ro', 1),
    ('Mihai', 'Georgescu', 'Doctor', 'Pediatrie', 'Pediatrie', 'MED001236', '0721234569', 'mihai.georgescu@valyanmed.ro', 1),
    ('Elena', 'Vasilescu', 'Asistent Medical', '', 'Cardiologie', 'ASM001237', '0721234570', 'elena.vasilescu@valyanmed.ro', 1),
    ('Cristina', 'Munteanu', 'Asistent Medical', '', 'Neurologie', 'ASM001238', '0721234571', 'cristina.munteanu@valyanmed.ro', 1),
    ('Alexandru', 'Radu', 'Tehnician Medical', 'Radiologie', 'Radiologie', 'TEC001239', '0721234572', 'alexandru.radu@valyanmed.ro', 1),
    ('Diana', 'Stoica', 'Kinetoterapeut', 'Recuperare', 'Recuperare', 'KIN001240', '0721234573', 'diana.stoica@valyanmed.ro', 1),
    ('Gabriel', 'Constantin', 'Psiholog', 'Psihologie clinic?', 'Psihologie', 'PSI001241', '0721234574', 'gabriel.constantin@valyanmed.ro', 1),
    ('Laura', 'Dumitrescu', 'Nutritionist', 'Nutri?ie clinic?', 'Nutri?ie', 'NUT001242', '0721234575', 'laura.dumitrescu@valyanmed.ro', 1),
    ('Maria', 'Ionescu', 'Receptioner Principal', '', 'Receptie', '', '0721234576', 'maria.ionescu@valyanmed.ro', 1);

-- Inserare pacien?i de test
INSERT INTO Patients (CNP, FirstName, LastName, DateOfBirth, Gender, Phone, Email, Address)
VALUES 
    ('1850203123456', 'Gheorghe', 'Enescu', '1985-02-03', 'Masculin', '0731111111', 'gheorghe.enescu@email.ro', 'Str. Muzicii nr. 1, Bucure?ti'),
    ('2901215234567', 'Maria', 'Curie', '1990-12-15', 'Feminin', '0732222222', 'maria.curie@email.ro', 'Str. ?tiin?ei nr. 2, Bucure?ti'),
    ('1751010345678', 'Nicolae', 'Tesla', '1975-10-10', 'Masculin', '0733333333', 'nicolae.tesla@email.ro', 'Str. Inventatorilor nr. 3, Bucure?ti'),
    ('2830605456789', 'Ana', 'Pauker', '1983-06-05', 'Feminin', '0734444444', 'ana.pauker@email.ro', 'Str. Politicii nr. 4, Bucure?ti'),
    ('1920918567890', 'Mihai', 'Eminescu', '1992-09-18', 'Masculin', '0735555555', 'mihai.eminescu@email.ro', 'Str. Literaturii nr. 5, Bucure?ti');

-- Password hash de mai sus reprezint? "ValyanMed2025!" - va trebui actualizat cu hash-ul real