USE YachtDiceDB;
GO

-- Agrega a la tabla PlayerSettings la columna FlagCode, donde se guarda la bandera de pais que cada jugador elige.
-- Es necesario porque el perfil del jugador lee y guarda su bandera en esa columna.
-- Es seguro de correr varias veces: solo agrega la columna si todavia no existe.
IF COL_LENGTH('dbo.PlayerSettings', 'FlagCode') IS NULL
BEGIN
    ALTER TABLE dbo.PlayerSettings ADD FlagCode NVARCHAR(2) NULL;
END
GO

-- Comprobacion: debe regresar 2 (la columna existe y mide dos caracteres).
SELECT COL_LENGTH('dbo.PlayerSettings', 'FlagCode') / 2 AS FlagCodeMaxLength;
