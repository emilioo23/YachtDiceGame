using System;
using Xunit;
using YachtDice.Server.Data.Exceptions;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Tests.Data
{
    /// <summary>
    /// Behavior of the data layer when the server runs without its configuration. The test project has no
    /// connection string, so every repository must fail with the exception that the services turn into the
    /// database fault.
    /// </summary>
    public class PlayerRepositoryTests
    {
        private const string Username = "player";
        private const int PlayerId = 1;
        private const int AvatarId = 1;
        private const string Email = "player@gmail.com";

        private readonly PlayerRepository _repository = new PlayerRepository();

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
