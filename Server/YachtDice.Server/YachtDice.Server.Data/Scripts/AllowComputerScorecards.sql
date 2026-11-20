USE YachtDiceDB;
GO

-- Permite guardar la tarjeta de un jugador de la computadora en la tabla Scorecard, con PlayerId en NULL.
-- Es necesario porque el historial muestra la tarjeta de la computadora y ese jugador no tiene cuenta.
-- Es seguro de correr varias veces: quita la llave foranea de PlayerId, deja la columna como NULL y la vuelve a crear.
DECLARE @foreignKeyName SYSNAME;
DECLARE @dropSql NVARCHAR(MAX);

SELECT @foreignKeyName = foreignKey.name
FROM sys.foreign_keys AS foreignKey
INNER JOIN sys.foreign_key_columns AS keyColumn ON keyColumn.constraint_object_id = foreignKey.object_id
INNER JOIN sys.columns AS tableColumn
    ON tableColumn.object_id = keyColumn.parent_object_id AND tableColumn.column_id = keyColumn.parent_column_id
WHERE foreignKey.parent_object_id = OBJECT_ID('dbo.Scorecard')
  AND tableColumn.name = 'PlayerId';

IF @foreignKeyName IS NOT NULL
BEGIN
    SET @dropSql = N'ALTER TABLE dbo.Scorecard DROP CONSTRAINT ' + QUOTENAME(@foreignKeyName);
    EXEC sp_executesql @dropSql;
END

ALTER TABLE dbo.Scorecard ALTER COLUMN PlayerId INT NULL;

ALTER TABLE dbo.Scorecard
    ADD CONSTRAINT FK_Scorecard_Player FOREIGN KEY (PlayerId) REFERENCES dbo.Player (Id);
GO

-- Comprobacion: la columna PlayerId debe aceptar NULL.
SELECT name, is_nullable FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Scorecard') AND name = 'PlayerId';
