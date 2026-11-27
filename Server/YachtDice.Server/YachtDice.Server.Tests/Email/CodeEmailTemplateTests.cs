using System;
using Xunit;
using YachtDice.Server.Logic.Email;

namespace YachtDice.Server.Tests.Email
{
    /// <summary>
    /// The template of an email that carries a code: the keys of its texts and how long the code lasts.
    /// </summary>
    public class CodeEmailTemplateTests
    {
        private const string SubjectKey = "TwoFactor_EmailSubject";
        private const string BodyKey = "TwoFactor_EmailBody";
        private const int ExpirationMinutes = 10;

        [Fact]
        public void Constructor_NullSubjectKey_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CodeEmailTemplate(null, BodyKey, ExpirationMinutes));
        }

        [Fact]
        public void Constructor_NullBodyKey_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CodeEmailTemplate(SubjectKey, null, ExpirationMinutes));
        }

        [Fact]
        public void Constructor_ValidValues_KeepsTheSubjectKey()
        {
            var template = new CodeEmailTemplate(SubjectKey, BodyKey, ExpirationMinutes);

            Assert.Equal(SubjectKey, template.SubjectResourceKey);
        }

        [Fact]
        public void Constructor_ValidValues_KeepsTheBodyKey()
        {
            var template = new CodeEmailTemplate(SubjectKey, BodyKey, ExpirationMinutes);

            Assert.Equal(BodyKey, template.BodyResourceKey);
        }

        [Fact]
        public void Constructor_ValidValues_KeepsTheExpirationMinutes()
        {
            var template = new CodeEmailTemplate(SubjectKey, BodyKey, ExpirationMinutes);

            Assert.Equal(ExpirationMinutes, template.ExpirationMinutes);
        }
    }
}
