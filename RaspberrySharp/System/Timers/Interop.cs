using System;
using System.Runtime.InteropServices;

namespace RaspberrySharp.System.Timers
{
    internal static class Interop
    {
        #region Constants

        public static int CLOCK_MONOTONIC_RAW = 4;

        #endregion

        #region Classes

        public struct Timespec
        {
            public IntPtr tv_sec; /* seconds */
            public IntPtr tv_nsec; /* nanoseconds */
        }

        #endregion

        #region Methods

        [DllImport("libc.so.6")]
        public static extern int nanosleep(ref Timespec req, ref Timespec rem);

        #endregion
    }
}
