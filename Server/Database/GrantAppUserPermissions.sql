-- Creates the login that the game server uses and gives it only the permissions it needs.
-- Run it with an administrator login after creating the database. Replace the password before running
-- it and never save the real password in this file.
-- The server does not need db_datareader nor db_datawriter, so both roles are removed from the user.

USE [YachtDiceDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'YachtAppUser')
BEGIN
    CREATE LOGIN [YachtAppUser]
        WITH PASSWORD = N'<replace with the password of the server>',
        DEFAULT_DATABASE = [YachtDiceDB],
        CHECK_POLICY = ON;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'YachtAppUser')
BEGIN
    CREATE USER [YachtAppUser] FOR LOGIN [YachtAppUser] WITH DEFAULT_SCHEMA = [dbo];
END
GO

IF IS_ROLEMEMBER(N'db_datareader', N'YachtAppUser') = 1
BEGIN
    ALTER ROLE [db_datareader] DROP MEMBER [YachtAppUser];
END

IF IS_ROLEMEMBER(N'db_datawriter', N'YachtAppUser') = 1
BEGIN
    ALTER ROLE [db_datawriter] DROP MEMBER [YachtAppUser];
END
GO

-- Accounts: registration, login, avatar and password reset.
GRANT SELECT, INSERT, UPDATE ON [dbo].[Player] TO [YachtAppUser];

-- Two-step verification setting of each player.
GRANT SELECT, INSERT, UPDATE ON [dbo].[TwoFactorAuth] TO [YachtAppUser];

-- Password recovery codes. A new code replaces the previous one, so the server deletes rows.
GRANT SELECT, INSERT, DELETE ON [dbo].[PwdResetToken] TO [YachtAppUser];

-- Friend requests and friendships.
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Friendship] TO [YachtAppUser];

-- Statistics and ranking. The server only reads the history.
GRANT SELECT ON [dbo].[GameHistoryPlayer] TO [YachtAppUser];
GO
