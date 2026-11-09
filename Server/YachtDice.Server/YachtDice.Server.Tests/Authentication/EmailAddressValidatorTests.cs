using Xunit;
using YachtDice.Server.Logic.Authentication;

namespace YachtDice.Server.Tests.Authentication
{
    /// <summary>
    /// Rules to decide whether a text is written like an email address.
    /// </summary>
    public class EmailAddressValidatorTests
    {
        [Theory]
        [InlineData("gabriel@example.com")]
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
        [InlineData("@example.com")]
        [InlineData("gabriel example.com")]
        public void IsValid_EmptyOrMalformedText_ReturnsFalse(string email)
        {
            Assert.False(EmailAddressValidator.IsValid(email));
        }

        [Fact]
        public void IsValid_EmailWithSpacesAround_ReturnsFalse()
        {
            Assert.False(EmailAddressValidator.IsValid(" gabriel@example.com "));
        }
    }
}
