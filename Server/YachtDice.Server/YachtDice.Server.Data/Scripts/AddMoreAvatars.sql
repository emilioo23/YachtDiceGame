USE YachtDiceDB;
GO

-- Agrega a la tabla Avatar los avatares 11 al 20, que son los que se sumaron al juego.
-- Es necesario porque el jugador guarda su avatar con una llave foranea a esta tabla.
-- Es segura de correr varias veces: solo inserta los avatares que todavia no existen.
SET IDENTITY_INSERT dbo.Avatar ON;

INSERT INTO dbo.Avatar (Id, ImageUrl)
SELECT newAvatar.Id, newAvatar.ImageUrl
FROM (VALUES
    (11, N'Avatar_11'),
    (12, N'Avatar_12'),
    (13, N'Avatar_13'),
    (14, N'Avatar_14'),
    (15, N'Avatar_15'),
    (16, N'Avatar_16'),
    (17, N'Avatar_17'),
    (18, N'Avatar_18'),
    (19, N'Avatar_19'),
    (20, N'Avatar_20')
) AS newAvatar (Id, ImageUrl)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Avatar AS existing WHERE existing.Id = newAvatar.Id);

SET IDENTITY_INSERT dbo.Avatar OFF;
GO

-- Comprobacion: debe regresar 20 o mas.
SELECT COUNT(*) AS TotalAvatars FROM dbo.Avatar;
