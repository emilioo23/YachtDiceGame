using System;
using System.ServiceModel;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Services.Contracts;
using YachtDice.Server.Services.Mapping;

namespace YachtDice.Server.Services
{
    /// <summary>
    /// Implements the player registration service contract for the server.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PlayerRegistrationService : IPlayerRegistrationManager
    {
        private readonly IRegistrationService _registrationLogic;

        /// <summary>
        /// Initializes a new instance of the PlayerRegistrationService class with default dependencies.
        /// </summary>
        public PlayerRegistrationService()
        {
            _registrationLogic = new RegistrationService(new YachtDice.Server.Data.Repositories.PlayerRepository());
        }

        /// <summary>
        /// Initializes a new instance of the PlayerRegistrationService class with a specified player repository.
        /// </summary>
        /// <param name="playerRepository">The player repository to use for registration data access.</param>
        public PlayerRegistrationService(YachtDice.Server.Data.Repositories.IPlayerRepository playerRepository)
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
        /// Registers a new player using the provided registration information.
        /// </summary>
        /// <param name="request">The player registration information.</param>
        /// <returns>A code indicating the result of the registration attempt.</returns>
        public RegisterResultCode Register(Contracts.PlayerRegistrationInfo request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                Logic.Authentication.PlayerRegistrationInfo logicInfo = RegistrationMapper.ToRegisterRequest(request);
                RegistrationResult logicResult = _registrationLogic.Register(logicInfo);

                return EnumMapper.Map<RegistrationResult, RegisterResultCode>(logicResult);
            }
            catch (Exception ex)
            {
                var fault = new ServiceFault
                {
                    Code = ServiceErrorCode.UnexpectedError,
                    Message = ex.Message
                };

                throw new FaultException<ServiceFault>(fault, new FaultReason(ex.Message));
            }
        }
    }
}