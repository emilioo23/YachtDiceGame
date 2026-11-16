using System.ServiceModel;

namespace YachtDice.Server.Contracts.TwoFactor
{
    /// <summary>
    /// Defines the service contract for managing two-factor authentication settings.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface ITwoFactorManagementService
    {
        /// <summary>
        /// Indicates whether the player has the two-step verification turned on.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>True if the player must enter a code to log in.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        bool IsTwoFactorEnabled(string displayName);

        /// <summary>
        /// Starts turning on the two-step verification by sending a code to the player's email.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        /// <returns>Outcome of the request and the data of the challenge.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        TwoFactorActivationData StartTwoFactorActivation(string sessionToken, string culture);

        /// <summary>
        /// Checks the code entered by the player and, if correct, turns on the two-step verification.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="verificationCode">Code entered by the player and the challenge of the activation.</param>
        /// <returns>Result of the validation.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        VerificationResultCode ConfirmTwoFactorActivation(string sessionToken, VerificationCode verificationCode);

        /// <summary>
        /// Sends a new code while the player is turning on the two-step verification.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="delivery">Challenge of the activation and culture preferred by the player.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResendResultCode ResendActivationCode(string sessionToken, CodeDelivery delivery);

        /// <summary>
        /// Turns off the two-step verification after checking the player's password.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="password">Current password of the player.</param>
        /// <returns>Outcome of the request.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        TwoFactorChangeResultCode DisableTwoFactor(string sessionToken, string password);

        /// <summary>
        /// Changes the password of the player who is logged in.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="change">Current password and new password.</param>
        /// <returns>The result of the change.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        PasswordChangeResultCode ChangePassword(string sessionToken, PasswordChange change);

        /// <summary>
        /// Creates new backup codes after checking the player's password. The codes created before stop working.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="password">Current password of the player.</param>
        /// <returns>Outcome of the request and the new codes.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        BackupCodesData CreateBackupCodes(string sessionToken, string password);

        /// <summary>
        /// Counts the backup codes that the player has not used.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The number of backup codes left.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        int CountBackupCodes(string sessionToken);
    }
}