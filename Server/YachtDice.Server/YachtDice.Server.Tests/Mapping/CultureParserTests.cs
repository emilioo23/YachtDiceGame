using System.Globalization;
using Xunit;
using YachtDice.Server.Services.Mapping;

namespace YachtDice.Server.Tests.Mapping
{
    /// <summary>
    /// The language that the client sends, turned into the culture used by the emails of the server.
    /// </summary>
    public class CultureParserTests
    {
        private const string PortugueseCultureName = "pt-BR";
        private const string MalformedCultureName = "idioma_inventado!";
        private const string BlankCultureName = "   ";

        [Fact]
        public void Parse_PortugueseCulture_ReturnsThePortugueseCulture()
        {
            CultureInfo culture = CultureParser.Parse(PortugueseCultureName);

            Assert.Equal(PortugueseCultureName, culture.Name);
        }

        [Fact]
        public void Parse_MalformedCulture_ReturnsTheInvariantCulture()
        {
            CultureInfo culture = CultureParser.Parse(MalformedCultureName);

            Assert.Equal(CultureInfo.InvariantCulture, culture);
        }

        [Fact]
        public void Parse_BlankName_ReturnsTheInvariantCulture()
        {
            CultureInfo culture = CultureParser.Parse(BlankCultureName);

            Assert.Equal(CultureInfo.InvariantCulture, culture);
        }

        [Fact]
        public void Parse_NullName_ReturnsTheInvariantCulture()
        {
            CultureInfo culture = CultureParser.Parse(null);

            Assert.Equal(CultureInfo.InvariantCulture, culture);
        }
    }
}
