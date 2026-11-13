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

        [Fact]
        public void Build_PlayerBelowTheRequiredLevel_LocksTheAvatar()
        {
            AvatarOption[] expected =
            {
                CreateOption(FirstAvatarId, FreeLevel, true, true),
                CreateOption(SecondAvatarId, ThirdLevel, false, false),
                CreateOption(ThirdAvatarId, FifthLevel, false, false)
            };

            List<AvatarOption> options = AvatarOptionBuilder.Build(CreateCatalog(), FreeLevel, FirstAvatarId);

            Assert.Equivalent(expected, options, strict: true);
        }

        [Fact]
        public void Build_PlayerExactlyAtTheRequiredLevel_UnlocksTheAvatar()
        {
            AvatarOption[] expected =
            {
                CreateOption(FirstAvatarId, FreeLevel, true, true),
                CreateOption(SecondAvatarId, ThirdLevel, true, false),
                CreateOption(ThirdAvatarId, FifthLevel, false, false)
            };

            List<AvatarOption> options = AvatarOptionBuilder.Build(CreateCatalog(), ThirdLevel, FirstAvatarId);

            Assert.Equivalent(expected, options, strict: true);
        }

        [Fact]
        public void Build_PlayerAboveEveryLevel_UnlocksEveryAvatar()
        {
            AvatarOption[] expected =
            {
                CreateOption(FirstAvatarId, FreeLevel, true, true),
                CreateOption(SecondAvatarId, ThirdLevel, true, false),
                CreateOption(ThirdAvatarId, FifthLevel, true, false)
            };

            List<AvatarOption> options = AvatarOptionBuilder.Build(CreateCatalog(), FifthLevel + 1, FirstAvatarId);

            Assert.Equivalent(expected, options, strict: true);
        }

        [Fact]
        public void Build_SelectedAvatar_MarksOnlyThatAvatarAsSelected()
        {
            AvatarOption[] expected =
            {
                CreateOption(FirstAvatarId, FreeLevel, true, false),
                CreateOption(SecondAvatarId, ThirdLevel, true, true),
                CreateOption(ThirdAvatarId, FifthLevel, true, false)
            };

            List<AvatarOption> options = AvatarOptionBuilder.Build(CreateCatalog(), FifthLevel, SecondAvatarId);

            Assert.Equivalent(expected, options, strict: true);
        }

        [Fact]
        public void Build_SelectedAvatarNotInTheCatalog_MarksNoneAsSelected()
        {
            AvatarOption[] expected =
            {
                CreateOption(FirstAvatarId, FreeLevel, true, false),
                CreateOption(SecondAvatarId, ThirdLevel, true, false),
                CreateOption(ThirdAvatarId, FifthLevel, true, false)
            };

            List<AvatarOption> options = AvatarOptionBuilder.Build(CreateCatalog(), FifthLevel, UnknownAvatarId);

            Assert.Equivalent(expected, options, strict: true);
        }

        [Fact]
        public void Build_CatalogOutOfOrder_ReturnsTheOptionsOrderedByIdentifier()
        {
            List<AvatarRequirement> catalog = CreateCatalog().OrderByDescending(avatar => avatar.Id).ToList();
            AvatarOption[] expected =
            {
                CreateOption(FirstAvatarId, FreeLevel, true, true),
                CreateOption(SecondAvatarId, ThirdLevel, true, false),
                CreateOption(ThirdAvatarId, FifthLevel, true, false)
            };

            List<AvatarOption> options = AvatarOptionBuilder.Build(catalog, FifthLevel, FirstAvatarId);

            Assert.Equivalent(expected, options, strict: true);
        }

        [Fact]
        public void Build_EmptyCatalog_ReturnsAnEmptyList()
        {
            var emptyCatalog = new List<AvatarRequirement>();

            List<AvatarOption> options = AvatarOptionBuilder.Build(emptyCatalog, FifthLevel, FirstAvatarId);

            Assert.Empty(options);
        }

        private static AvatarOption CreateOption(int id, int requiredLevel, bool isUnlocked, bool isSelected)
        {
            var option = new AvatarOption
            {
                Id = id,
                RequiredLevel = requiredLevel,
                IsUnlocked = isUnlocked,
                IsSelected = isSelected
            };
            return option;
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
