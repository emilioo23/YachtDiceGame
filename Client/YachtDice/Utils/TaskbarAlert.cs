using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace YachtDice.Utils
{
    /// <summary>
    /// Makes the button of a window blink in the taskbar of Windows until the player opens the window, so a
    /// player who is looking at another program notices that something needs them.
    /// </summary>
    public static class TaskbarAlert
    {
        private const uint FlashAllUntilForeground = 0x0000000F;
        private const uint EndlessFlashes = 0;
        private const uint DefaultBlinkRate = 0;

        /// <summary>
        /// Makes the window blink when it is not the active window. Nothing happens when it is active.
        /// </summary>
        /// <param name="window">Window that needs the attention of the player.</param>
        public static void FlashIfInactive(Window window)
        {
            IntPtr handle = new WindowInteropHelper(window).Handle;

            if (!window.IsActive && handle != IntPtr.Zero)
            {
                var flashSettings = new NativeMethods.FlashSettings
                {
                    Size = (uint)Marshal.SizeOf(typeof(NativeMethods.FlashSettings)),
                    WindowHandle = handle,
                    Flags = FlashAllUntilForeground,
                    Count = EndlessFlashes,
                    Timeout = DefaultBlinkRate
                };

                NativeMethods.FlashWindowEx(ref flashSettings);
            }
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool FlashWindowEx(ref FlashSettings flashSettings);

            [StructLayout(LayoutKind.Sequential)]
            public struct FlashSettings
            {
                public uint Size;
                public IntPtr WindowHandle;
                public uint Flags;
                public uint Count;
                public uint Timeout;
            }
        }
    }
}
