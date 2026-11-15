using System;
using System.Globalization;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts.Players;
    using Data = YachtDice.Server.Data;
    using LogicAuth = YachtDice.Server.Logic.Authentication;

    /// <summary>
    /// Implements the player registration service contract for the server.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class PlayerRegistrationService : ServiceBase, Contracts.IPlayerRegistrationService
    {
        private const string NullDependencyMessage = "A dependency of the player registration service cannot be null.";

        private const int CleanupIntervalSeconds = 3600;
        private const int NoTurnNumber = 0;
        private const string RemovedAccountsLogFormat = "{0} accounts without a confirmed email were deleted.";

        private readonly LogicAuth.IRegistrationService _registrationLogic;
        private TurnClock _cleanupClock;

        /// <summary>
        /// Initializes a new instance of the PlayerRegistrationService class.
        /// </summary>
        /// <param name="registrationLogic">The registration logic service.</param>
        public PlayerRegistrationService(LogicAuth.IRegistrationService registrationLogic)
        {
            _registrationLogic = registrationLogic
                ?? throw new ArgumentNullException(nameof(registrationLogic), NullDependencyMessage);
            _cleanupClock = StartCleanupClock();
        }

        /// <summary>
        /// Registers a new player.
        /// </summary>
        /// <param name="newPlayer">Data of the player who wants to register.</param>
        /// <returns>A code indicating the result of the registration attempt.</returns>
        public Contracts.RegisterResultCode RegisterPlayer(Contracts.Player newPlayer)
        {
            Contracts.RegisterResultCode result = Execute(newPlayer, () => ProcessRegistration(newPlayer));

            return result;
        }

        private TurnClock StartCleanupClock()
        {
            return new TurnClock(NoTurnNumber, CleanupIntervalSeconds, RemoveUnverifiedAccounts);
        }

        private void RemoveUnverifiedAccounts()
        {
            try
            {
                int removedCount = _registrationLogic.RemoveUnverifiedAccounts(DateTime.UtcNow);

                LogInfo(string.Format(CultureInfo.InvariantCulture, RemovedAccountsLogFormat, removedCount));
            }
            finally
            {
                _cleanupClock.Dispose();
                _cleanupClock = StartCleanupClock();
            }
        }

        private Contracts.RegisterResultCode ProcessRegistration(Contracts.Player newPlayer)
        {
            Data.Player logicPlayer = Mapping.RegistrationMapper.ToEntity(newPlayer);
            LogicAuth.RegistrationResult logicResult =
                _registrationLogic.RegisterPlayer(logicPlayer, newPlayer.Password);

            return Mapping.EnumMapper.Map<LogicAuth.RegistrationResult, Contracts.RegisterResultCode>(logicResult);
        }
    }
}
