USE YachtDiceDB;
GO

-- Permite el estado Blocked en la tabla Friendship, que es como se guarda que un jugador bloqueo a otro.
-- Es necesario porque al bloquear el servidor guarda Blocked y la tabla solo aceptaba Accepted y Pending.
-- Es seguro de correr varias veces: solo cambia el CHECK de Status si todavia no acepta Blocked.
IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Friendship_Status'
      AND parent_object_id = OBJECT_ID('dbo.Friendship')
      AND definition NOT LIKE '%Blocked%')
BEGIN
    ALTER TABLE dbo.Friendship DROP CONSTRAINT CK_Friendship_Status;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Friendship_Status')
BEGIN
    ALTER TABLE dbo.Friendship
        ADD CONSTRAINT CK_Friendship_Status CHECK (Status IN ('Accepted', 'Pending', 'Blocked'));
END
GO

-- Comprobacion: debe regresar una fila con el CHECK CK_Friendship_Status que acepta Blocked.
SELECT name, definition FROM sys.check_constraints WHERE name = 'CK_Friendship_Status';
