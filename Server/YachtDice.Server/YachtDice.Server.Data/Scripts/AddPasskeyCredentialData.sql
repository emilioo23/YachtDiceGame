USE YachtDiceDB;
GO

-- Agrega a la tabla Passkey las columnas CredentialId y SignCount.
-- CredentialId guarda el identificador de la credencial (en base64url) con el que el servidor encuentra la llave publica
-- cuando el jugador inicia sesion con su passkey. SignCount guarda el contador de firmas que reporta el dispositivo,
-- y sirve para detectar una passkey clonada.
-- Es seguro de correr varias veces: solo agrega lo que todavia no existe.
IF COL_LENGTH('dbo.Passkey', 'CredentialId') IS NULL
BEGIN
    ALTER TABLE dbo.Passkey ADD CredentialId NVARCHAR(400) NULL;
END
GO

IF COL_LENGTH('dbo.Passkey', 'SignCount') IS NULL
BEGIN
    ALTER TABLE dbo.Passkey ADD SignCount BIGINT NOT NULL CONSTRAINT DF_Passkey_SignCount DEFAULT 0;
END
GO

-- Las filas que ya existian se guardaron antes de que la tabla tuviera CredentialId, asi que nunca pudieron usarse
-- para iniciar sesion (el codigo del servidor todavia no leia esta tabla). Se borran para poder exigir CredentialId.
DELETE FROM dbo.Passkey WHERE CredentialId IS NULL;
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Passkey') AND name = 'CredentialId' AND is_nullable = 1)
BEGIN
    ALTER TABLE dbo.Passkey ALTER COLUMN CredentialId NVARCHAR(400) NOT NULL;
END
GO

-- Dos jugadores no pueden tener la misma credencial.
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.Passkey') AND name = 'UQ_Passkey_CredentialId')
BEGIN
    CREATE UNIQUE INDEX UQ_Passkey_CredentialId ON dbo.Passkey (CredentialId);
END
GO

-- Comprobacion: deben aparecer las dos columnas (CredentialId de 400 caracteres y SignCount) y el indice unico.
SELECT name AS ColumnName, TYPE_NAME(user_type_id) AS DataType, max_length / 2 AS MaxLength
FROM sys.columns
WHERE object_id = OBJECT_ID('dbo.Passkey') AND name IN ('CredentialId', 'SignCount');

SELECT name AS IndexName FROM sys.indexes
WHERE object_id = OBJECT_ID('dbo.Passkey') AND name = 'UQ_Passkey_CredentialId';
