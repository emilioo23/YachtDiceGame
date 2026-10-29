using System;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts;
    using Data = YachtDice.Server.Data;
    using LogicAuth = YachtDice.Server.Logic.Authentication;

    /// <summary>
    /// Implements the player registration service contract for the server.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PlayerRegistrationService : ServiceBase, Contracts.IPlayerRegistrationService
    {
        private readonly LogicAuth.IRegistrationService _registrationLogic;

        /// <summary>
        /// Initializes a new instance of the PlayerRegistrationService class.
        /// </summary>
        /// <param name="registrationLogic">The registration logic service.</param>
        public PlayerRegistrationService(LogicAuth.IRegistrationService registrationLogic)
        {
            _registrationLogic = registrationLogic ?? throw new ArgumentNullException(nameof(registrationLogic));
        }

        /// <summary>
        /// Registers a new player.
        /// </summary>
        /// <param name="newPlayer">Data of the player who wants to register.</param>
        /// <returns>A code indicating the result of the registration attempt.</returns>
        public Contracts.RegisterResultCode RegisterPlayer(Contracts.Player newPlayer)
        {
            return Execute(newPlayer, () => ProcessRegistration(newPlayer));
        }

        private Contracts.RegisterResultCode ProcessRegistration(Contracts.Player newPlayer)
        {
            Data.Player logicPlayer = Mapping.RegistrationMapper.ToLogicModel(newPlayer);
            LogicAuth.RegistrationResult logicResult =
                _registrationLogic.RegisterPlayer(logicPlayer, newPlayer.Password);

            return Mapping.EnumMapper.Map<LogicAuth.RegistrationResult, Contracts.RegisterResultCode>(logicResult);
        }
    }
}
