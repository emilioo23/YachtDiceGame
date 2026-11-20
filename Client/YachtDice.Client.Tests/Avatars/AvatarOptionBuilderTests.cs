using System.Collections.Generic;
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
            AvatarOption[] optionsExpected =
            {
                CreateOption(CreateRequirement(FirstAvatarId, FreeLevel), true, true),
                CreateOption(CreateRequirement(SecondAvatarId, ThirdLevel), false, false),
                CreateOption(CreateRequirement(ThirdAvatarId, FifthLevel), false, false)
            };

            List<AvatarOption> options = AvatarOptionBuilder.Build(CreateCatalog(), FreeLevel, FirstAvatarId);

            Assert.Equivalent(optionsExpected, options, strict: true);
        }

        [Fact]
        public void Build_PlayerExactlyAtTheRequiredLevel_UnlocksTheAvatar()
        {
            AvatarOption[] optionsExpected =
            {
                CreateOption(CreateRequirement(FirstAvatarId, FreeLevel), true, true),
                CreateOption(CreateRequirement(SecondAvatarId, ThirdLevel), true, false),
                CreateOption(CreateRequirement(ThirdAvatarId, FifthLevel), false, false)
            };

            List<AvatarOption> options = AvatarOptionBuilder.Build(CreateCatalog(), ThirdLevel, FirstAvatarId);

            Assert.Equivalent(optionsExpected, options, strict: true);
        }

        [Fact]
        public void Build_PlayerAboveEveryLevel_UnlocksEveryAvatar()
        {
            AvatarOption[] optionsExpected =
            {
                CreateOption(CreateRequirement(FirstAvatarId, FreeLevel), true, true),
                CreateOption(CreateRequirement(SecondAvatarId, ThirdLevel), true, false),
                CreateOption(CreateRequirement(ThirdAvatarId, FifthLevel), true, false)
            };

            List<AvatarOption> options = AvatarOptionBuilder.Build(CreateCatalog(), FifthLevel + 1, FirstAvatarId);

            Assert.Equivalent(optionsExpected, options, strict: true);
        }

        [Fact]
        public void Build_SelectedAvatar_MarksOnlyThatAvatarAsSelected()
        {
            AvatarOption[] optionsExpected =
            {
                CreateOption(CreateRequirement(FirstAvatarId, FreeLevel), true, false),
                CreateOption(CreateRequirement(SecondAvatarId, ThirdLevel), true, true),
                CreateOption(CreateRequirement(ThirdAvatarId, FifthLevel), true, false)
            };

            List<AvatarOption> options = AvatarOptionBuilder.Build(CreateCatalog(), FifthLevel, SecondAvatarId);

            Assert.Equivalent(optionsExpected, options, strict: true);
        }

        [Fact]
        public void Build_SelectedAvatarNotInTheCatalog_MarksNoneAsSelected()
        {
            AvatarOption[] optionsExpected =
            {
                CreateOption(CreateRequirement(FirstAvatarId, FreeLevel), true, false),
                CreateOption(CreateRequirement(SecondAvatarId, ThirdLevel), true, false),
                CreateOption(CreateRequirement(ThirdAvatarId, FifthLevel), true, false)
            };

            List<AvatarOption> options = AvatarOptionBuilder.Build(CreateCatalog(), FifthLevel, UnknownAvatarId);

            Assert.Equivalent(optionsExpected, options, strict: true);
        }

        [Fact]
        public void Build_CatalogOutOfOrder_ReturnsTheOptionsOrderedByRequiredLevel()
        {
            var catalog = new List<AvatarRequirement>
            {
                CreateRequirement(FirstAvatarId, FifthLevel),
                CreateRequirement(SecondAvatarId, FreeLevel),
                CreateRequirement(ThirdAvatarId, ThirdLevel)
            };
            AvatarOption[] optionsExpected =
            {
                CreateOption(CreateRequirement(SecondAvatarId, FreeLevel), true, false),
                CreateOption(CreateRequirement(ThirdAvatarId, ThirdLevel), true, false),
                CreateOption(CreateRequirement(FirstAvatarId, FifthLevel), true, true)
            };

            List<AvatarOption> options = AvatarOptionBuilder.Build(catalog, FifthLevel, FirstAvatarId);

            Assert.Equivalent(optionsExpected, options, strict: true);
        }

        [Fact]
        public void Build_AvatarsWithTheSameLevel_ReturnsThemOrderedByIdentifier()
        {
            var catalog = new List<AvatarRequirement>
            {
                CreateRequirement(ThirdAvatarId, ThirdLevel),
                CreateRequirement(SecondAvatarId, ThirdLevel)
            };
            AvatarOption[] optionsExpected =
            {
                CreateOption(CreateRequirement(SecondAvatarId, ThirdLevel), true, false),
                CreateOption(CreateRequirement(ThirdAvatarId, ThirdLevel), true, false)
            };

            List<AvatarOption> options = AvatarOptionBuilder.Build(catalog, ThirdLevel, FirstAvatarId);

            Assert.Equivalent(optionsExpected, options, strict: true);
        }

        [Fact]
        public void Build_EmptyCatalog_ReturnsAnEmptyList()
        {
            var emptyCatalog = new List<AvatarRequirement>();

            List<AvatarOption> options = AvatarOptionBuilder.Build(emptyCatalog, FifthLevel, FirstAvatarId);

            Assert.Empty(options);
        }

        private static AvatarRequirement CreateRequirement(int id, int requiredLevel)
        {
            return new AvatarRequirement { Id = id, RequiredLevel = requiredLevel };
        }

        private static AvatarOption CreateOption(AvatarRequirement requirement, bool isUnlocked, bool isSelected)
        {
            var option = new AvatarOption
            {
                Id = requirement.Id,
                RequiredLevel = requirement.RequiredLevel,
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
