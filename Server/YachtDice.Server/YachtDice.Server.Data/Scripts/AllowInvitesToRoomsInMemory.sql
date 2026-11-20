USE YachtDiceDB;
GO

-- Permite que RoomInvite guarde invitaciones sin una sala en la base: RoomId acepta nulos y se agrega la columna RoomCode.
-- Es necesario porque las salas viven en la memoria del servidor y solo se guardan en Room cuando termina una partida.
-- Es seguro de correr varias veces: solo cambia la columna, la llave foranea y RoomCode si todavia no se han cambiado.
DECLARE @foreignKeyName NVARCHAR(200);
SELECT @foreignKeyName = fk.name
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
WHERE fk.parent_object_id = OBJECT_ID('dbo.RoomInvite') AND c.name = 'RoomId';

IF @foreignKeyName IS NOT NULL AND COLUMNPROPERTY(OBJECT_ID('dbo.RoomInvite'), 'RoomId', 'AllowsNull') = 0
BEGIN
    EXEC('ALTER TABLE dbo.RoomInvite DROP CONSTRAINT [' + @foreignKeyName + ']');
    ALTER TABLE dbo.RoomInvite ALTER COLUMN RoomId INT NULL;
    ALTER TABLE dbo.RoomInvite ADD CONSTRAINT FK_RoomInvite_Room FOREIGN KEY (RoomId) REFERENCES dbo.Room (Id);
END
GO

IF COL_LENGTH('dbo.RoomInvite', 'RoomCode') IS NULL
    ALTER TABLE dbo.RoomInvite ADD RoomCode NVARCHAR(6) NULL;
GO

SELECT
    COLUMNPROPERTY(OBJECT_ID('dbo.RoomInvite'), 'RoomId', 'AllowsNull') AS RoomIdAllowsNull,
    COL_LENGTH('dbo.RoomInvite', 'RoomCode') / 2 AS RoomCodeMaxLength;
