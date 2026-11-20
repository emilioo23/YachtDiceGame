using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace YachtDice.Passkeys
{
    /// <summary>
    /// Keeps the unmanaged memory that a call to Windows needs and frees all of it when the call ends.
    /// </summary>
    internal sealed class NativeScope : IDisposable
    {
        private readonly List<IntPtr> _blocks = new List<IntPtr>();

        /// <summary>
        /// Copies the bytes to unmanaged memory.
        /// </summary>
        /// <param name="data">Bytes to copy.</param>
        /// <returns>Pointer to the copy.</returns>
        public IntPtr Bytes(byte[] data)
        {
            IntPtr block = Marshal.AllocHGlobal(Math.Max(data.Length, 1));

            Marshal.Copy(data, 0, block, data.Length);
            _blocks.Add(block);

            return block;
        }

        /// <summary>
        /// Copies the text to unmanaged memory as UTF-16 with a terminator.
        /// </summary>
        /// <param name="text">Text to copy.</param>
        /// <returns>Pointer to the copy.</returns>
        public IntPtr Text(string text)
        {
            IntPtr block = Marshal.StringToHGlobalUni(text);

            _blocks.Add(block);

            return block;
        }

        /// <summary>
        /// Copies the items to unmanaged memory, one after the other, as the array of structures that Windows reads.
        /// </summary>
        /// <typeparam name="T">Type of the structure.</typeparam>
        /// <param name="items">Items to copy.</param>
        /// <returns>Pointer to the first item.</returns>
        public IntPtr Array<T>(IList<T> items)
            where T : struct
        {
            int itemSize = Marshal.SizeOf(typeof(T));
            IntPtr block = Marshal.AllocHGlobal(Math.Max(itemSize * items.Count, 1));

            _blocks.Add(block);

            for (int index = 0; index < items.Count; index++)
            {
                Marshal.StructureToPtr(items[index], new IntPtr(block.ToInt64() + (index * itemSize)), false);
            }

            return block;
        }

        /// <summary>
        /// Frees all the memory that the scope reserved.
        /// </summary>
        public void Dispose()
        {
            foreach (IntPtr block in _blocks)
            {
                Marshal.FreeHGlobal(block);
            }

            _blocks.Clear();
        }
    }
}
