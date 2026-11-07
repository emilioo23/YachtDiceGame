using System;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Translates the error codes of the faults of the server. Every service reference generates its own
    /// enumeration, but they all share the names of <see cref="GatewayErrorCode"/>.
    /// </summary>
    internal static class GatewayErrorCodeParser
    {
        /// <summary>
        /// Gets the gateway error code with the same name as the code sent by the server.
        /// </summary>
        /// <param name="serverCode">Code of the fault; it is null if the fault has no known detail.</param>
        /// <returns>The matching error code, or the unexpected error if there is none.</returns>
        public static GatewayErrorCode Parse(object serverCode)
        {
            GatewayErrorCode errorCode;

            if (!Enum.TryParse(Convert.ToString(serverCode), out errorCode))
            {
                errorCode = GatewayErrorCode.UnexpectedError;
            }

            return errorCode;
        }
    }
}
