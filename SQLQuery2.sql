CREATE TABLE Departamente (
    DepartamentID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY, -- GUID
    DepartamentCode INT IDENTITY(1,1) NOT NULL,                                   -- cod numeric auto
    Categorie NVARCHAR(100) NOT NULL,        -- ex: Medicale, Chirurgicale, Pediatrice etc.
    Specialitate NVARCHAR(200) NOT NULL,     -- ex: Medicina interna, Chirurgie generala
    Subspecialitate NVARCHAR(200) NULL       -- ex: Cardiologie, Gastroenterologie
);

delete from Departamente
delete from DepartamenteIerarhie
