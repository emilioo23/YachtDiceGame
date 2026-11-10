using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.AuthenticationServiceReference;
using AuthFault = YachtDice.AuthenticationServiceReference.ServiceFault;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Single entry point of the client to the authentication operations of the server.
    /// </summary>
    public class AuthenticationGateway : GatewayBase<AuthenticationServiceClient, IAuthenticationService>
    {
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
        /// <param name="code">Verification code entered by the player.</param>
        /// <param name="challengeId">Unique challenge identifier tied to the login session.</param>
        /// <returns>Result of the validation.</returns>
        public async Task<VerificationResultCode> VerifyCodeAsync(string email, string code, string challengeId)
        {
            VerificationResultCode result =
                await ExecuteAsync(client => client.VerifyPlayerCode(email, code, challengeId));

            return result;
        }

        /// <summary>
        /// Asks the server to send a new verification code.
        /// </summary>
        /// <param name="email">Email address to send the new code to.</param>
        /// <param name="challengeId">Unique challenge identifier tied to the login session.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        public async Task<ResendResultCode> ResendCodeAsync(string email, string challengeId, string culture)
        {
            ResendResultCode result =
                await ExecuteAsync(client => client.ResendPlayerCode(email, challengeId, culture));

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