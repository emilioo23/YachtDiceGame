using System.ServiceModel;

namespace YachtDice.Server.Contracts
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
        /// <param name="displayName">Name of the player.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        /// <returns>Outcome of the request and the data of the challenge.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        TwoFactorActivationData StartTwoFactorActivation(string displayName, string culture);

        /// <summary>
        /// Checks the code entered by the player and, if correct, turns on the two-step verification.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="code">Verification code entered by the player.</param>
        /// <param name="challengeId">Unique challenge identifier of the activation.</param>
        /// <returns>Result of the validation.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        VerificationResultCode ConfirmTwoFactorActivation(string displayName, string code, string challengeId);

        /// <summary>
        /// Sends a new code while the player is turning on the two-step verification.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="challengeId">Unique challenge identifier of the activation.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResendResultCode ResendActivationCode(string displayName, string challengeId, string culture);

        /// <summary>
        /// Turns off the two-step verification after checking the player's password.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="password">Current password of the player.</param>
        /// <returns>Outcome of the request.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        TwoFactorChangeResultCode DisableTwoFactor(string displayName, string password);
    }
}