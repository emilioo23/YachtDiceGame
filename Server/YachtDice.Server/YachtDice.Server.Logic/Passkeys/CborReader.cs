using System;
using System.Collections.Generic;
using System.Text;

namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Reads the small part of CBOR that WebAuthn uses: integers, byte strings, texts, arrays, maps and the simple
    /// values true, false and null. The data comes from the client, so every length is checked before it is used.
    /// </summary>
    public sealed class CborReader
    {
        private const int RootDepth = 0;
        private const int MaxDepth = 6;
        private const int MaxItems = 64;
        private const int MajorShift = 5;
        private const int AdditionalMask = 0x1F;
        private const int OneByteCount = 1;
        private const int TwoByteCount = 2;
        private const int FourByteCount = 4;
        private const int OneByteLength = 24;
        private const int TwoBytesLength = 25;
        private const int FourBytesLength = 26;
        private const int EightBytesLength = 27;
        private const int MajorUnsigned = 0;
        private const int MajorNegative = 1;
        private const long NegativeBase = -1;
        private const int MajorBytes = 2;
        private const int MajorText = 3;
        private const int MajorArray = 4;
        private const int MajorMap = 5;
        private const int MajorSimple = 7;
        private const int SimpleFalse = 20;
        private const int SimpleTrue = 21;
        private const int SimpleNull = 22;
        private const int BitsPerByte = 8;
        private const string InvalidDataMessage = "The CBOR data is not valid.";
        private const string NullDataMessage = "The CBOR data cannot be null.";

        private readonly byte[] _data;
        private int _position;

        /// <summary>
        /// Initializes a reader that starts at the first byte of the data.
        /// </summary>
        /// <param name="data">Bytes to read.</param>
        /// <exception cref="ArgumentNullException">The data is null.</exception>
        public CborReader(byte[] data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data), NullDataMessage);
        }

        /// <summary>
        /// Number of bytes read so far.
        /// </summary>
        public int Position => _position;

        /// <summary>
        /// Reads the next item. Integers are long, byte strings are byte[], texts are string, arrays are
        /// List of object, maps are Dictionary of object and object, and the simple values are bool or null.
        /// </summary>
        /// <returns>The item that was read.</returns>
        /// <exception cref="FormatException">The data ended early or uses something that is not supported.</exception>
        public object ReadItem()
        {
            return ReadItem(RootDepth);
        }

        private object ReadItem(int depth)
        {
            if (depth > MaxDepth)
            {
                throw new FormatException(InvalidDataMessage);
            }

            int initial = ReadByte();

            return ReadMajorType(initial >> MajorShift, initial & AdditionalMask, depth);
        }

        private object ReadMajorType(int major, int additional, int depth)
        {
            object item;

            switch (major)
            {
                case MajorUnsigned:
                    item = ReadArgument(additional);
                    break;
                case MajorNegative:
                    item = NegativeBase - ReadArgument(additional);
                    break;
                case MajorBytes:
                    item = ReadBytes(ReadLength(additional));
                    break;
                case MajorText:
                    item = Encoding.UTF8.GetString(ReadBytes(ReadLength(additional)));
                    break;
                case MajorArray:
                    item = ReadArray(ReadLength(additional), depth);
                    break;
                case MajorMap:
                    item = ReadMap(ReadLength(additional), depth);
                    break;
                case MajorSimple:
                    item = ReadSimple(additional);
                    break;
                default:
                    throw new FormatException(InvalidDataMessage);
            }

            return item;
        }

        private int ReadByte()
        {
            if (_position >= _data.Length)
            {
                throw new FormatException(InvalidDataMessage);
            }

            int value = _data[_position];

            _position++;

            return value;
        }

        private byte[] ReadBytes(int length)
        {
            if (length > _data.Length - _position)
            {
                throw new FormatException(InvalidDataMessage);
            }

            var bytes = new byte[length];

            Array.Copy(_data, _position, bytes, 0, length);
            _position += length;

            return bytes;
        }

        private int ReadLength(int additional)
        {
            long length = ReadArgument(additional);

            if (length > _data.Length)
            {
                throw new FormatException(InvalidDataMessage);
            }

            return (int)length;
        }

        private long ReadArgument(int additional)
        {
            long argument;

            if (additional < OneByteLength)
            {
                argument = additional;
            }
            else if (additional == OneByteLength)
            {
                argument = ReadNumber(OneByteCount);
            }
            else if (additional == TwoBytesLength)
            {
                argument = ReadNumber(TwoByteCount);
            }
            else if (additional == FourBytesLength)
            {
                argument = ReadNumber(FourByteCount);
            }
            else if (additional == EightBytesLength)
            {
                argument = ReadNumber(BitsPerByte);
            }
            else
            {
                throw new FormatException(InvalidDataMessage);
            }

            return argument;
        }

        private long ReadNumber(int byteCount)
        {
            ulong number = 0;

            for (int index = 0; index < byteCount; index++)
            {
                number = (number << BitsPerByte) | (uint)ReadByte();
            }

            if (number > long.MaxValue)
            {
                throw new FormatException(InvalidDataMessage);
            }

            return (long)number;
        }

        private List<object> ReadArray(int count, int depth)
        {
            if (count > MaxItems)
            {
                throw new FormatException(InvalidDataMessage);
            }

            var items = new List<object>();

            for (int index = 0; index < count; index++)
            {
                items.Add(ReadItem(depth + 1));
            }

            return items;
        }

        private Dictionary<object, object> ReadMap(int count, int depth)
        {
            if (count > MaxItems)
            {
                throw new FormatException(InvalidDataMessage);
            }

            var map = new Dictionary<object, object>();

            int index = 0;
            bool hasInvalidKey = false;

            while (index < count && !hasInvalidKey)
            {
                object key = ReadItem(depth + 1);

                hasInvalidKey = key == null;

                if (!hasInvalidKey)
                {
                    map[key] = ReadItem(depth + 1);
                }

                index++;
            }

            if (hasInvalidKey)
            {
                throw new FormatException(InvalidDataMessage);
            }

            return map;
        }

        private static object ReadSimple(int additional)
        {
            object value;

            switch (additional)
            {
                case SimpleFalse:
                    value = false;
                    break;
                case SimpleTrue:
                    value = true;
                    break;
                case SimpleNull:
                    value = null;
                    break;
                default:
                    throw new FormatException(InvalidDataMessage);
            }

            return value;
        }
    }
}
