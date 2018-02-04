using System;

namespace RaspberrySharp.IO
{
    /// <summary>
    /// Provides an interface for input, analog pin.
    /// </summary>
    public interface IInputAnalogPin : IDisposable
    {
        #region Methods

        /// <summary>
        /// Reads the value of the pin.
        /// </summary>
        /// <returns>The value.</returns>
        AnalogValue Read();

        #endregion
    }
}
