-- Registers the 10 avatars of the game in the Avatar table.
-- Run it once in SQL Server Management Studio, connected to the YachtDice database
-- (the drawings are inside the client; the table only needs the identifiers).
-- It is safe to run it again: it only adds the rows that are missing.
--
-- Identifier -> avatar -> level needed (the levels are defined in the server, in AvatarCatalog.cs)
--    1 Marinero      level 1     6 Robot        level 5
--    2 Capitan       level 1     7 Gato         level 8
--    3 Dado          level 1     8 Astronauta   level 12
--    4 Gaviota       level 1     9 Mago         level 16
--    5 Pirata        level 3    10 Rey          level 20

SET NOCOUNT ON;

SET IDENTITY_INSERT dbo.Avatar ON;

MERGE dbo.Avatar AS target
USING (VALUES
    (1,  N'avatar_01'),
    (2,  N'avatar_02'),
    (3,  N'avatar_03'),
    (4,  N'avatar_04'),
    (5,  N'avatar_05'),
    (6,  N'avatar_06'),
    (7,  N'avatar_07'),
    (8,  N'avatar_08'),
    (9,  N'avatar_09'),
    (10, N'avatar_10')
) AS source (Id, ImageUrl)
    ON target.Id = source.Id
WHEN MATCHED AND target.ImageUrl <> source.ImageUrl THEN
    UPDATE SET ImageUrl = source.ImageUrl
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, ImageUrl) VALUES (source.Id, source.ImageUrl);

SET IDENTITY_INSERT dbo.Avatar OFF;

SELECT Id, ImageUrl FROM dbo.Avatar ORDER BY Id;
