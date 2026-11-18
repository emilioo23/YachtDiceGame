using System;
using System.Collections.Generic;
using Xunit;
using YachtDice.Server.Logic.Passkeys;

namespace YachtDice.Server.Tests.Passkeys
{
    /// <summary>
    /// Rules of the reader of the CBOR data that the authenticators send.
    /// </summary>
    public class CborReaderTests
    {
        private const long TwoHundredFiftySix = 256;
        private const long MinusOne = -1;
        private const long MinusSeven = -7;
        private const long AlgorithmLabel = 3;
        private const string FormatText = "fmt";
        private static readonly byte[] UnsignedBytes = { 0x19, 0x01, 0x00 };
        private static readonly byte[] NegativeBytes = { 0x26 };
        private static readonly byte[] TextBytes = { 0x63, 0x66, 0x6D, 0x74 };
        private static readonly byte[] MapBytes = { 0xA2, 0x01, 0x02, 0x03, 0x26 };
        private static readonly byte[] TruncatedBytes = { 0x58, 0x20, 0x01 };
        private static readonly byte[] NullKeyBytes = { 0xA1, 0xF6, 0x01 };

        [Fact]
        public void ReadItem_UnsignedInteger_ReturnsTheNumber()
        {
            object itemExpected = TwoHundredFiftySix;

            object itemResult = new CborReader(UnsignedBytes).ReadItem();

            Assert.Equal(itemExpected, itemResult);
        }

        [Fact]
        public void ReadItem_NegativeInteger_ReturnsTheNegativeNumber()
        {
            object itemExpected = MinusSeven;

            object itemResult = new CborReader(NegativeBytes).ReadItem();

            Assert.Equal(itemExpected, itemResult);
        }

        [Fact]
        public void ReadItem_Text_ReturnsTheText()
        {
            object itemExpected = FormatText;

            object itemResult = new CborReader(TextBytes).ReadItem();

            Assert.Equal(itemExpected, itemResult);
        }

        [Fact]
        public void ReadItem_MapWithIntegerKeys_ReturnsTheValueOfTheKey()
        {
            object valueExpected = MinusSeven;
            var map = (Dictionary<object, object>)new CborReader(MapBytes).ReadItem();

            object valueResult = map[AlgorithmLabel];

            Assert.Equal(valueExpected, valueResult);
        }

        [Fact]
        public void ReadItem_ByteStringLongerThanTheData_ThrowsFormatException()
        {
            var reader = new CborReader(TruncatedBytes);

            Assert.Throws<FormatException>(() => reader.ReadItem());
        }

        [Fact]
        public void ReadItem_MapWithNullKey_ThrowsFormatException()
        {
            var reader = new CborReader(NullKeyBytes);

            Assert.Throws<FormatException>(() => reader.ReadItem());
        }

        [Fact]
        public void Constructor_NullData_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CborReader(null));
        }
    }
}
