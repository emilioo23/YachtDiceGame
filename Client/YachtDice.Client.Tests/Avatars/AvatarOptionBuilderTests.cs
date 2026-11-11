using System.Collections.Generic;
using System.Linq;
using Xunit;
using YachtDice.Client.Logic.Avatars;

namespace YachtDice.Client.Tests.Avatars
{
    /// <summary>
    /// Rules to mark which avatars a player can choose in the profile window.
    /// </summary>
    public class AvatarOptionBuilderTests
    {
        private const int FreeLevel = 1;
        private const int ThirdLevel = 3;
        private const int FifthLevel = 5;
        private const int FirstAvatarId = 1;
        private const int SecondAvatarId = 2;
        private const int ThirdAvatarId = 3;
        private const int UnknownAvatarId = 99;

        private static readonly bool[] OnlyFirstUnlocked = { true, false, false };
        private static readonly bool[] FirstTwoUnlocked = { true, true, false };

        [Fact]
        public void Build_PlayerBelowTheRequiredLevel_LocksTheAvatar()
        {
            List<AvatarOption> options = AvatarOptionBuilder.Build(CreateCatalog(), FreeLevel, FirstAvatarId);

            Assert.Equal(OnlyFirstUnlocked, options.Select(option => option.IsUnlocked).ToArray());
        }

        [Fact]
        public void Build_PlayerExactlyAtTheRequiredLevel_UnlocksTheAvatar()
        {
            List<AvatarOption> options = AvatarOptionBuilder.Build(CreateCatalog(), ThirdLevel, FirstAvatarId);

            Assert.Equal(FirstTwoUnlocked, options.Select(option => option.IsUnlocked).ToArray());
        }

        [Fact]
        public void Build_PlayerAboveEveryLevel_UnlocksEveryAvatar()
        {
            List<AvatarOption> options = AvatarOptionBuilder.Build(CreateCatalog(), FifthLevel + 1, FirstAvatarId);

            Assert.True(options.All(option => option.IsUnlocked));
        }

        [Fact]
        public void Build_SelectedAvatar_MarksOnlyThatAvatarAsSelected()
        {
            List<AvatarOption> options = AvatarOptionBuilder.Build(CreateCatalog(), FifthLevel, SecondAvatarId);

            Assert.Equal(SecondAvatarId, options.Single(option => option.IsSelected).Id);
        }

        [Fact]
        public void Build_SelectedAvatarNotInTheCatalog_MarksNoneAsSelected()
        {
            List<AvatarOption> options = AvatarOptionBuilder.Build(CreateCatalog(), FifthLevel, UnknownAvatarId);

            Assert.DoesNotContain(options, option => option.IsSelected);
        }

        [Fact]
        public void Build_CatalogOutOfOrder_ReturnsTheOptionsOrderedByIdentifier()
        {
            List<AvatarRequirement> catalog = CreateCatalog().OrderByDescending(avatar => avatar.Id).ToList();

            List<AvatarOption> options = AvatarOptionBuilder.Build(catalog, FifthLevel, FirstAvatarId);

            var expectedOrder = new[] { FirstAvatarId, SecondAvatarId, ThirdAvatarId };
            Assert.Equal(expectedOrder, options.Select(option => option.Id));
        }

        [Fact]
        public void Build_EmptyCatalog_ReturnsAnEmptyList()
        {
            var emptyCatalog = new List<AvatarRequirement>();

            List<AvatarOption> options = AvatarOptionBuilder.Build(emptyCatalog, FifthLevel, FirstAvatarId);

            Assert.Empty(options);
        }

        private static List<AvatarRequirement> CreateCatalog()
        {
            return new List<AvatarRequirement>
            {
                new AvatarRequirement { Id = FirstAvatarId, RequiredLevel = FreeLevel },
                new AvatarRequirement { Id = SecondAvatarId, RequiredLevel = ThirdLevel },
                new AvatarRequirement { Id = ThirdAvatarId, RequiredLevel = FifthLevel }
            };
        }
    }
}
