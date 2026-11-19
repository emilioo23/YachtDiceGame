using System;

namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Converts an ECDSA signature from the DER format that the authenticators send into the fixed format
    /// (r followed by s) that the .NET classes verify.
    /// </summary>
    public static class EcdsaSignatureConverter
    {
        private const byte SequenceTag = 0x30;
        private const byte IntegerTag = 0x02;
        private const int LongFormFlag = 0x80;
        private const int OneByteLongForm = 0x81;
        private const int PartCount = 2;

        /// <summary>
        /// Converts the signature.
        /// </summary>
        /// <param name="derSignature">Signature in DER format.</param>
        /// <param name="partSize">Size in bytes of r and of s; 32 for the P-256 curve.</param>
        /// <param name="fixedSignature">Signature with r and s one after the other; null if it is not valid.</param>
        /// <returns>True if the signature was converted.</returns>
        public static bool TryConvert(byte[] derSignature, int partSize, out byte[] fixedSignature)
        {
            fixedSignature = null;

            return derSignature != null
                && new DerReader(derSignature).TryReadSignature(partSize, out fixedSignature);
        }

        private sealed class DerReader
        {
            private readonly byte[] _data;
            private int _position;

            public DerReader(byte[] data)
            {
                _data = data;
            }

            public bool IsAtEnd
            {
                get { return _position == _data.Length; }
            }

            public bool TryReadSignature(int partSize, out byte[] signature)
            {
                byte[] r = null;
                byte[] s = null;
                bool isValid = TryReadSequence() && TryReadInteger(partSize, out r)
                    && TryReadInteger(partSize, out s) && IsAtEnd;

                signature = null;

                if (isValid)
                {
                    signature = new byte[partSize * PartCount];
                    Array.Copy(r, 0, signature, 0, partSize);
                    Array.Copy(s, 0, signature, partSize, partSize);
                }

                return isValid;
            }

            private bool TryReadSequence()
            {
                int length = 0;
                bool isValid = TryReadTag(SequenceTag) && TryReadLength(out length);

                if (isValid)
                {
                    isValid = length == _data.Length - _position;
                }

                return isValid;
            }

            private bool TryReadInteger(int partSize, out byte[] value)
            {
                int length = 0;
                bool isValid = TryReadTag(IntegerTag) && TryReadLength(out length)
                    && length > 0 && length <= _data.Length - _position;

                value = null;

                if (isValid)
                {
                    value = Pad(length, partSize);
                    isValid = value != null;
                    _position += length;
                }

                return isValid;
            }

            private bool TryReadTag(byte tag)
            {
                bool isValid = _position < _data.Length && _data[_position] == tag;

                if (isValid)
                {
                    _position++;
                }

                return isValid;
            }

            private bool TryReadLength(out int length)
            {
                bool isValid = _position < _data.Length;

                length = 0;

                if (isValid)
                {
                    int first = _data[_position];

                    _position++;

                    if (first < LongFormFlag)
                    {
                        length = first;
                    }
                    else if (first == OneByteLongForm && _position < _data.Length)
                    {
                        length = _data[_position];
                        _position++;
                    }
                    else
                    {
                        isValid = false;
                    }
                }

                return isValid;
            }

            private byte[] Pad(int length, int partSize)
            {
                int start = _position;
                int end = _position + length;
                byte[] padded = null;

                while (start < end - 1 && _data[start] == 0)
                {
                    start++;
                }

                int significant = end - start;

                if (significant <= partSize)
                {
                    padded = new byte[partSize];
                    Array.Copy(_data, start, padded, partSize - significant, significant);
                }

                return padded;
            }
        }
    }
}
