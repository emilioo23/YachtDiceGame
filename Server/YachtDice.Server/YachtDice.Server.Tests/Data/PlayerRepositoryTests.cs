using System;
using System.Linq;
using Xunit;
using YachtDice.Server.Data.Configuration;
using YachtDice.Server.Data.Exceptions;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Tests.Data
{
    /// <summary>
    /// Behavior of the data layer when the server runs without its configuration. The tests clear the database
    /// variables of the process, so every repository must fail with the exception that the services turn into the
    /// database fault.
    /// </summary>
    public class PlayerRepositoryTests : IDisposable
    {
        private const string Username = "emilioalvarez";
        private const int ProgressExperience = 350;
        private const int ProgressLevel = 2;
        private const int PlayerId = 1;
        private const int AvatarId = 1;
        private const string Email = "emilioalvarez@gmail.com";

        private static readonly string[] DatabaseVariables =
        {
            EnvironmentSettings.DatabaseServer,
            EnvironmentSettings.DatabaseUser,
            EnvironmentSettings.DatabasePassword
        };

        private readonly PlayerRepository _repository = new PlayerRepository();
        private readonly string[] _savedValues;

        public PlayerRepositoryTests()
        {
            _savedValues = DatabaseVariables.Select(Environment.GetEnvironmentVariable).ToArray();

            foreach (string variable in DatabaseVariables)
            {
                Environment.SetEnvironmentVariable(variable, null);
            }
        }

        public void Dispose()
        {
            for (int index = 0; index < DatabaseVariables.Length; index++)
            {
                Environment.SetEnvironmentVariable(DatabaseVariables[index], _savedValues[index]);
            }
        }

        [Fact]
        public void FindByUsername_WithoutConnectionString_ThrowsDataAccessException()
        {
            Assert.Throws<DataAccessException>(() => _repository.FindByUsername(Username));
        }

        [Fact]
        public void SaveAvatar_WithoutConnectionString_ThrowsDataAccessException()
        {
            Assert.Throws<DataAccessException>(() => _repository.SaveAvatar(PlayerId, AvatarId));
        }

        [Fact]
        public void SaveProgress_WithoutConnectionString_ThrowsDataAccessException()
        {
            Assert.Throws<DataAccessException>(
                () => _repository.SaveProgress(PlayerId, ProgressExperience, ProgressLevel));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void FindByEmail_BlankEmail_ThrowsArgumentException(string email)
        {
            Assert.Throws<ArgumentException>(() => _repository.FindByEmail(email));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void FindByName_BlankName_ThrowsArgumentException(string name)
        {
            Assert.Throws<ArgumentException>(() => _repository.FindByName(name));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void FindByUsername_BlankUsername_ThrowsArgumentException(string username)
        {
            Assert.Throws<ArgumentException>(() => _repository.FindByUsername(username));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void EmailExists_BlankEmail_ThrowsArgumentException(string email)
        {
            Assert.Throws<ArgumentException>(() => _repository.EmailExists(email));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void UsernameExists_BlankUsername_ThrowsArgumentException(string username)
        {
            Assert.Throws<ArgumentException>(() => _repository.UsernameExists(username));
        }

        [Fact]
        public void Add_NullPlayer_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _repository.Add(null));
        }

        [Fact]
        public void FindByEmail_WithoutConnectionString_ThrowsDataAccessException()
        {
            Assert.Throws<DataAccessException>(() => _repository.FindByEmail(Email));
        }

        [Fact]
        public void EmailExists_WithoutConnectionString_ThrowsDataAccessException()
        {
            Assert.Throws<DataAccessException>(() => _repository.EmailExists(Email));
        }

        [Fact]
        public void HasTwoFactorEnabled_WithoutConnectionString_ThrowsDataAccessException()
        {
            Assert.Throws<DataAccessException>(() => _repository.HasTwoFactorEnabled(PlayerId));
        }

        [Fact]
        public void SaveTwoFactorEnabled_WithoutConnectionString_ThrowsDataAccessException()
        {
            Assert.Throws<DataAccessException>(() => _repository.SaveTwoFactorEnabled(PlayerId, true));
        }
    }
}
