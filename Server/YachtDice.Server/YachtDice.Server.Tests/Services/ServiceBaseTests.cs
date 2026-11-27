using System;
using System.ServiceModel;
using Xunit;
using YachtDice.Server.Contracts;
using YachtDice.Server.Data.Exceptions;
using YachtDice.Server.Logic.Security;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Services
{
    /// <summary>
    /// The common behavior of the services: the session decides who calls, a guest cannot use the name of an
    /// account, and every failure reaches the client as a fault with a code instead of an exception.
    /// </summary>
    public class ServiceBaseTests
    {
        private const string PlayerName = "gabrielhernandez";
        private const string AccountName = "emilioo23";
        private const string GuestName = "Invitado";
        private const string UnknownToken = "token-inventado";
        private const string NoToken = "";
        private const string OperationResult = "done";
        private const string FailureMessage = "The operation failed.";
        private const int AccountId = 7;

        private readonly SessionRegistry _sessions = new SessionRegistry();
        private readonly InMemoryFriendshipService _playerIds = new InMemoryFriendshipService();
        private readonly InMemoryService _service;

        /// <summary>
        /// Prepares a service whose sessions are kept in memory and with one registered account.
        /// </summary>
        public ServiceBaseTests()
        {
            _playerIds.AddAccount(AccountName, AccountId);
            _service = new InMemoryService { Sessions = _sessions, PlayerIds = _playerIds };
        }

        [Fact]
        public void FindCaller_TokenOfAStartedSession_ReturnsThePlayerOfTheSession()
        {
            string token = _sessions.Start(PlayerName);

            string caller = _service.FindCaller(token, AccountName);

            Assert.Equal(PlayerName, caller);
        }

        [Fact]
        public void FindCaller_GuestWithAFreeName_ReturnsTheGuestName()
        {
            string caller = _service.FindCaller(NoToken, GuestName);

            Assert.Equal(GuestName, caller);
        }

        [Fact]
        public void FindCaller_GuestWithTheNameOfAnAccount_SendsAValidationFault()
        {
            ServiceErrorCode codeExpected = ServiceErrorCode.ValidationFailed;

            ServiceErrorCode codeResult = ReadFaultCode(() => _service.FindCaller(NoToken, AccountName));

            Assert.Equal(codeExpected, codeResult);
        }

        [Fact]
        public void FindCaller_GuestWithTheNameOfAnAccountInOtherCase_SendsAValidationFault()
        {
            ServiceErrorCode codeExpected = ServiceErrorCode.ValidationFailed;

            ServiceErrorCode codeResult =
                ReadFaultCode(() => _service.FindCaller(NoToken, AccountName.ToUpperInvariant()));

            Assert.Equal(codeExpected, codeResult);
        }

        [Fact]
        public void FindCaller_UnknownToken_SendsASessionExpiredFault()
        {
            ServiceErrorCode codeExpected = ServiceErrorCode.SessionExpired;

            ServiceErrorCode codeResult = ReadFaultCode(() => _service.FindCaller(UnknownToken, GuestName));

            Assert.Equal(codeExpected, codeResult);
        }

        [Fact]
        public void FindCaller_TokenOfASessionStartedAgainElsewhere_SendsASessionReplacedFault()
        {
            ServiceErrorCode codeExpected = ServiceErrorCode.SessionReplaced;
            string oldToken = _sessions.Start(PlayerName);
            _sessions.Start(PlayerName);

            ServiceErrorCode codeResult = ReadFaultCode(() => _service.FindCaller(oldToken, GuestName));

            Assert.Equal(codeExpected, codeResult);
        }

        [Fact]
        public void FindCaller_TokenOfAnEndedSession_SendsASessionExpiredFault()
        {
            ServiceErrorCode codeExpected = ServiceErrorCode.SessionExpired;
            string token = _sessions.Start(PlayerName);
            _sessions.EndFor(PlayerName);

            ServiceErrorCode codeResult = ReadFaultCode(() => _service.FindCaller(token, GuestName));

            Assert.Equal(codeExpected, codeResult);
        }

        [Fact]
        public void FindCaller_NullGuestName_SendsAValidationFault()
        {
            ServiceErrorCode codeExpected = ServiceErrorCode.ValidationFailed;

            ServiceErrorCode codeResult = ReadFaultCode(() => _service.FindCaller(NoToken, null));

            Assert.Equal(codeExpected, codeResult);
        }

        [Fact]
        public void FindCaller_GuestWhileTheDatabaseIsDown_SendsADatabaseFault()
        {
            ServiceErrorCode codeExpected = ServiceErrorCode.DatabaseUnavailable;
            _service.PlayerIds = new InMemoryFailingPlayerIdFinder();

            ServiceErrorCode codeResult = ReadFaultCode(() => _service.FindCaller(NoToken, GuestName));

            Assert.Equal(codeExpected, codeResult);
        }

        [Fact]
        public void RunForPlayer_TokenOfAStartedSession_GivesThePlayerToTheOperation()
        {
            string token = _sessions.Start(PlayerName);

            string receivedName = _service.RunForPlayer(token, playerName => playerName);

            Assert.Equal(PlayerName, receivedName);
        }

        [Fact]
        public void RunForPlayer_TokenOfARenamedPlayer_GivesTheNewNameToTheOperation()
        {
            string token = _sessions.Start(PlayerName);
            _sessions.Rename(PlayerName, AccountName);

            string receivedName = _service.RunForPlayer(token, playerName => playerName);

            Assert.Equal(AccountName, receivedName);
        }

        [Fact]
        public void RunForPlayer_UnknownToken_SendsASessionExpiredFault()
        {
            ServiceErrorCode codeExpected = ServiceErrorCode.SessionExpired;

            ServiceErrorCode codeResult =
                ReadFaultCode(() => _service.RunForPlayer(UnknownToken, playerName => playerName));

            Assert.Equal(codeExpected, codeResult);
        }

        [Fact]
        public void RunForPlayer_NullToken_SendsASessionExpiredFault()
        {
            ServiceErrorCode codeExpected = ServiceErrorCode.SessionExpired;

            ServiceErrorCode codeResult = ReadFaultCode(() => _service.RunForPlayer(null, playerName => playerName));

            Assert.Equal(codeExpected, codeResult);
        }

        [Fact]
        public void RunForPlayer_OperationRejectsTheData_SendsAValidationFault()
        {
            ServiceErrorCode codeExpected = ServiceErrorCode.ValidationFailed;
            string token = _sessions.Start(PlayerName);

            ServiceErrorCode codeResult = ReadFaultCode(() => _service.RunForPlayer(token, RejectData));

            Assert.Equal(codeExpected, codeResult);
        }

        [Fact]
        public void RunWithData_DataPresent_ReturnsTheResultOfTheOperation()
        {
            string result = _service.RunWithData(PlayerName, () => OperationResult);

            Assert.Equal(OperationResult, result);
        }

        [Fact]
        public void RunWithData_NullData_SendsAValidationFault()
        {
            ServiceErrorCode codeExpected = ServiceErrorCode.ValidationFailed;

            ServiceErrorCode codeResult = ReadFaultCode(() => _service.RunWithData(null, () => OperationResult));

            Assert.Equal(codeExpected, codeResult);
        }

        [Fact]
        public void RunWithData_DatabaseFails_SendsADatabaseFault()
        {
            ServiceErrorCode codeExpected = ServiceErrorCode.DatabaseUnavailable;

            ServiceErrorCode codeResult = ReadFaultCode(() => _service.RunWithData(PlayerName, FailDatabase));

            Assert.Equal(codeExpected, codeResult);
        }

        [Fact]
        public void RunWithData_UnexpectedFailure_SendsAnUnexpectedFault()
        {
            ServiceErrorCode codeExpected = ServiceErrorCode.UnexpectedError;

            ServiceErrorCode codeResult = ReadFaultCode(() => _service.RunWithData(PlayerName, FailUnexpectedly));

            Assert.Equal(codeExpected, codeResult);
        }

        [Fact]
        public void RunWithData_UnexpectedFailure_HidesTheMessageOfTheException()
        {
            var fault = (FaultException<ServiceFault>)Record.Exception(
                () => _service.RunWithData(PlayerName, FailUnexpectedly));

            Assert.DoesNotContain(FailureMessage, fault.Detail.Message);
        }

        private static ServiceErrorCode ReadFaultCode(Func<string> call)
        {
            var fault = (FaultException<ServiceFault>)Record.Exception(() => call());

            return fault.Detail.Code;
        }

        private static string RejectData(string playerName)
        {
            throw new ArgumentException(FailureMessage, nameof(playerName));
        }

        private static string FailDatabase()
        {
            throw new DataAccessException(FailureMessage);
        }

        private static string FailUnexpectedly()
        {
            throw new InvalidOperationException(FailureMessage);
        }
    }
}
