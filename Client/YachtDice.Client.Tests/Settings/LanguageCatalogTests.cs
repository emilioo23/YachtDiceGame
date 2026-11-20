using Xunit;
using YachtDice.Client.Logic.Settings;

namespace YachtDice.Client.Tests.Settings
{
    /// <summary>
    /// The languages of the interface with their flag, short name and position in the lists.
    /// </summary>
    public class LanguageCatalogTests
    {
        private const string SpanishCulture = "es-MX";
        private const string EnglishCulture = "en-US";
        private const string PortugueseCulture = "pt-BR";
        private const string ChineseCulture = "zh-CN";
        private const string UnknownCulture = "fr-FR";
        private const int EnglishPosition = 1;
        private const int SpanishPosition = 0;
        private const string BrazilFlag = "br";
        private const string ChinaFlag = "cn";
        private const string SpanishFlag = "es";
        private const string PortugueseShortName = "PT";
        private const string ChineseShortName = "ZH";

        [Fact]
        public void IsSupported_ChineseCulture_ReturnsTrue()
        {
            bool isSupported = LanguageCatalog.IsSupported(ChineseCulture);

            Assert.True(isSupported);
        }

        [Fact]
        public void IsSupported_UnknownCulture_ReturnsFalse()
        {
            bool isSupported = LanguageCatalog.IsSupported(UnknownCulture);

            Assert.False(isSupported);
        }

        [Fact]
        public void CultureCodes_Always_ListsTheFourLanguagesInOrder()
        {
            var culturesExpected = new[] { SpanishCulture, EnglishCulture, PortugueseCulture, ChineseCulture };

            Assert.Equal(culturesExpected, LanguageCatalog.CultureCodes);
        }

        [Fact]
        public void GetFlagCode_PortugueseCulture_ReturnsTheFlagOfBrazil()
        {
            string flag = LanguageCatalog.GetFlagCode(PortugueseCulture);

            Assert.Equal(BrazilFlag, flag);
        }

        [Fact]
        public void GetFlagCode_ChineseCulture_ReturnsTheFlagOfChina()
        {
            string flag = LanguageCatalog.GetFlagCode(ChineseCulture);

            Assert.Equal(ChinaFlag, flag);
        }

        [Fact]
        public void GetFlagCode_UnknownCulture_ReturnsTheSpanishFlag()
        {
            string flag = LanguageCatalog.GetFlagCode(UnknownCulture);

            Assert.Equal(SpanishFlag, flag);
        }

        [Fact]
        public void GetShortName_PortugueseCulture_ReturnsPt()
        {
            string name = LanguageCatalog.GetShortName(PortugueseCulture);

            Assert.Equal(PortugueseShortName, name);
        }

        [Fact]
        public void GetShortName_ChineseCulture_ReturnsZh()
        {
            string name = LanguageCatalog.GetShortName(ChineseCulture);

            Assert.Equal(ChineseShortName, name);
        }

        [Fact]
        public void IndexOf_EnglishCulture_ReturnsTheSecondPosition()
        {
            int index = LanguageCatalog.IndexOf(EnglishCulture);

            Assert.Equal(EnglishPosition, index);
        }

        [Fact]
        public void IndexOf_UnknownCulture_ReturnsTheSpanishPosition()
        {
            int index = LanguageCatalog.IndexOf(UnknownCulture);

            Assert.Equal(SpanishPosition, index);
        }
    }
}
