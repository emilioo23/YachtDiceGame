USE YachtDiceDB;
GO

-- Agrega la columna GameSpeed a Room y la columna IsHeld a RolledDie.
-- Es necesario porque las partidas en curso se guardan para retomarlas si el servidor se reinicia, con su velocidad y los dados apartados.
-- Es seguro de correr varias veces: solo agrega las columnas que todavia no existen.
IF COL_LENGTH('dbo.Room', 'GameSpeed') IS NULL
    ALTER TABLE dbo.Room ADD GameSpeed NVARCHAR(10) NULL;
GO

IF COL_LENGTH('dbo.RolledDie', 'IsHeld') IS NULL
    ALTER TABLE dbo.RolledDie ADD IsHeld BIT NOT NULL CONSTRAINT DF_RolledDie_IsHeld DEFAULT 0;
GO

SELECT
    COL_LENGTH('dbo.Room', 'GameSpeed') / 2 AS GameSpeedMaxLength,
    COL_LENGTH('dbo.RolledDie', 'IsHeld') AS IsHeldLength;
