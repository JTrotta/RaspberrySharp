using System;

namespace RaspberrySharp.System.Timers
{
    /// <summary>
    /// Provides utilities for <see cref="TimeSpan"/>.
    /// </summary>
    public static class TimeSpanUtility
    {
        /// <summary>
        /// Creates a timespan from a number of microseconds.
        /// </summary>
        /// <param name="microseconds">The microseconds.</param>
        /// <returns></returns>
        public static TimeSpan FromMicroseconds(double microseconds)
        {
            return TimeSpan.FromTicks((long)(microseconds / 10));
        }
    }
}
