USE YachtDiceDB;
GO

-- Agrega a la tabla Category las doce casillas de la tarjeta de puntos.
-- Es necesario porque cada casilla guardada en ScorecardEntry apunta con una llave foranea a esta tabla.
-- Es seguro de correr varias veces: solo inserta las casillas que todavia no existen.
INSERT INTO dbo.Category (Name)
SELECT categoryName
FROM (VALUES ('Ones'), ('Twos'), ('Threes'), ('Fours'), ('Fives'), ('Sixes'),
             ('FullHouse'), ('FourOfAKind'), ('LittleStraight'), ('BigStraight'), ('Choice'), ('Yacht'))
     AS categories(categoryName)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Category WHERE Name = categories.categoryName);
GO

SELECT COUNT(*) AS CategoryCount FROM dbo.Category;
