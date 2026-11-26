namespace YachtDice.Server.Data.Logging
{
    /// <summary>
    /// Texts that only the developer sees: the messages of the exceptions and the entries of the log.
    /// Each text is written once here and the classes of every project use it.
    /// </summary>
    public static class LogMessages
    {
        /// <summary>
        /// Exception message: A dependency of the authentication service cannot be null.
        /// </summary>
        public const string AuthenticationNullDependencyMessage =
            "A dependency of the authentication service cannot be null.";

        /// <summary>
        /// Exception message: The name of the player cannot be empty.
        /// </summary>
        public const string BlankPlayerNameMessage = "The name of the player cannot be empty.";

        /// <summary>
        /// Exception message: The CBOR data cannot be null.
        /// </summary>
        public const string CborReaderNullDataMessage = "The CBOR data cannot be null.";

        /// <summary>
        /// Exception message: The message to censor cannot be null.
        /// </summary>
        public const string ChatCensorNullMessageMessage = "The message to censor cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the chat service cannot be null.
        /// </summary>
        public const string ChatNullDependencyMessage = "A dependency of the chat service cannot be null.";

        /// <summary>
        /// Exception message: The email to send cannot be null.
        /// </summary>
        public const string EmailNullMessageMessage = "The email to send cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the friend service cannot be null.
        /// </summary>
        public const string FriendNullDependencyMessage = "A dependency of the friend service cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the friendship service cannot be null.
        /// </summary>
        public const string FriendshipNullDependencyMessage = "A dependency of the friendship service cannot be null.";

        /// <summary>
        /// Exception message: The details of the report cannot be null.
        /// </summary>
        public const string FriendshipNullReportMessage = "The details of the report cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the game checkpoint service cannot be null.
        /// </summary>
        public const string GameCheckpointNullDependencyMessage =
            "A dependency of the game checkpoint service cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the game coordinator cannot be null.
        /// </summary>
        public const string GameCoordinatorNullDependencyMessage =
            "A dependency of the game coordinator cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the game service cannot be null.
        /// </summary>
        public const string GameNullDependencyMessage = "A dependency of the game service cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the history of expulsions cannot be null.
        /// </summary>
        public const string KickHistoryNullDependencyMessage =
            "A dependency of the history of expulsions cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the login service cannot be null.
        /// </summary>
        public const string LoginNullDependencyMessage = "A dependency of the login service cannot be null.";

        /// <summary>
        /// Exception message: The achievements of the players cannot be null.
        /// </summary>
        public const string NullAchievementsMessage = "The achievements of the players cannot be null.";

        /// <summary>
        /// Exception message: The value cannot be null.
        /// </summary>
        public const string NullArgumentMessage = "The value cannot be null.";

        /// <summary>
        /// Exception message: The answer of the authenticator cannot be null.
        /// </summary>
        public const string NullAttestationMessage = "The answer of the authenticator cannot be null.";

        /// <summary>
        /// Exception message: The data access of the backup codes cannot be null.
        /// </summary>
        public const string NullBackupRepositoryMessage = "The data access of the backup codes cannot be null.";

        /// <summary>
        /// Exception message: The resource key of the body cannot be null.
        /// </summary>
        public const string NullBodyResourceKeyMessage = "The resource key of the body cannot be null.";

        /// <summary>
        /// Exception message: The score calculator cannot be null.
        /// </summary>
        public const string NullCalculatorMessage = "The score calculator cannot be null.";

        /// <summary>
        /// Exception message: The callback channel of the player cannot be null.
        /// </summary>
        public const string NullCallbackMessage = "The callback channel of the player cannot be null.";

        /// <summary>
        /// Exception message: The scorecard of the player cannot be null.
        /// </summary>
        public const string NullCardMessage = "The scorecard of the player cannot be null.";

        /// <summary>
        /// Exception message: The identifier of the challenge cannot be null.
        /// </summary>
        public const string NullChallengeIdMessage = "The identifier of the challenge cannot be null.";

        /// <summary>
        /// Exception message: The game in progress cannot be null.
        /// </summary>
        public const string NullCheckpointMessage = "The game in progress cannot be null.";

        /// <summary>
        /// Exception message: The clock cannot be null.
        /// </summary>
        public const string NullClockMessage = "The clock cannot be null.";

        /// <summary>
        /// Exception message: The list of backup codes cannot be null.
        /// </summary>
        public const string NullCodesMessage = "The list of backup codes cannot be null.";

        /// <summary>
        /// Exception message: The login credentials cannot be null.
        /// </summary>
        public const string NullCredentialsMessage = "The login credentials cannot be null.";

        /// <summary>
        /// Exception message: Empty data was received in {0}.
        /// </summary>
        public const string NullDataLogFormat = "Empty data was received in {0}.";

        /// <summary>
        /// Exception message: The delivery data of the code cannot be null.
        /// </summary>
        public const string NullDeliveryMessage = "The delivery data of the code cannot be null.";

        /// <summary>
        /// Exception message: The dice of a roll cannot be null.
        /// </summary>
        public const string NullDiceMessage = "The dice of a roll cannot be null.";

        /// <summary>
        /// Exception message: The roller of the dice cannot be null.
        /// </summary>
        public const string NullDieRollerMessage = "The roller of the dice cannot be null.";

        /// <summary>
        /// Exception message: The document of the rules of the game cannot be null.
        /// </summary>
        public const string NullDocumentMessage = "The document of the rules of the game cannot be null.";

        /// <summary>
        /// Exception message: The email service cannot be null.
        /// </summary>
        public const string NullEmailServiceMessage = "The email service cannot be null.";

        /// <summary>
        /// Exception message: The exception to log cannot be null.
        /// </summary>
        public const string NullExceptionMessage = "The exception to log cannot be null.";

        /// <summary>
        /// Exception message: What finds the identifiers of the players cannot be null.
        /// </summary>
        public const string NullFinderMessage = "What finds the identifiers of the players cannot be null.";

        /// <summary>
        /// Exception message: The first name of the player cannot be null.
        /// </summary>
        public const string NullFirstNameMessage = "The first name of the player cannot be null.";

        /// <summary>
        /// Exception message: The name of the friend cannot be null.
        /// </summary>
        public const string NullFriendNameMessage = "The name of the friend cannot be null.";

        /// <summary>
        /// Exception message: The friendship cannot be null.
        /// </summary>
        public const string NullFriendshipMessage = "The friendship cannot be null.";

        /// <summary>
        /// Exception message: The data access of the game history cannot be null.
        /// </summary>
        public const string NullGameHistoryRepositoryMessage = "The data access of the game history cannot be null.";

        /// <summary>
        /// Exception message: The hashes of the backup codes cannot be null.
        /// </summary>
        public const string NullHashesMessage = "The hashes of the backup codes cannot be null.";

        /// <summary>
        /// Exception message: The invitation cannot be null.
        /// </summary>
        public const string NullInviteMessage = "The invitation cannot be null.";

        /// <summary>
        /// Exception message: The expulsion cannot be null.
        /// </summary>
        public const string NullKickMessage = "The expulsion cannot be null.";

        /// <summary>
        /// Exception message: The table of levels cannot be null.
        /// </summary>
        public const string NullLevelTableMessage = "The table of levels cannot be null.";

        /// <summary>
        /// Exception message: The masked email cannot be null.
        /// </summary>
        public const string NullMaskedEmailMessage = "The masked email cannot be null.";

        /// <summary>
        /// Exception message: The action when the time is over cannot be null.
        /// </summary>
        public const string NullOnExpiredMessage = "The action when the time is over cannot be null.";

        /// <summary>
        /// Exception message: The owner type cannot be null when initializing the logger.
        /// </summary>
        public const string NullOwnerTypeMessage = "The owner type cannot be null when initializing the logger.";

        /// <summary>
        /// Exception message: The participant of the game cannot be null.
        /// </summary>
        public const string NullParticipantMessage = "The participant of the game cannot be null.";

        /// <summary>
        /// Exception message: The participants of the game cannot be null.
        /// </summary>
        public const string NullParticipantsMessage = "The participants of the game cannot be null.";

        /// <summary>
        /// Exception message: The identifier of the player cannot be null.
        /// </summary>
        public const string NullPlayerIdMessage = "The identifier of the player cannot be null.";

        /// <summary>
        /// Exception message: The player cannot be null.
        /// </summary>
        public const string NullPlayerMessage = "The player cannot be null.";

        /// <summary>
        /// Exception message: The name of the player cannot be null.
        /// </summary>
        public const string NullPlayerNameMessage = "The name of the player cannot be null.";

        /// <summary>
        /// Exception message: The data access of the players cannot be null.
        /// </summary>
        public const string NullPlayerRepositoryMessage = "The data access of the players cannot be null.";

        /// <summary>
        /// Exception message: The passkey to save cannot be null.
        /// </summary>
        public const string NullRecordMessage = "The passkey to save cannot be null.";

        /// <summary>
        /// Exception message: The result of the game cannot be null.
        /// </summary>
        public const string NullResultMessage = "The result of the game cannot be null.";

        /// <summary>
        /// Exception message: The code of the room cannot be null.
        /// </summary>
        public const string NullRoomCodeMessage = "The code of the room cannot be null.";

        /// <summary>
        /// Exception message: The room cannot be null.
        /// </summary>
        public const string NullRoomMessage = "The room cannot be null.";

        /// <summary>
        /// Exception message: A rule of the game cannot be null.
        /// </summary>
        public const string NullRuleMessage = "A rule of the game cannot be null.";

        /// <summary>
        /// Exception message: The rules of the achievements cannot be null.
        /// </summary>
        public const string NullRulesMessage = "The rules of the achievements cannot be null.";

        /// <summary>
        /// Exception message: The saved game cannot be null.
        /// </summary>
        public const string NullSavedGameMessage = "The saved game cannot be null.";

        /// <summary>
        /// Exception message: The saved boxes cannot be null.
        /// </summary>
        public const string NullScoresMessage = "The saved boxes cannot be null.";

        /// <summary>
        /// Exception message: The seconds of each speed cannot be null.
        /// </summary>
        public const string NullSecondsMessage = "The seconds of each speed cannot be null.";

        /// <summary>
        /// Exception message: The name of the sender cannot be null.
        /// </summary>
        public const string NullSenderNameMessage = "The name of the sender cannot be null.";

        /// <summary>
        /// Exception message: The identifier of the session cannot be null.
        /// </summary>
        public const string NullSessionIdMessage = "The identifier of the session cannot be null.";

        /// <summary>
        /// Exception message: The sessions of the players cannot be null.
        /// </summary>
        public const string NullSessionsMessage = "The sessions of the players cannot be null.";

        /// <summary>
        /// Exception message: The settings of the room cannot be null.
        /// </summary>
        public const string NullSettingsMessage = "The settings of the room cannot be null.";

        /// <summary>
        /// Exception message: The state of the game cannot be null.
        /// </summary>
        public const string NullSnapshotMessage = "The state of the game cannot be null.";

        /// <summary>
        /// Exception message: The resource key of the subject cannot be null.
        /// </summary>
        public const string NullSubjectResourceKeyMessage = "The resource key of the subject cannot be null.";

        /// <summary>
        /// Exception message: The template of the email cannot be null.
        /// </summary>
        public const string NullTemplateMessage = "The template of the email cannot be null.";

        /// <summary>
        /// Exception message: The experience of each level cannot be null.
        /// </summary>
        public const string NullThresholdsMessage = "The experience of each level cannot be null.";

        /// <summary>
        /// Exception message: The recovery code cannot be null.
        /// </summary>
        public const string NullTokenMessage = "The recovery code cannot be null.";

        /// <summary>
        /// Exception message: The duration of the turns cannot be null.
        /// </summary>
        public const string NullTurnDurationMessage = "The duration of the turns cannot be null.";

        /// <summary>
        /// Exception message: The turn cannot be null.
        /// </summary>
        public const string NullTurnMessage = "The turn cannot be null.";

        /// <summary>
        /// Exception message: The filter of offensive words cannot be null.
        /// </summary>
        public const string NullWordFilterMessage = "The filter of offensive words cannot be null.";

        /// <summary>
        /// Exception message: The list of offensive words cannot be null.
        /// </summary>
        public const string NullWordsMessage = "The list of offensive words cannot be null.";

        /// <summary>
        /// Exception message: The text to check cannot be null.
        /// </summary>
        public const string OffensiveWordFilterNullTextMessage = "The text to check cannot be null.";

        /// <summary>
        /// Exception message: The bytes to encode cannot be null.
        /// </summary>
        public const string PasskeyNullDataMessage = "The bytes to encode cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the passkey service cannot be null.
        /// </summary>
        public const string PasskeyNullDependencyMessage = "A dependency of the passkey service cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the password service cannot be null.
        /// </summary>
        public const string PasswordNullDependencyMessage = "A dependency of the password service cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the achievements of the players cannot be null.
        /// </summary>
        public const string PlayerAchievementsNullDependencyMessage =
            "A dependency of the achievements of the players cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the player service cannot be null.
        /// </summary>
        public const string PlayerNullDependencyMessage = "A dependency of the player service cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the player registration service cannot be null.
        /// </summary>
        public const string PlayerRegistrationNullDependencyMessage =
            "A dependency of the player registration service cannot be null.";

        /// <summary>
        /// Exception message: The report cannot be null.
        /// </summary>
        public const string PlayerReportNullReportMessage = "The report cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the player statistics service cannot be null.
        /// </summary>
        public const string PlayerStatisticsNullDependencyMessage =
            "A dependency of the player statistics service cannot be null.";

        /// <summary>
        /// Exception message: The new player cannot be null.
        /// </summary>
        public const string RegistrationNullPlayerMessage = "The new player cannot be null.";

        /// <summary>
        /// Exception message: The player repository cannot be null.
        /// </summary>
        public const string RegistrationNullRepositoryMessage = "The player repository cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the room invitation service cannot be null.
        /// </summary>
        public const string RoomInvitationNullDependencyMessage =
            "A dependency of the room invitation service cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the room manager cannot be null.
        /// </summary>
        public const string RoomNullDependencyMessage = "A dependency of the room manager cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the room service cannot be null.
        /// </summary>
        public const string RoomServiceNullDependencyMessage = "A dependency of the room service cannot be null.";

        /// <summary>
        /// Exception message: The provided data cannot be empty.
        /// </summary>
        public const string ServiceNullDataMessage = "The provided data cannot be empty.";

        /// <summary>
        /// Exception message: The text to clean cannot be null.
        /// </summary>
        public const string TextSpacingNullTextMessage = "The text to clean cannot be null.";

        /// <summary>
        /// Exception message: The verification code cannot be null.
        /// </summary>
        public const string TwoFactorCodeNullCodeMessage = "The verification code cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the two-factor management cannot be null.
        /// </summary>
        public const string TwoFactorManagementNullDependencyMessage =
            "A dependency of the two-factor management cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the two-factor settings cannot be null.
        /// </summary>
        public const string TwoFactorSettingsNullDependencyMessage =
            "A dependency of the two-factor settings cannot be null.";

        /// <summary>
        /// Exception message: A dependency of the two-factor verification cannot be null.
        /// </summary>
        public const string TwoFactorVerificationNullDependencyMessage =
            "A dependency of the two-factor verification cannot be null.";

        /// <summary>
        /// Exception message: The value cannot be empty.
        /// </summary>
        public const string BlankValueMessage = "The value cannot be empty.";

        /// <summary>
        /// Exception message: The CBOR data is not valid.
        /// </summary>
        public const string CborReaderInvalidDataMessage = "The CBOR data is not valid.";

        /// <summary>
        /// Exception message: The recipient address cannot be empty.
        /// </summary>
        public const string EmptyRecipientMessage = "The recipient address cannot be empty.";

        /// <summary>
        /// Exception message: There must be at least one level.
        /// </summary>
        public const string EmptyThresholdsMessage = "There must be at least one level.";

        /// <summary>
        /// Exception message: A roll must have exactly five dice.
        /// </summary>
        public const string InvalidCountMessage = "A roll must have exactly five dice.";

        /// <summary>
        /// Exception message: The first level must need zero experience.
        /// </summary>
        public const string InvalidFirstThresholdMessage = "The first level must need zero experience.";

        /// <summary>
        /// Exception message: The code must have at least one digit.
        /// </summary>
        public const string InvalidLengthMessage = "The code must have at least one digit.";

        /// <summary>
        /// Exception message: The level must be at least one.
        /// </summary>
        public const string InvalidLevelMessage = "The level must be at least one.";

        /// <summary>
        /// Exception message: The maximum value must be greater than the minimum value.
        /// </summary>
        public const string InvalidRangeMessage = "The maximum value must be greater than the minimum value.";

        /// <summary>
        /// Exception message: A die can only show a value from one to six.
        /// </summary>
        public const string InvalidValueMessage = "A die can only show a value from one to six.";

        /// <summary>
        /// Exception message: The setting '{0}' is missing in the configuration file.
        /// </summary>
        public const string MissingSettingMessage = "The setting '{0}' is missing in the configuration file.";

        /// <summary>
        /// Exception message: The environment variable {0} is not defined on this computer.
        /// </summary>
        public const string MissingVariableFormat = "The environment variable {0} is not defined on this computer.";

        /// <summary>
        /// Exception message: The experience of a player cannot be negative.
        /// </summary>
        public const string NegativeExperienceMessage = "The experience of a player cannot be negative.";

        /// <summary>
        /// Exception message: The total of the scorecard cannot be negative.
        /// </summary>
        public const string NegativeTotalMessage = "The total of the scorecard cannot be negative.";

        /// <summary>
        /// Exception message: The provided data is invalid.
        /// </summary>
        public const string ServiceInvalidDataMessage = "The provided data is invalid.";

        /// <summary>
        /// Exception message: The turn of every speed must last at least one second.
        /// </summary>
        public const string TooShortSecondsMessage = "The turn of every speed must last at least one second.";

        /// <summary>
        /// Exception message: The box does not exist in the scorecard.
        /// </summary>
        public const string UnknownCategoryMessage = "The box does not exist in the scorecard.";

        /// <summary>
        /// Exception message: Could not save the avatar due to a database failure.
        /// </summary>
        public const string AvatarUpdateFailedMessage = "Could not save the avatar due to a database failure.";

        /// <summary>
        /// Exception message: Database error occurred while reading the backup codes.
        /// </summary>
        public const string BackupCodeQueryFailedMessage = "Database error occurred while reading the backup codes.";

        /// <summary>
        /// Exception message: Database error occurred while saving the backup codes.
        /// </summary>
        public const string BackupCodeUpdateFailedMessage = "Database error occurred while saving the backup codes.";

        /// <summary>
        /// Exception message: Could not save the new password due to a database failure.
        /// </summary>
        public const string CompleteFailedMessage = "Could not save the new password due to a database failure.";

        /// <summary>
        /// Exception message: Database error occurred while reading the games against the computer.
        /// </summary>
        public const string ComputerGamesFailedMessage =
            "Database error occurred while reading the games against the computer.";

        /// <summary>
        /// Exception message: A notification could not be delivered to a client.
        /// </summary>
        public const string DeliveryFailedMessage = "A notification could not be delivered to a client.";

        /// <summary>
        /// Exception message: Database error occurred while verifying email existence.
        /// </summary>
        public const string EmailExistsFailedMessage = "Database error occurred while verifying email existence.";

        /// <summary>
        /// Exception message: Database error occurred while searching player by email.
        /// </summary>
        public const string FindByEmailFailedMessage = "Database error occurred while searching player by email.";

        /// <summary>
        /// Exception message: Database error occurred while searching player by name.
        /// </summary>
        public const string FindByNameFailedMessage = "Database error occurred while searching player by name.";

        /// <summary>
        /// Exception message: Database error occurred while fetching game history by player ID.
        /// </summary>
        public const string FindByPlayerFailedMessage =
            "Database error occurred while fetching game history by player ID.";

        /// <summary>
        /// Exception message: Database error occurred while searching player by username.
        /// </summary>
        public const string FindByUsernameFailedMessage = "Database error occurred while searching player by username.";

        /// <summary>
        /// Exception message: Database error occurred while searching the recovery code.
        /// </summary>
        public const string FindFailedMessage = "Database error occurred while searching the recovery code.";

        /// <summary>
        /// Exception message: Could not read the flag due to a database failure.
        /// </summary>
        public const string FlagQueryFailedMessage = "Could not read the flag due to a database failure.";

        /// <summary>
        /// Exception message: Could not save the flag due to a database failure.
        /// </summary>
        public const string FlagUpdateFailedMessage = "Could not save the flag due to a database failure.";

        /// <summary>
        /// Exception message: Database error occurred while querying the friendships.
        /// </summary>
        public const string FriendQueryFailedMessage = "Database error occurred while querying the friendships.";

        /// <summary>
        /// Exception message: Database error occurred while saving the friendships.
        /// </summary>
        public const string FriendUpdateFailedMessage = "Database error occurred while saving the friendships.";

        /// <summary>
        /// Exception message: Database error occurred while reading the games in progress.
        /// </summary>
        public const string GameCheckpointQueryFailedMessage =
            "Database error occurred while reading the games in progress.";

        /// <summary>
        /// Exception message: Database error occurred while saving the game in progress.
        /// </summary>
        public const string GameCheckpointSaveFailedMessage =
            "Database error occurred while saving the game in progress.";

        /// <summary>
        /// Exception message: Database error occurred while saving the result of a game.
        /// </summary>
        public const string GameHistorySaveFailedMessage = "Database error occurred while saving the result of a game.";

        /// <summary>
        /// Exception message: Database error occurred while reading the expulsions.
        /// </summary>
        public const string KickLogQueryFailedMessage = "Database error occurred while reading the expulsions.";

        /// <summary>
        /// Exception message: Database error occurred while saving the expulsion.
        /// </summary>
        public const string KickLogUpdateFailedMessage = "Database error occurred while saving the expulsion.";

        /// <summary>
        /// Exception message: Could not read the login lock due to a database failure.
        /// </summary>
        public const string LockQueryFailedMessage = "Could not read the login lock due to a database failure.";

        /// <summary>
        /// Exception message: Could not save the login lock due to a database failure.
        /// </summary>
        public const string LockUpdateFailedMessage = "Could not save the login lock due to a database failure.";

        /// <summary>
        /// Exception message: Database error occurred while reading the passkeys.
        /// </summary>
        public const string PasskeyQueryFailedMessage = "Database error occurred while reading the passkeys.";

        /// <summary>
        /// Exception message: Database error occurred while saving the passkeys.
        /// </summary>
        public const string PasskeyUpdateFailedMessage = "Database error occurred while saving the passkeys.";

        /// <summary>
        /// Exception message: Could not delete the recovery code due to a database failure.
        /// </summary>
        public const string PasswordResetDeleteFailedMessage =
            "Could not delete the recovery code due to a database failure.";

        /// <summary>
        /// Exception message: Could not save the recovery code due to a database failure.
        /// </summary>
        public const string PasswordResetSaveFailedMessage =
            "Could not save the recovery code due to a database failure.";

        /// <summary>
        /// Exception message: Could not delete the player due to a database failure.
        /// </summary>
        public const string PlayerDeleteFailedMessage = "Could not delete the player due to a database failure.";

        /// <summary>
        /// Exception message: Database error occurred while reading the player reports.
        /// </summary>
        public const string PlayerReportQueryFailedMessage =
            "Database error occurred while reading the player reports.";

        /// <summary>
        /// Exception message: Database error occurred while saving the player report.
        /// </summary>
        public const string PlayerReportUpdateFailedMessage = "Database error occurred while saving the player report.";

        /// <summary>
        /// Exception message: Could not save the player due to a database failure.
        /// </summary>
        public const string PlayerSaveFailedMessage = "Could not save the player due to a database failure.";

        /// <summary>
        /// Exception message: Database error occurred while calculating player rankings.
        /// </summary>
        public const string RankingFailedMessage = "Database error occurred while calculating player rankings.";

        /// <summary>
        /// Exception message: Database error occurred while reading the recent games.
        /// </summary>
        public const string RecentGamesFailedMessage = "Database error occurred while reading the recent games.";

        /// <summary>
        /// Exception message: Database error occurred while removing the game in progress.
        /// </summary>
        public const string RemoveFailedMessage = "Database error occurred while removing the game in progress.";

        /// <summary>
        /// Exception message: Database error occurred while reading the invitations.
        /// </summary>
        public const string RoomInviteQueryFailedMessage = "Database error occurred while reading the invitations.";

        /// <summary>
        /// Exception message: Database error occurred while saving the invitation.
        /// </summary>
        public const string RoomInviteUpdateFailedMessage = "Database error occurred while saving the invitation.";

        /// <summary>
        /// Exception message: Database error occurred while reading the scorecards of a game.
        /// </summary>
        public const string ScorecardsFailedMessage = "Database error occurred while reading the scorecards of a game.";

        /// <summary>
        /// Exception message: Could not save the state of the account due to a database failure.
        /// </summary>
        public const string StateUpdateFailedMessage =
            "Could not save the state of the account due to a database failure.";

        /// <summary>
        /// Exception message: Could not read the title due to a database failure.
        /// </summary>
        public const string TitleQueryFailedMessage = "Could not read the title due to a database failure.";

        /// <summary>
        /// Exception message: Could not save the title due to a database failure.
        /// </summary>
        public const string TitleUpdateFailedMessage = "Could not save the title due to a database failure.";

        /// <summary>
        /// Exception message: Database error occurred while checking the two-step verification.
        /// </summary>
        public const string TwoFactorQueryFailedMessage =
            "Database error occurred while checking the two-step verification.";

        /// <summary>
        /// Exception message: Could not save the two-step verification setting due to a database failure.
        /// </summary>
        public const string TwoFactorUpdateFailedMessage =
            "Could not save the two-step verification setting due to a database failure.";

        /// <summary>
        /// Exception message: Database error occurred while verifying username existence.
        /// </summary>
        public const string UsernameExistsFailedMessage = "Database error occurred while verifying username existence.";

        /// <summary>
        /// Exception message: Could not save the username due to a database failure.
        /// </summary>
        public const string UsernameUpdateFailedMessage = "Could not save the username due to a database failure.";

        /// <summary>
        /// Exception message: The name belongs to a registered player. Log in to use it.
        /// </summary>
        public const string AccountNameMessage = "The name belongs to a registered player. Log in to use it.";

        /// <summary>
        /// Exception message: The database is not available.
        /// </summary>
        public const string DatabaseFaultMessage = "The database is not available.";

        /// <summary>
        /// Exception message: An achievement cannot use the identifier of the default title.
        /// </summary>
        public const string DefaultTitleIdMessage = "An achievement cannot use the identifier of the default title.";

        /// <summary>
        /// Exception message: The attribute {0} of the element {1} is missing or is not a whole number of zero or more.
        /// </summary>
        public const string GameInvalidNumberFormat =
            "The attribute {0} of the element {1} is missing or is not a whole number of zero or more.";

        /// <summary>
        /// Exception message: The data did not pass the model validation. Properties: {0}.
        /// </summary>
        public const string InvalidEntityFormat = "The data did not pass the model validation. Properties: {0}.";

        /// <summary>
        /// Exception message: The attribute {0} of the element {1} has a value that is not valid.
        /// </summary>
        public const string InvalidValueFormat = "The attribute {0} of the element {1} has a value that is not valid.";

        /// <summary>
        /// Exception message: Player {0}
        /// </summary>
        public const string KnownFormat = "Player {0}";

        /// <summary>
        /// Exception message: The rules of the game have no {0} section.
        /// </summary>
        public const string MissingSectionFormat = "The rules of the game have no {0} section.";

        /// <summary>
        /// Exception message: There is no turn duration for the speed {0}.
        /// </summary>
        public const string MissingSpeedFormat = "There is no turn duration for the speed {0}.";

        /// <summary>
        /// Exception message: A player who is not inside the room cannot join its chat.
        /// </summary>
        public const string NotInsideRoomMessage = "A player who is not inside the room cannot join its chat.";

        /// <summary>
        /// Exception message: Two achievements cannot have the same identifier.
        /// </summary>
        public const string RepeatedIdMessage = "Two achievements cannot have the same identifier.";

        /// <summary>
        /// Exception message: The setting {0} is missing or is not a positive whole number.
        /// </summary>
        public const string ServiceRegistrationInvalidNumberFormat =
            "The setting {0} is missing or is not a positive whole number.";

        /// <summary>
        /// Exception message: The session ended. Log in again.
        /// </summary>
        public const string SessionExpiredMessage = "The session ended. Log in again.";

        /// <summary>
        /// Exception message: The session ended because the player logged in again somewhere else.
        /// </summary>
        public const string SessionReplacedMessage =
            "The session ended because the player logged in again somewhere else.";

        /// <summary>
        /// Exception message: An unexpected error occurred.
        /// </summary>
        public const string UnexpectedFaultMessage = "An unexpected error occurred.";

        /// <summary>
        /// Exception message: A player
        /// </summary>
        public const string UnknownText = "A player";

        /// <summary>
        /// Exception message: Each level must need more experience than the last one.
        /// </summary>
        public const string UnorderedThresholdsMessage = "Each level must need more experience than the last one.";

        /// <summary>
        /// Log entry: A verification code was accepted.
        /// </summary>
        public const string AcceptedLogMessage = "A verification code was accepted.";

        /// <summary>
        /// Log entry: The email about the locked account could not be sent.
        /// </summary>
        public const string AccountLockNotifierSendFailedLogMessage =
            "The email about the locked account could not be sent.";

        /// <summary>
        /// Log entry: {0} was rejected because a guest used the name of an account.
        /// </summary>
        public const string AccountNameLogFormat = "{0} was rejected because a guest used the name of an account.";

        /// <summary>
        /// Log entry: A TCP port is already in use by another application.
        /// </summary>
        public const string AddressInUseLogMessage = "A TCP port is already in use by another application.";

        /// <summary>
        /// Log entry: Player {0} created new backup codes.
        /// </summary>
        public const string BackupCodesLogFormat = "Player {0} created new backup codes.";

        /// <summary>
        /// Log entry: Login attempt with a passkey answer that is not valid.
        /// </summary>
        public const string BadAssertionLogMessage = "Login attempt with a passkey answer that is not valid.";

        /// <summary>
        /// Log entry: A passkey could not be saved because its answer is not valid.
        /// </summary>
        public const string BadRegistrationLogMessage = "A passkey could not be saved because its answer is not valid.";

        /// <summary>
        /// Log entry: A player changed the password from the settings.
        /// </summary>
        public const string ChangedFromSettingsLogMessage = "A player changed the password from the settings.";

        /// <summary>
        /// Log entry: Password changed with a recovery code.
        /// </summary>
        public const string ChangedLogMessage = "Password changed with a recovery code.";

        /// <summary>
        /// Log entry: A game in progress could not be saved for a restart.
        /// </summary>
        public const string CheckpointNotSavedLogMessage = "A game in progress could not be saved for a restart.";

        /// <summary>
        /// Log entry: Login attempt with a passkey whose counter went back.
        /// </summary>
        public const string ClonedLogMessage = "Login attempt with a passkey whose counter went back.";

        /// <summary>
        /// Log entry: Room {0} was closed because it waited too long to start.
        /// </summary>
        public const string ClosedRoomLogFormat = "Room {0} was closed because it waited too long to start.";

        /// <summary>
        /// Log entry: A verification code was sent to player {0}.
        /// </summary>
        public const string CodeSentLogFormat = "A verification code was sent to player {0}.";

        /// <summary>
        /// Log entry: The services could not be started or stopped.
        /// </summary>
        public const string CommunicationFailedLogMessage = "The services could not be started or stopped.";

        /// <summary>
        /// Log entry: The service configuration is not valid.
        /// </summary>
        public const string ConfigurationFailedLogMessage = "The service configuration is not valid.";

        /// <summary>
        /// Log entry: {0} is now online.
        /// </summary>
        public const string ConnectedLogFormat = "{0} is now online.";

        /// <summary>
        /// Log entry: {0} created room {1}.
        /// </summary>
        public const string CreatedLogFormat = "{0} created room {1}.";

        /// <summary>
        /// Log entry: The account of player {0} was locked for the day after wrong passwords.
        /// </summary>
        public const string DailyLockLogFormat =
            "The account of player {0} was locked for the day after wrong passwords.";

        /// <summary>
        /// Log entry: Database error while processing {0}.
        /// </summary>
        public const string DatabaseLogFormat = "Database error while processing {0}.";

        /// <summary>
        /// Log entry: The account of player {0} was deleted.
        /// </summary>
        public const string DeletedLogFormat = "The account of player {0} was deleted.";

        /// <summary>
        /// Log entry: Player {0} turned off the two-step verification.
        /// </summary>
        public const string DisabledLogFormat = "Player {0} turned off the two-step verification.";

        /// <summary>
        /// Log entry: {0} is now offline.
        /// </summary>
        public const string DisconnectedLogFormat = "{0} is now offline.";

        /// <summary>
        /// Log entry: An email could not be delivered to the mail server.
        /// </summary>
        public const string EmailSendFailedLogMessage = "An email could not be delivered to the mail server.";

        /// <summary>
        /// Log entry: Player {0} turned on the two-step verification.
        /// </summary>
        public const string EnabledLogFormat = "Player {0} turned on the two-step verification.";

        /// <summary>
        /// Log entry: The action of an expired countdown failed.
        /// </summary>
        public const string ExpiredActionFailedLogMessage = "The action of an expired countdown failed.";

        /// <summary>
        /// Log entry: The game of room {0} finished.
        /// </summary>
        public const string FinishedLogFormat = "The game of room {0} finished.";

        /// <summary>
        /// Log entry: The games in progress could not be restored.
        /// </summary>
        public const string GamesNotRestoredLogMessage = "The games in progress could not be restored.";

        /// <summary>
        /// Log entry: Games in progress restored after the restart: {0}.
        /// </summary>
        public const string GamesRestoredLogFormat = "Games in progress restored after the restart: {0}.";

        /// <summary>
        /// Log entry: A chat message was ignored because it is empty or its author is not inside the room.
        /// </summary>
        public const string IgnoredMessageLogMessage =
            "A chat message was ignored because it is empty or its author is not inside the room.";

        /// <summary>
        /// Log entry: Invalid data was received in {0}.
        /// </summary>
        public const string InvalidDataLogFormat = "Invalid data was received in {0}.";

        /// <summary>
        /// Log entry: Login attempt with a malformed email.
        /// </summary>
        public const string InvalidEmailLogMessage = "Login attempt with a malformed email.";

        /// <summary>
        /// Log entry: Login attempt with a malformed username.
        /// </summary>
        public const string InvalidUsernameLogMessage = "Login attempt with a malformed username.";

        /// <summary>
        /// Log entry: A player entered room {0}.
        /// </summary>
        public const string JoinedLogFormat = "A player entered room {0}.";

        /// <summary>
        /// Log entry: A player was expelled from room {0} by the host.
        /// </summary>
        public const string KickLogFormat = "A player was expelled from room {0} by the host.";

        /// <summary>
        /// Log entry: A player left room {0}.
        /// </summary>
        public const string LeftLogFormat = "A player left room {0}.";

        /// <summary>
        /// Log entry: Login attempt while the two-step verification is locked.
        /// </summary>
        public const string LockedAccountLogMessage = "Login attempt while the two-step verification is locked.";

        /// <summary>
        /// Log entry: Login attempt while the account of player {0} is locked by wrong passwords.
        /// </summary>
        public const string LockedLoginLogFormat =
            "Login attempt while the account of player {0} is locked by wrong passwords.";

        /// <summary>
        /// Log entry: Player {0} logged in.
        /// </summary>
        public const string LoginLoggedInLogFormat = "Player {0} logged in.";

        /// <summary>
        /// Log entry: Login attempt with a wrong password for player {0}.
        /// </summary>
        public const string LoginWrongPasswordLogFormat = "Login attempt with a wrong password for player {0}.";

        /// <summary>
        /// Log entry: A player is now the host of room {0}.
        /// </summary>
        public const string NewHostLogFormat = "A player is now the host of room {0}.";

        /// <summary>
        /// Log entry: A passkey answer arrived with a challenge that is not valid.
        /// </summary>
        public const string PasskeyExpiredLogMessage = "A passkey answer arrived with a challenge that is not valid.";

        /// <summary>
        /// Log entry: Login attempt with a passkey while the account is locked.
        /// </summary>
        public const string PasskeyLockedLogMessage = "Login attempt with a passkey while the account is locked.";

        /// <summary>
        /// Log entry: Player {0} logged in with a passkey.
        /// </summary>
        public const string PasskeyLoggedInLogFormat = "Player {0} logged in with a passkey.";

        /// <summary>
        /// Log entry: Passkey removal with a wrong password.
        /// </summary>
        public const string PasskeyWrongPasswordLogMessage = "Passkey removal with a wrong password.";

        /// <summary>
        /// Log entry: A wrong current password was typed in a password change.
        /// </summary>
        public const string PasswordWrongPasswordLogMessage =
            "A wrong current password was typed in a password change.";

        /// <summary>
        /// Log entry: Account deletion with a wrong password for player {0}.
        /// </summary>
        public const string ProfileWrongPasswordLogFormat = "Account deletion with a wrong password for player {0}.";

        /// <summary>
        /// Log entry: Player {0} registered a passkey.
        /// </summary>
        public const string RegisteredLogFormat = "Player {0} registered a passkey.";

        /// <summary>
        /// Log entry: Player registered successfully.
        /// </summary>
        public const string RegisteredLogMessage = "Player registered successfully.";

        /// <summary>
        /// Log entry: {0} was rejected because the session is not valid.
        /// </summary>
        public const string RejectedSessionLogFormat = "{0} was rejected because the session is not valid.";

        /// <summary>
        /// Log entry: A connection was rejected because the session is not valid.
        /// </summary>
        public const string RejectedSessionLogMessage = "A connection was rejected because the session is not valid.";

        /// <summary>
        /// Log entry: {0} accounts without a confirmed email were deleted.
        /// </summary>
        public const string RemovedAccountsLogFormat = "{0} accounts without a confirmed email were deleted.";

        /// <summary>
        /// Log entry: Player {0} removed a passkey.
        /// </summary>
        public const string RemovedLogFormat = "Player {0} removed a passkey.";

        /// <summary>
        /// Log entry: Player {0} reported player {1}.
        /// </summary>
        public const string ReportSentLogFormat = "Player {0} reported player {1}.";

        /// <summary>
        /// Log entry: A password recovery code was requested for an account.
        /// </summary>
        public const string RequestedLogMessage = "A password recovery code was requested for an account.";

        /// <summary>
        /// Log entry: The result of a finished game could not be saved.
        /// </summary>
        public const string ResultNotSavedLogMessage = "The result of a finished game could not be saved.";

        /// <summary>
        /// Log entry: The email {0} could not be sent.
        /// </summary>
        public const string SendFailedLogFormat = "The email {0} could not be sent.";

        /// <summary>
        /// Log entry: The email {0} was sent.
        /// </summary>
        public const string SentLogFormat = "The email {0} was sent.";

        /// <summary>
        /// Log entry: The email about the locked account was sent.
        /// </summary>
        public const string SentLogMessage = "The email about the locked account was sent.";

        /// <summary>
        /// Log entry: The game of room {0} started with {1} players.
        /// </summary>
        public const string StartedLogFormat = "The game of room {0} started with {1} players.";

        /// <summary>
        /// Log entry: Server started. Press Enter or Ctrl+C to stop it.
        /// </summary>
        public const string StartedLogMessage = "Server started. Press Enter or Ctrl+C to stop it.";

        /// <summary>
        /// Log entry: Server stopped gracefully.
        /// </summary>
        public const string StoppedLogMessage = "Server stopped gracefully.";

        /// <summary>
        /// Log entry: Player {0} was suspended after reports from several players.
        /// </summary>
        public const string SuspendedLogFormat = "Player {0} was suspended after reports from several players.";

        /// <summary>
        /// Log entry: Login attempt with a password longer than the maximum.
        /// </summary>
        public const string TooLongPasswordLogMessage = "Login attempt with a password longer than the maximum.";

        /// <summary>
        /// Log entry: Recovery code discarded after too many wrong attempts.
        /// </summary>
        public const string TooManyAttemptsLogMessage = "Recovery code discarded after too many wrong attempts.";

        /// <summary>
        /// Log entry: A wrong password was typed in a security change of player {0}.
        /// </summary>
        public const string TwoFactorSettingsWrongPasswordLogFormat =
            "A wrong password was typed in a security change of player {0}.";

        /// <summary>
        /// Log entry: A verification code that expired was typed.
        /// </summary>
        public const string TwoFactorVerificationExpiredLogMessage = "A verification code that expired was typed.";

        /// <summary>
        /// Log entry: The verification codes are locked after too many wrong tries.
        /// </summary>
        public const string TwoFactorVerificationLockedLogMessage =
            "The verification codes are locked after too many wrong tries.";

        /// <summary>
        /// Log entry: Unexpected error while processing {0}.
        /// </summary>
        public const string UnexpectedLogFormat = "Unexpected error while processing {0}.";

        /// <summary>
        /// Log entry: An error that no service handled is closing the server.
        /// </summary>
        public const string UnhandledErrorLogMessage = "An error that no service handled is closing the server.";

        /// <summary>
        /// Log entry: Login attempt with a passkey that is not registered.
        /// </summary>
        public const string UnknownCredentialLogMessage = "Login attempt with a passkey that is not registered.";

        /// <summary>
        /// Log entry: Login attempt with an email that is not registered.
        /// </summary>
        public const string UnknownEmailLogMessage = "Login attempt with an email that is not registered.";

        /// <summary>
        /// Log entry: Login attempt with a username that is not registered.
        /// </summary>
        public const string UnknownUsernameLogMessage = "Login attempt with a username that is not registered.";

        /// <summary>
        /// Log entry: The database could not be warmed up; the first request will be slower.
        /// </summary>
        public const string WarmUpFailedLogMessage =
            "The database could not be warmed up; the first request will be slower.";

        /// <summary>
        /// Log entry: A wrong verification code was typed.
        /// </summary>
        public const string WrongLogMessage = "A wrong verification code was typed.";
    }
}
