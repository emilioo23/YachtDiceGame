USE YachtDiceDB;
GO

-- Permite que KickLog guarde expulsiones sin una sala en la base: RoomId acepta nulos y se agrega la columna RoomCode.
-- Es necesario porque las salas viven en la memoria del servidor y solo se guardan en Room cuando termina una partida.
-- Es seguro de correr varias veces: solo cambia la columna, la llave foranea y RoomCode si todavia no se han cambiado.
DECLARE @foreignKeyName NVARCHAR(200);
SELECT @foreignKeyName = fk.name
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
WHERE fk.parent_object_id = OBJECT_ID('dbo.KickLog') AND c.name = 'RoomId';

IF @foreignKeyName IS NOT NULL AND COLUMNPROPERTY(OBJECT_ID('dbo.KickLog'), 'RoomId', 'AllowsNull') = 0
BEGIN
    EXEC('ALTER TABLE dbo.KickLog DROP CONSTRAINT [' + @foreignKeyName + ']');
    ALTER TABLE dbo.KickLog ALTER COLUMN RoomId INT NULL;
    ALTER TABLE dbo.KickLog ADD CONSTRAINT FK_KickLog_Room FOREIGN KEY (RoomId) REFERENCES dbo.Room (Id);
END
GO

IF COL_LENGTH('dbo.KickLog', 'RoomCode') IS NULL
    ALTER TABLE dbo.KickLog ADD RoomCode NVARCHAR(6) NULL;
GO

SELECT
    COLUMNPROPERTY(OBJECT_ID('dbo.KickLog'), 'RoomId', 'AllowsNull') AS RoomIdAllowsNull,
    COL_LENGTH('dbo.KickLog', 'RoomCode') / 2 AS RoomCodeMaxLength;
