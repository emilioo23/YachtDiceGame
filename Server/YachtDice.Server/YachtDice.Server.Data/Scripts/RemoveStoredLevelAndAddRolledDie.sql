USE YachtDiceDB;
GO

-- Quita la columna Level de Player y la columna DiceValues de TurnState, y crea la tabla RolledDie con un dado por fila.
-- Es necesario porque el nivel ahora se calcula con la experiencia y los dados del turno se guardan en RolledDie.
-- Es seguro de correr varias veces: solo quita o crea lo que todavia no se ha cambiado.
DECLARE @defaultName NVARCHAR(200);
SELECT @defaultName = dc.name
FROM sys.default_constraints dc
JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
WHERE dc.parent_object_id = OBJECT_ID('dbo.Player') AND c.name = 'Level';
IF @defaultName IS NOT NULL
    EXEC('ALTER TABLE dbo.Player DROP CONSTRAINT [' + @defaultName + ']');
GO

IF COL_LENGTH('dbo.Player', 'Level') IS NOT NULL
    ALTER TABLE dbo.Player DROP COLUMN Level;
GO

IF COL_LENGTH('dbo.TurnState', 'DiceValues') IS NOT NULL
    ALTER TABLE dbo.TurnState DROP COLUMN DiceValues;
GO

IF OBJECT_ID('dbo.RolledDie', 'U') IS NULL
    CREATE TABLE dbo.RolledDie (
        TurnStateId INT NOT NULL,
        DiePosition INT NOT NULL,
        Value INT NOT NULL,
        CONSTRAINT PK_RolledDie PRIMARY KEY (TurnStateId, DiePosition),
        CONSTRAINT FK_RolledDie_TurnState FOREIGN KEY (TurnStateId) REFERENCES dbo.TurnState (Id),
        CONSTRAINT CK_RolledDie_Position CHECK (DiePosition BETWEEN 1 AND 5),
        CONSTRAINT CK_RolledDie_Value CHECK (Value BETWEEN 1 AND 6)
    );
GO

SELECT
    COL_LENGTH('dbo.Player', 'Level') AS PlayerLevelLength,
    COL_LENGTH('dbo.TurnState', 'DiceValues') AS DiceValuesLength,
    OBJECT_ID('dbo.RolledDie', 'U') AS RolledDieTableId;
