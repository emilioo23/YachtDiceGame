USE YachtDiceDB;
GO

-- Agranda la columna Code de BackupCode a 64 caracteres.
-- Es necesario porque cada codigo de respaldo se guarda como hash SHA-256 en hexadecimal, que mide 64 caracteres.
-- Es seguro de correr varias veces: solo cambia la columna si todavia mide menos de 64 caracteres.
IF COL_LENGTH('dbo.BackupCode', 'Code') < 128
    ALTER TABLE dbo.BackupCode ALTER COLUMN Code NVARCHAR(64) NOT NULL;
GO

SELECT COL_LENGTH('dbo.BackupCode', 'Code') / 2 AS CodeMaxLength;
