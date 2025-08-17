-- Normalize Romanian diacritics for key string columns in Medicament table
-- This script will update values by removing diacritics using a CLR-independent approach.
-- NOTE: Backup your data first. Review a sample before applying to production.

SET NOCOUNT ON;

-- Helper inline function to remove diacritics using TRANSLATE for common Romanian characters
-- (limited vs. CLR approach but sufficient for most use cases)

-- Replace known Romanian diacritics
UPDATE m SET
    Nume = TRANSLATE(Nume, N'??????����', N'tTsSaAaAiI'),
    DenumireComunaInternationala = TRANSLATE(DenumireComunaInternationala, N'??????����', N'tTsSaAaAiI'),
    Concentratie = TRANSLATE(Concentratie, N'??????����', N'tTsSaAaAiI'),
    FormaFarmaceutica = TRANSLATE(FormaFarmaceutica, N'??????����', N'tTsSaAaAiI'),
    Producator = TRANSLATE(Producator, N'??????����', N'tTsSaAaAiI'),
    Ambalaj = TRANSLATE(Ambalaj, N'??????����', N'tTsSaAaAiI')
FROM dbo.Medicament m;

-- Optionally normalize other text columns as well (Prospect/Contraindicatii/Interactiuni/Observatii)
-- Uncomment if desired
-- UPDATE dbo.Medicament
-- SET Prospect = TRANSLATE(Prospect, N'??????����', N'tTsSaAaAiI'),
--     Contraindicatii = TRANSLATE(Contraindicatii, N'??????����', N'tTsSaAaAiI'),
--     Interactiuni = TRANSLATE(Interactiuni, N'??????����', N'tTsSaAaAiI'),
--     Observatii = TRANSLATE(Observatii, N'??????����', N'tTsSaAaAiI');
