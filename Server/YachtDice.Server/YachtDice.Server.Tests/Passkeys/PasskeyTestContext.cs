using YachtDice.Server.Data;
using YachtDice.Server.Logic.Passkeys;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Passkeys
{
    /// <summary>
    /// Service of the passkeys with two registered players and without a database, shared by the tests.
    /// </summary>
    internal sealed class PasskeyTestContext
    {
        /// <summary>Identifier of the first player.</summary>
        public const int PlayerId = 1;

        /// <summary>Identifier of the second player.</summary>
        public const int OtherPlayerId = 2;

        /// <summary>Display name of the first player.</summary>
        public const string DisplayName = "gabrielhernandez";

        /// <summary>First name of the first player.</summary>
        public const string FirstName = "Gabriel";

        /// <summary>Display name of the second player.</summary>
        public const string OtherDisplayName = "emilioalvarez";

        /// <summary>First name of the second player.</summary>
        public const string OtherFirstName = "Emilio";

        /// <summary>
        /// Initializes the service with two players.
        /// </summary>
        public PasskeyTestContext()
        {
            Players = new InMemoryPlayerRepository();
            Passkeys = new InMemoryPasskeyRepository(Players);
            Players.Add(new Player { Id = PlayerId, DisplayName = DisplayName, FirstName = FirstName });
            Players.Add(new Player { Id = OtherPlayerId, DisplayName = OtherDisplayName, FirstName = OtherFirstName });
            Service = new PasskeyService(Players, Passkeys, new PasskeyChallengeStore());
        }

        /// <summary>
        /// Players of the test.
        /// </summary>
        public InMemoryPlayerRepository Players { get; }

        /// <summary>
        /// Passkeys saved by the test.
        /// </summary>
        public InMemoryPasskeyRepository Passkeys { get; }

        /// <summary>
        /// Service under test.
        /// </summary>
        public PasskeyService Service { get; }

        /// <summary>
        /// Registers a passkey of the authenticator for the first player.
        /// </summary>
        /// <param name="authenticator">Authenticator that creates the passkey.</param>
        /// <param name="options">Values of the answer of the authenticator.</param>
        /// <returns>Outcome of the registration.</returns>
        public PasskeyRegistrationStatus Register(FakeAuthenticator authenticator, AuthenticatorOptions options)
        {
            PasskeyChallenge challenge = Service.BeginRegistration(DisplayName);

            return Service.CompleteRegistration(DisplayName, authenticator.CreateAttestation(challenge, options));
        }
    }
}
