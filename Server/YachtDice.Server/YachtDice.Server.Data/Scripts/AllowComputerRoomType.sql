USE YachtDiceDB;
GO

-- Permite el tipo de sala Computer en la tabla Room, que es como se guardan las partidas contra la computadora.
-- Es necesario porque el servidor guarda el resultado de esas partidas y la tabla solo aceptaba Public y Private.
-- Es seguro de correr varias veces: quita el CHECK de RoomType que exista y deja uno que tambien acepta Computer.
DECLARE @constraintName SYSNAME;
DECLARE @dropSql NVARCHAR(MAX);

DECLARE constraintCursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT name
    FROM sys.check_constraints
    WHERE parent_object_id = OBJECT_ID('dbo.Room')
      AND definition LIKE '%RoomType%';

OPEN constraintCursor;
FETCH NEXT FROM constraintCursor INTO @constraintName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @dropSql = N'ALTER TABLE dbo.Room DROP CONSTRAINT ' + QUOTENAME(@constraintName);
    EXEC sp_executesql @dropSql;
    FETCH NEXT FROM constraintCursor INTO @constraintName;
END

CLOSE constraintCursor;
DEALLOCATE constraintCursor;

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Room_RoomType')
BEGIN
    ALTER TABLE dbo.Room
        ADD CONSTRAINT CK_Room_RoomType CHECK (RoomType IN ('Public', 'Private', 'Computer'));
END
GO

-- Comprobacion: debe regresar una fila con el CHECK CK_Room_RoomType que acepta Computer.
SELECT name, definition FROM sys.check_constraints WHERE name = 'CK_Room_RoomType';
