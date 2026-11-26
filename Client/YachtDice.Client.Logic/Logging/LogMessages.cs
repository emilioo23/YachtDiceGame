namespace YachtDice.Client.Logic.Logging
{
    /// <summary>
    /// Texts that only the developer sees: the messages of the exceptions and the entries of the log.
    /// Each text is written once here and the classes of the client use it.
    /// </summary>
    public static class LogMessages
    {
        /// <summary>
        /// Exception message: The window that opens it cannot be null.
        /// </summary>
        public const string HowToPlayNullOwnerMessage = "The window that opens it cannot be null.";

        /// <summary>
        /// Exception message: The window that shows the invitations cannot be null.
        /// </summary>
        public const string InvitationNullOwnerMessage = "The window that shows the invitations cannot be null.";

        /// <summary>
        /// Exception message: The choices about the sound cannot be null.
        /// </summary>
        public const string NullAudioMessage = "The choices about the sound cannot be null.";

        /// <summary>
        /// Exception message: The canvas where it is drawn cannot be null.
        /// </summary>
        public const string NullCanvasMessage = "The canvas where it is drawn cannot be null.";

        /// <summary>
        /// Exception message: The connection to the chat cannot be null.
        /// </summary>
        public const string NullChatMessage = "The connection to the chat cannot be null.";

        /// <summary>
        /// Exception message: The players of the columns cannot be null.
        /// </summary>
        public const string NullColumnsMessage = "The players of the columns cannot be null.";

        /// <summary>
        /// Exception message: The bytes to encode cannot be null.
        /// </summary>
        public const string NullDataMessage = "The bytes to encode cannot be null.";

        /// <summary>
        /// Exception message: The delegate of the reconnect loop cannot be null.
        /// </summary>
        public const string NullDelegateMessage = "The delegate of the reconnect loop cannot be null.";

        /// <summary>
        /// Exception message: A shared component of the client cannot be null.
        /// </summary>
        public const string NullDependencyMessage = "A shared component of the client cannot be null.";

        /// <summary>
        /// Exception message: The email of the account cannot be null.
        /// </summary>
        public const string NullEmailMessage = "The email of the account cannot be null.";

        /// <summary>
        /// Exception message: The exception to log cannot be null.
        /// </summary>
        public const string NullExceptionMessage = "The exception to log cannot be null.";

        /// <summary>
        /// Exception message: The first name of the player cannot be null.
        /// </summary>
        public const string NullFirstNameMessage = "The first name of the player cannot be null.";

        /// <summary>
        /// Exception message: The folder of the images cannot be null.
        /// </summary>
        public const string NullFolderMessage = "The folder of the images cannot be null.";

        /// <summary>
        /// Exception message: The choices of the player about the help cannot be null.
        /// </summary>
        public const string NullGameplayMessage = "The choices of the player about the help cannot be null.";

        /// <summary>
        /// Exception message: The connection of the match cannot be null.
        /// </summary>
        public const string NullGatewayMessage = "The connection of the match cannot be null.";

        /// <summary>
        /// Exception message: The membership of the player in the room cannot be null.
        /// </summary>
        public const string NullMembershipMessage = "The membership of the player in the room cannot be null.";

        /// <summary>
        /// Exception message: The action of the click cannot be null.
        /// </summary>
        public const string NullOnClickMessage = "The action of the click cannot be null.";

        /// <summary>
        /// Exception message: The type that owns the logger cannot be null.
        /// </summary>
        public const string NullOwnerTypeMessage = "The type that owns the logger cannot be null.";

        /// <summary>
        /// Exception message: The panel where it is drawn cannot be null.
        /// </summary>
        public const string NullPanelMessage = "The panel where it is drawn cannot be null.";

        /// <summary>
        /// Exception message: The identifier of the player cannot be null.
        /// </summary>
        public const string NullPlayerIdMessage = "The identifier of the player cannot be null.";

        /// <summary>
        /// Exception message: The name of the player cannot be null.
        /// </summary>
        public const string NullPlayerNameMessage = "The name of the player cannot be null.";

        /// <summary>
        /// Exception message: The name of the reported player cannot be null.
        /// </summary>
        public const string NullReportedNameMessage = "The name of the reported player cannot be null.";

        /// <summary>
        /// Exception message: The element that holds the resources cannot be null.
        /// </summary>
        public const string NullResourcesMessage = "The element that holds the resources cannot be null.";

        /// <summary>
        /// Exception message: The code of the room cannot be null.
        /// </summary>
        public const string NullRoomCodeMessage = "The code of the room cannot be null.";

        /// <summary>
        /// Exception message: The connection to the room cannot be null.
        /// </summary>
        public const string NullRoomGatewayMessage = "The connection to the room cannot be null.";

        /// <summary>
        /// Exception message: The saved scorecards cannot be null.
        /// </summary>
        public const string NullScorecardsMessage = "The saved scorecards cannot be null.";

        /// <summary>
        /// Exception message: The shared components of the client cannot be null.
        /// </summary>
        public const string NullServicesMessage = "The shared components of the client cannot be null.";

        /// <summary>
        /// Exception message: The session of the player cannot be null.
        /// </summary>
        public const string NullSessionMessage = "The session of the player cannot be null.";

        /// <summary>
        /// Exception message: The audio settings of the music cannot be null.
        /// </summary>
        public const string NullSettingsMessage = "The audio settings of the music cannot be null.";

        /// <summary>
        /// Exception message: The final places cannot be null.
        /// </summary>
        public const string NullStandingsMessage = "The final places cannot be null.";

        /// <summary>
        /// Exception message: The state of the game cannot be null.
        /// </summary>
        public const string NullStateMessage = "The state of the game cannot be null.";

        /// <summary>
        /// Exception message: The status text cannot be null.
        /// </summary>
        public const string NullStatusMessage = "The status text cannot be null.";

        /// <summary>
        /// Exception message: The verification challenge cannot be null.
        /// </summary>
        public const string TwoFactorNullChallengeMessage = "The verification challenge cannot be null.";

        /// <summary>
        /// Exception message: The challenge of the server cannot be null.
        /// </summary>
        public const string WindowsPasskeyNullChallengeMessage = "The challenge of the server cannot be null.";

        /// <summary>
        /// Exception message: Windows could not complete the passkey operation. HRESULT: 0x{0:X8}.
        /// </summary>
        public const string CallFailedMessage = "Windows could not complete the passkey operation. HRESULT: 0x{0:X8}.";

        /// <summary>
        /// Exception message: A picture cannot be converted back to an avatar identifier.
        /// </summary>
        public const string ConvertBackMessage = "A picture cannot be converted back to an avatar identifier.";

        /// <summary>
        /// Exception message: The challenge of the server cannot be empty.
        /// </summary>
        public const string EmptyChallengeMessage = "The challenge of the server cannot be empty.";

        /// <summary>
        /// Exception message: The type of the ceremony cannot be empty.
        /// </summary>
        public const string EmptyTypeMessage = "The type of the ceremony cannot be empty.";

        /// <summary>
        /// Exception message: The server sent a value that is not valid base64url.
        /// </summary>
        public const string InvalidBase64Message = "The server sent a value that is not valid base64url.";

        /// <summary>
        /// Exception message: The position of the die is not one of the five dice.
        /// </summary>
        public const string InvalidDieMessage = "The position of the die is not one of the five dice.";

        /// <summary>
        /// Exception message: The face of a die goes from one to six.
        /// </summary>
        public const string InvalidFaceMessage = "The face of a die goes from one to six.";

        /// <summary>
        /// Exception message: The maximum value must be greater than zero.
        /// </summary>
        public const string InvalidMaximumMessage = "The maximum value must be greater than zero.";

        /// <summary>
        /// Exception message: The position of the die must be inside the dice of the row.
        /// </summary>
        public const string InvalidRackMessage = "The position of the die must be inside the dice of the row.";

        /// <summary>
        /// Exception message: The value of a die goes from zero to six.
        /// </summary>
        public const string InvalidValueMessage = "The value of a die goes from zero to six.";

        /// <summary>
        /// Exception message: Windows did not return the answer of the passkey operation.
        /// </summary>
        public const string NoAnswerMessage = "Windows did not return the answer of the passkey operation.";

        /// <summary>
        /// Exception message: There is no connection with the server.
        /// </summary>
        public const string NotConnectedMessage = "There is no connection with the server.";

        /// <summary>
        /// Exception message: The server could not complete the request.
        /// </summary>
        public const string ServerFaultMessage = "The server could not complete the request.";

        /// <summary>
        /// Exception message: The server is not available.
        /// </summary>
        public const string ServerUnavailableMessage = "The server is not available.";

        /// <summary>
        /// Exception message: The server presented a certificate whose thumbprint is not the one of the game.
        /// </summary>
        public const string UnknownCertificateMessage =
            "The server presented a certificate whose thumbprint is not the one of the game.";

        /// <summary>
        /// Log entry: An attempt to reconnect to the match failed: {0}.
        /// </summary>
        public const string AttemptFailedLogFormat = "An attempt to reconnect to the match failed: {0}.";

        /// <summary>
        /// Log entry: The list of rooms could not be updated.
        /// </summary>
        public const string AutoRefreshFailedLogMessage = "The list of rooms could not be updated.";

        /// <summary>
        /// Log entry: The game client closed.
        /// </summary>
        public const string ClosedLogMessage = "The game client closed.";

        /// <summary>
        /// Log entry: The connection with the server was lost.
        /// </summary>
        public const string ConnectionLostLogMessage = "The connection with the server was lost.";

        /// <summary>
        /// Log entry: The room code could not be copied to the clipboard.
        /// </summary>
        public const string CopyFailedLogMessage = "The room code could not be copied to the clipboard.";

        /// <summary>
        /// Log entry: The connection could not be closed normally, so it was aborted.
        /// </summary>
        public const string DuplexGatewayCloseFailedLogMessage =
            "The connection could not be closed normally, so it was aborted.";

        /// <summary>
        /// Log entry: The server did not give back the place in the game: {0}.
        /// </summary>
        public const string GameRefusedLogFormat = "The server did not give back the place in the game: {0}.";

        /// <summary>
        /// Log entry: The player could not join the chat of the room.
        /// </summary>
        public const string JoinFailedLogMessage = "The player could not join the chat of the room.";

        /// <summary>
        /// Log entry: The saved preferences could not be read, the defaults are used.
        /// </summary>
        public const string LoadFailedLogMessage = "The saved preferences could not be read, the defaults are used.";

        /// <summary>
        /// Log entry: Windows could not use the passkey to log in.
        /// </summary>
        public const string LoginPasskeyFailedLogMessage = "Windows could not use the passkey to log in.";

        /// <summary>
        /// Log entry: The background music could not be played.
        /// </summary>
        public const string MusicFailedLogMessage = "The background music could not be played.";

        /// <summary>
        /// Log entry: A sound effect could not be played.
        /// </summary>
        public const string PlayFailedLogMessage = "A sound effect could not be played.";

        /// <summary>
        /// Log entry: The presence connection could not be closed, so it was aborted.
        /// </summary>
        public const string PresenceCloseFailedLogMessage =
            "The presence connection could not be closed, so it was aborted.";

        /// <summary>
        /// Log entry: The presence service could not be reached.
        /// </summary>
        public const string PresenceUnavailableLogMessage = "The presence service could not be reached.";

        /// <summary>
        /// Log entry: The connection with the match was restored.
        /// </summary>
        public const string RestoredLogMessage = "The connection with the match was restored.";

        /// <summary>
        /// Log entry: The server did not give back the place in the room: {0}.
        /// </summary>
        public const string RoomRefusedLogFormat = "The server did not give back the place in the room: {0}.";

        /// <summary>
        /// Log entry: The preferences of the player could not be saved.
        /// </summary>
        public const string SaveFailedLogMessage = "The preferences of the player could not be saved.";

        /// <summary>
        /// Log entry: The server reported an error.
        /// </summary>
        public const string ServerFaultLogMessage = "The server reported an error.";

        /// <summary>
        /// Log entry: The server could not be reached.
        /// </summary>
        public const string ServerUnavailableLogMessage = "The server could not be reached.";

        /// <summary>
        /// Log entry: Windows could not create the passkey.
        /// </summary>
        public const string SettingsPasskeyFailedLogMessage = "Windows could not create the passkey.";

        /// <summary>
        /// Log entry: The game client started.
        /// </summary>
        public const string StartedLogMessage = "The game client started.";

        /// <summary>
        /// Log entry: Unexpected exception during a call to the server.
        /// </summary>
        public const string UnexpectedErrorLogMessage = "Unexpected exception during a call to the server.";

        /// <summary>
        /// Log entry: An error that no window handled reached the application.
        /// </summary>
        public const string UnhandledErrorLogMessage = "An error that no window handled reached the application.";

        /// <summary>
        /// Log entry.
        /// </summary>
        public const string UnknownCertificateLogMessage =
            "The server presented a certificate that is not the one of the game, so the connection was refused.";

        /// <summary>
        /// Log entry: A background task failed and nobody observed its error.
        /// </summary>
        public const string UnobservedTaskLogMessage = "A background task failed and nobody observed its error.";

        /// <summary>
        /// Log entry: The connection to the server could not be warmed up.
        /// </summary>
        public const string WarmUpFailedLogMessage = "The connection to the server could not be warmed up.";

        /// <summary>
        /// Log entry: This Windows does not have the passkey API, so passkeys are off.
        /// </summary>
        public const string WindowsPasskeyUnavailableLogMessage =
            "This Windows does not have the passkey API, so passkeys are off.";
    }
}
