using System;
using System.ServiceModel;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Services.Mapping;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts;
    using Data = YachtDice.Server.Data;
    using LogicAuth = YachtDice.Server.Logic.Authentication;

    /// <summary>
    /// Implements the player registration service contract for the server.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PlayerRegistrationService : ServiceBase, Contracts.IPlayerRegistrationManager
    {
        private readonly IRegistrationService _registrationLogic;

        /// <summary>
        /// Initializes a new instance of the PlayerRegistrationService class with default dependencies.
        /// </summary>
        public PlayerRegistrationService()
        {
            _registrationLogic = new RegistrationService(new Data.Repositories.PlayerRepository());
        }

        /// <summary>
        /// Initializes a new instance of the PlayerRegistrationService class with a specified player repository.
        /// </summary>
        /// <param name="playerRepository">The player repository to use for registration data access.</param>
        public PlayerRegistrationService(Data.Repositories.IPlayerRepository playerRepository)
        {
            if (playerRepository == null)
            {
                throw new ArgumentNullException(nameof(playerRepository));
            }

            _registrationLogic = new RegistrationService(playerRepository);
        }

        /// <summary>
        /// Initializes a new instance of the PlayerRegistrationService class with a specified registration logic service.
        /// </summary>
        /// <param name="registrationLogic">The registration logic service.</param>
        public PlayerRegistrationService(IRegistrationService registrationLogic)
        {
            _registrationLogic = registrationLogic ?? throw new ArgumentNullException(nameof(registrationLogic));
        }

        /// <summary>
        /// Registers a new player using the provided player entity.
        /// </summary>
        /// <param name="player">The player entity to register.</param>
        /// <returns>A code indicating the result of the registration attempt.</returns>
        public Contracts.RegisterResultCode Register(Contracts.Player player)
        {
            return Execute(player, () => ProcessRegistration(player));
        }

        private Contracts.RegisterResultCode ProcessRegistration(Contracts.Player player)
        {
            Data.Player logicPlayer = RegistrationMapper.ToLogicModel(player);
            LogicAuth.RegistrationResult logicResult = _registrationLogic.Register(logicPlayer, player.Password);

            return EnumMapper.Map<LogicAuth.RegistrationResult, Contracts.RegisterResultCode>(logicResult);
        }
    }
}