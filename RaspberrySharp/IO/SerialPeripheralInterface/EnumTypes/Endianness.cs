using System;
using System.Collections.Generic;
using System.Text;

namespace RaspberrySharp.IO.SerialPeripheralInterface.EnumTypes
{
    /// <summary>
    /// Represents the endianness of a SPI communication.
    /// </summary>
    public enum Endianness
    {
        /// <summary>
        /// The communication is little endian.
        /// </summary>
        LittleEndian,

        /// <summary>
        /// The communication is big endian.
        /// </summary>
        BigEndian
    }
}
