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
    }
}
