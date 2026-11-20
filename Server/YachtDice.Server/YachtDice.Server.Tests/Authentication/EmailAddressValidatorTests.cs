using Xunit;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Tests.Authentication
{
    /// <summary>
    /// Rules to decide whether a text is written like an email address.
    /// </summary>
    public class EmailAddressValidatorTests
    {
        private const string UniversityDomain = "@uv.mx";
        private const string EmailWithSurroundingSpaces = " gabriel@gmail.com ";

        [Theory]
        [InlineData("gabriel@gmail.com")]
        [InlineData("emilio.alvarez@uv.mx")]
        public void IsValid_WellWrittenEmail_ReturnsTrue(string email)
        {
            Assert.True(EmailAddressValidator.IsValid(email));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        [InlineData("gabriel")]
        [InlineData("gabriel@")]
        [InlineData("@gmail.com")]
        [InlineData("gabriel gmail.com")]
        public void IsValid_EmptyOrMalformedText_ReturnsFalse(string email)
        {
            Assert.False(EmailAddressValidator.IsValid(email));
        }

        [Fact]
        public void IsValid_EmailLongerThanTheMaximum_ReturnsFalse()
        {
            string longEmail = new string('a', CredentialRules.EmailMaxLength) + UniversityDomain;

            Assert.False(EmailAddressValidator.IsValid(longEmail));
        }

        [Fact]
        public void IsValid_EmailWithSpacesAround_ReturnsFalse()
        {
            Assert.False(EmailAddressValidator.IsValid(EmailWithSurroundingSpaces));
        }
    }
}
