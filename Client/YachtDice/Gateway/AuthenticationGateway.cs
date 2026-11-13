using System;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.AuthenticationServiceReference;
using AuthFault = YachtDice.AuthenticationServiceReference.ServiceFault;
using ResendInput = YachtDice.Client.Logic.Authentication.ResendCodeInput;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Single entry point of the client to the authentication operations of the server.
    /// </summary>
    public class AuthenticationGateway : GatewayBase<AuthenticationServiceClient, IAuthenticationService>
    {
        private const string NullResendInputMessage = "The data to resend the code cannot be null.";

        /// <summary>
        /// Opens and closes a connection to the server so the first real call does not pay for the start-up.
        /// </summary>
        /// <returns>A task that finishes when the connection was opened.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public async Task WarmUpAsync()
        {
            await ExecuteAsync(client =>
            {
                client.Open();
                return true;
            });
        }

        /// <summary>
        /// Checks the credentials and asks the server to send the verification code.
        /// </summary>
        /// <param name="credentials">Login credentials of the player.</param>
        /// <returns>Outcome and initial session data of the login attempt.</returns>
        public async Task<LoginData> LoginAsync(LoginCredentials credentials)
        {
            LoginData loginData = await ExecuteAsync(client => client.LoginPlayer(credentials));

            return loginData;
        }

        /// <summary>
        /// Validates the verification code entered by the player.
        /// </summary>
        /// <param name="email">Email address the code was sent to.</param>
        /// <param name="verificationCode">Code entered by the player and the challenge of the login session.</param>
        /// <returns>Result of the validation.</returns>
        public async Task<VerificationResultCode> VerifyCodeAsync(string email, VerificationCode verificationCode)
        {
            VerificationResultCode result =
                await ExecuteAsync(client => client.VerifyPlayerCode(email, verificationCode));

            return result;
        }

        /// <summary>
        /// Asks the server to send a new verification code.
        /// </summary>
        /// <param name="email">Email address to send the new code to.</param>
        /// <param name="input">Challenge of the login session and culture preferred by the player.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        /// <exception cref="ArgumentNullException">The input is null.</exception>
        public async Task<ResendResultCode> ResendCodeAsync(string email, ResendInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), NullResendInputMessage);
            }

            var delivery = new CodeDelivery { ChallengeId = input.ChallengeId, Culture = input.Culture };
            ResendResultCode result = await ExecuteAsync(client => client.ResendPlayerCode(email, delivery));

            return result;
        }

        /// <summary>
        /// Asks the server to send a password recovery code to the email of the account.
        /// </summary>
        /// <param name="email">Email of the account to recover.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        /// <returns>Outcome of the request.</returns>
        public async Task<PasswordResetRequestResultCode> RequestPasswordResetAsync(string email, string culture)
        {
            PasswordResetRequestResultCode result =
                await ExecuteAsync(client => client.RequestPasswordReset(email, culture));

            return result;
        }

        /// <summary>
        /// Sends the recovery code and the new password so the server changes the password of the account.
        /// </summary>
        /// <param name="data">Email, code and new password written by the player.</param>
        /// <returns>Outcome of the confirmation.</returns>
        public async Task<PasswordResetResultCode> ConfirmPasswordResetAsync(PasswordResetData data)
        {
            PasswordResetResultCode result = await ExecuteAsync(client => client.ConfirmPasswordReset(data));

            return result;
        }

        /// <summary>
        /// Translates the WCF fault exception into a specific gateway error code.
        /// </summary>
        /// <param name="fault">The exception thrown by the service.</param>
        /// <returns>The translated gateway error code.</returns>
        protected override GatewayErrorCode GetErrorCode(FaultException fault)
        {
            var typedFault = fault as FaultException<AuthFault>;

            return GatewayErrorCodeParser.Parse(typedFault?.Detail.Code);
        }
    }
}