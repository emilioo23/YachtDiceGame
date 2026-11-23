using System;
using Xunit;
using YachtDice.Client.Logic.Authentication;

namespace YachtDice.Client.Tests.Authentication
{
    /// <summary>
    /// Checks and texts of the data of the account.
    /// </summary>
    public class AccountInputRulesTests
    {
        private const string ValidCode = "12345678";
        private const string ShortCode = "123456";
        private const string ResetShortCode = "123";
        private const string CodeWithLetters = "1234abcd";
        private const string CurrentPassword = "Old123!";
        private const string NewPassword = "New456!";
        private const string OtherPassword = "Other789!";
        private const string Spaces = "      ";
        private const string LongEmail = "gabriel@gmail.com";
        private const string MaskedLongEmail = "ga*****@gmail.com";
        private const string ShortEmail = "ab@gmail.com";
        private const string ThreeCharactersEmail = "abc@gmail.com";
        private const string MaskedThreeCharactersEmail = "ab*@gmail.com";
        private const string OneCharacterEmail = "a@gmail.com";
        private const string TextWithoutAtSign = "gabriel.gmail.com";
        private const string AtSignFirstText = "@gmail.com";
        private const string TwoAtSignsText = "gabriel@gm@il.com";
        private const string MaskedTwoAtSignsText = "ga*****@gm@il.com";
        private const string Code = "123456";
        private const string Password = "New456!";
        private const string SixSpaces = "      ";

        [Fact]
        public void IsValid_EightDigits_ReturnsTrue()
        {
            bool isValid = AccountInputRules.IsValid(ValidCode);

            Assert.True(isValid);
        }

