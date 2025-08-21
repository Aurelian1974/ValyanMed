/*
    Script to identify the correct table structure and create proper procedures
    Run this first to see the actual column names
*/

PRINT '=== Analyzing table structures ==='

-- Check Judet table structure
PRINT 'Judet table columns:'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Judet' 
  AND TABLE_SCHEMA = 'dbo'
ORDER BY ORDINAL_POSITION;

PRINT ''
PRINT 'Localitate table columns:'
-- Check Localitate table structure  
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Localitate' 
  AND TABLE_SCHEMA = 'dbo'
ORDER BY ORDINAL_POSITION;

-- Sample data from both tables
PRINT ''
PRINT 'Sample Judet data:'
SELECT TOP 3 * FROM dbo.Judet ORDER BY Nume;

PRINT ''
PRINT 'Sample Localitate data:'
SELECT TOP 3 * FROM dbo.Localitate ORDER BY Nume;