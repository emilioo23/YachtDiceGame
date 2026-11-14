-- Agrega la columna donde se guarda la bandera de pais que cada jugador elige para su perfil.
-- Es segura de correr varias veces: solo agrega la columna si todavia no existe.
IF COL_LENGTH('dbo.PlayerSettings', 'FlagCode') IS NULL
BEGIN
    ALTER TABLE dbo.PlayerSettings ADD FlagCode NVARCHAR(2) NULL;
END