        [Fact]
        public void IsValid_FewerDigits_ReturnsFalse()
        {
            bool isValid = AccountInputRules.IsValid(ShortCode);

            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_Letters_ReturnsFalse()
        {
            bool isValid = AccountInputRules.IsValid(CodeWithLetters);

            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_Null_ReturnsFalse()
        {
            bool isValid = AccountInputRules.IsValid(null);

            Assert.False(isValid);
        }

        [Fact]
        public void Validate_ThreePasswordsWithMatchingConfirmation_ReturnsValid()
        {
            PasswordChangeStatus status =
                AccountInputRules.ValidatePasswordChange(CurrentPassword, NewPassword, NewPassword);

            Assert.Equal(PasswordChangeStatus.Valid, status);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyCurrentPassword_ReturnsEmptyCurrentPassword(string currentPassword)
        {
            PasswordChangeStatus status =
                AccountInputRules.ValidatePasswordChange(currentPassword, NewPassword, NewPassword);

            Assert.Equal(PasswordChangeStatus.EmptyCurrentPassword, status);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("      ")]
        public void Validate_EmptyNewPassword_ReturnsEmptyNewPassword(string newPassword)
        {
            PasswordChangeStatus status =
                AccountInputRules.ValidatePasswordChange(CurrentPassword, newPassword, newPassword);

            Assert.Equal(PasswordChangeStatus.EmptyNewPassword, status);
        }

        [Fact]
        public void Validate_ConfirmationDifferentFromTheNewPassword_ReturnsPasswordsDoNotMatch()
        {
            PasswordChangeStatus status =
                AccountInputRules.ValidatePasswordChange(CurrentPassword, NewPassword, OtherPassword);

            Assert.Equal(PasswordChangeStatus.PasswordsDoNotMatch, status);
        }

        [Fact]
        public void Validate_CurrentPasswordOfOnlySpaces_ReturnsValid()
        {
            PasswordChangeStatus status =
                AccountInputRules.ValidatePasswordChange(Spaces, NewPassword, NewPassword);

            Assert.Equal(PasswordChangeStatus.Valid, status);
        }

        [Fact]
        public void Mask_LongLocalPart_KeepsTheFirstTwoCharactersAndTheDomain()
        {
            string masked = AccountInputRules.Mask(LongEmail);

            Assert.Equal(MaskedLongEmail, masked);
        }

        [Fact]
        public void Mask_LocalPartOfTwoCharacters_ReturnsTheSameAddress()
        {
            string masked = AccountInputRules.Mask(ShortEmail);

            Assert.Equal(ShortEmail, masked);
        }

        [Fact]
        public void Mask_LocalPartOfThreeCharacters_HidesOnlyTheLastOne()
        {
            string masked = AccountInputRules.Mask(ThreeCharactersEmail);

            Assert.Equal(MaskedThreeCharactersEmail, masked);
        }

        [Fact]
        public void Mask_LocalPartOfOneCharacter_ReturnsTheSameAddress()
        {
            string masked = AccountInputRules.Mask(OneCharacterEmail);

            Assert.Equal(OneCharacterEmail, masked);
        }

        [Fact]
        public void Mask_TextWithoutAtSign_ReturnsTheSameText()
        {
            string masked = AccountInputRules.Mask(TextWithoutAtSign);

            Assert.Equal(TextWithoutAtSign, masked);
        }

        [Fact]
        public void Mask_EmptyText_ReturnsAnEmptyText()
        {
            string masked = AccountInputRules.Mask(string.Empty);

            Assert.Equal(string.Empty, masked);
        }

        [Fact]
        public void Mask_AtSignAtTheStart_ReturnsTheSameText()
        {
            string masked = AccountInputRules.Mask(AtSignFirstText);

            Assert.Equal(AtSignFirstText, masked);
        }

        [Fact]
        public void Mask_TwoAtSigns_MasksUntilTheFirstOne()
        {
            string masked = AccountInputRules.Mask(TwoAtSignsText);

            Assert.Equal(MaskedTwoAtSignsText, masked);
        }

        [Fact]
        public void Validate_CompleteCodeAndMatchingPasswords_ReturnsValid()
        {
            ResetPasswordStatus status = ValidateInput(Code, Password, Password);

            Assert.Equal(ResetPasswordStatus.Valid, status);
        }

        [Theory]
        [InlineData("")]
        [InlineData("12345")]
        [InlineData("1234567")]
        [InlineData("12a456")]
        [InlineData(null)]
        public void Validate_CodeThatIsNotSixDigits_ReturnsIncompleteCode(string code)
        {
            ResetPasswordStatus status = ValidateInput(code, Password, Password);

            Assert.Equal(ResetPasswordStatus.IncompleteCode, status);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Validate_BlankPassword_ReturnsEmptyPassword(string password)
        {
            ResetPasswordStatus status = ValidateInput(Code, password, password);

            Assert.Equal(ResetPasswordStatus.EmptyPassword, status);
        }

        [Fact]
        public void Validate_ConfirmationDifferentFromPassword_ReturnsPasswordsDoNotMatch()
        {
            ResetPasswordStatus status = ValidateInput(Code, Password, OtherPassword);

            Assert.Equal(ResetPasswordStatus.PasswordsDoNotMatch, status);
        }

        [Fact]
        public void Validate_CodeOfSpaces_ReturnsIncompleteCode()
        {
            ResetPasswordStatus status = ValidateInput(SixSpaces, Password, Password);

            Assert.Equal(ResetPasswordStatus.IncompleteCode, status);
        }

        [Fact]
        public void Validate_CodeAndPasswordBothWrong_ReportsTheCodeFirst()
        {
            ResetPasswordStatus status = ValidateInput(ResetShortCode, string.Empty, Password);

            Assert.Equal(ResetPasswordStatus.IncompleteCode, status);
        }

        [Fact]
        public void Validate_BlankPasswordAndDifferentConfirmation_ReportsTheEmptyPassword()
        {
            ResetPasswordStatus status = ValidateInput(Code, string.Empty, Password);

            Assert.Equal(ResetPasswordStatus.EmptyPassword, status);
        }

        [Fact]
        public void Validate_NullConfirmation_ReturnsPasswordsDoNotMatch()
        {
            ResetPasswordStatus status = ValidateInput(Code, Password, null);

            Assert.Equal(ResetPasswordStatus.PasswordsDoNotMatch, status);
        }

        [Fact]
        public void Validate_ConfirmationWithOtherCase_ReturnsPasswordsDoNotMatch()
        {
            ResetPasswordStatus status = ValidateInput(Code, Password, Password.ToUpperInvariant());

            Assert.Equal(ResetPasswordStatus.PasswordsDoNotMatch, status);
        }

        private static ResetPasswordStatus ValidateInput(string code, string newPassword, string confirmation)
        {
            return AccountInputRules.ValidateResetPassword(code, newPassword, confirmation);
        }
    }
}
