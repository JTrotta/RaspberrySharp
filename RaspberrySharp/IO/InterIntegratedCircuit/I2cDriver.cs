using RaspberrySharp.IO.GeneralPurpose;
using RaspberrySharp.IO.Interop;
using RaspberrySharp.System;
using RaspberrySharp.System.Timers;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace RaspberrySharp.IO.InterIntegratedCircuit
{
    /// <summary>
    /// Represents a driver for I2C devices.
    /// </summary>
    public class I2cDriver : IDisposable
    {
        #region Fields

        private readonly object driverLock = new object();

        private readonly ProcessorPin sdaPin;
        private readonly ProcessorPin sclPin;
        private readonly bool wasSdaPinSet;
        private readonly bool wasSclPinSet;

        private readonly IntPtr gpioAddress;
        private readonly IntPtr bscAddress;

        private int currentDeviceAddress;
        private int waitInterval;

        #endregion

        #region Instance Management

        /// <summary>
        /// Initializes a new instance of the <see cref="I2cDriver"/> class.
        /// </summary>
        /// <param name="sdaPin">The SDA pin.</param>
        /// <param name="sclPin">The SCL pin.</param>
        public I2cDriver(ProcessorPin sdaPin, ProcessorPin sclPin)
        {
            this.sdaPin = sdaPin;
            this.sclPin = sclPin;

            var bscBase = GetBscBase(sdaPin, sclPin);

            var memoryFile = OP.open("/dev/mem", OP.O_RDWR + OP.O_SYNC);
            try
            {
                gpioAddress = OP.mmap(
                    IntPtr.Zero,
                    OP.BCM2835_BLOCK_SIZE,
                    OP.PROT_READ | OP.PROT_WRITE,
                    OP.MAP_SHARED,
                    memoryFile,
                    GetProcessorGpioAddress(Board.Current.Processor));

                bscAddress = OP.mmap(
                    IntPtr.Zero,
                    OP.BCM2835_BLOCK_SIZE,
                    OP.PROT_READ | OP.PROT_WRITE,
                    OP.MAP_SHARED,
                    memoryFile,
                    bscBase);
            }
            finally
            {
                OP.close(memoryFile);
            }

            if (bscAddress == (IntPtr)OP.MAP_FAILED)
                throw new InvalidOperationException("Unable to access device memory");

            // Set the I2C pins to the Alt 0 function to enable I2C access on them
            // remembers if the values were actually changed to clear them or not upon dispose
            wasSdaPinSet = SetPinMode((uint)(int)sdaPin, OP.BCM2835_GPIO_FSEL_ALT0); // SDA
            wasSclPinSet = SetPinMode((uint)(int)sclPin, OP.BCM2835_GPIO_FSEL_ALT0); // SCL

            // Read the clock divider register
            var dividerAddress = bscAddress + (int)OP.BCM2835_BSC_DIV;
            var divider = (ushort)SafeReadUInt32(dividerAddress);
            waitInterval = GetWaitInterval(divider);

            var addressAddress = bscAddress + (int)OP.BCM2835_BSC_A;
            SafeWriteUInt32(addressAddress, (uint)currentDeviceAddress);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Set all the I2C/BSC1 pins back to original values if changed
            if (wasSdaPinSet)
            {
                SetPinMode((uint)(int)sdaPin, OP.BCM2835_GPIO_FSEL_INPT); // SDA
            }
            if (wasSclPinSet)
            {
                SetPinMode((uint)(int)sclPin, OP.BCM2835_GPIO_FSEL_INPT); // SCL
            }

            OP.munmap(gpioAddress, OP.BCM2835_BLOCK_SIZE);
            OP.munmap(bscAddress, OP.BCM2835_BLOCK_SIZE);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the clock divider.
        /// </summary>
        /// <value>
        /// The clock divider.
        /// </value>
        public int ClockDivider
        {
            get
            {
                var dividerAddress = bscAddress + (int)OP.BCM2835_BSC_DIV;
                return (ushort)SafeReadUInt32(dividerAddress);
            }
            set
            {
                var dividerAddress = bscAddress + (int)OP.BCM2835_BSC_DIV;
                SafeWriteUInt32(dividerAddress, (uint)value);

                var actualDivider = (ushort)SafeReadUInt32(dividerAddress);
                waitInterval = GetWaitInterval(actualDivider);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Connects the specified device address.
        /// </summary>
        /// <param name="deviceAddress">The device address.</param>
        /// <returns>The device connection</returns>
        public I2cDeviceConnection Connect(int deviceAddress)
        {
            return new I2cDeviceConnection(this, deviceAddress);
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Reads I2C using repeated start.
        /// </summary>
        /// <param name="deviceAddress"></param>
        /// <param name="commands"></param>
        /// <param name="byteCount"></param>
        internal byte[] WriteRead(int deviceAddress, byte[] commands, int byteCount)
        {
            lock (driverLock)
            {
                this.EnsureDeviceAddress(deviceAddress);

                var len = (uint)commands.Length;
                var buffer = new byte[byteCount];

                var dlen = this.bscAddress + (int)OP.BCM2835_BSC_DLEN;
                var fifo = this.bscAddress + (int)OP.BCM2835_BSC_FIFO;
                var status = this.bscAddress + (int)OP.BCM2835_BSC_S;
                var control = this.bscAddress + (int)OP.BCM2835_BSC_C;

                var remaining = len;
                var i = 0;

                // Clear FIFO
                WriteUInt32Mask(control, OP.BCM2835_BSC_C_CLEAR_1, OP.BCM2835_BSC_C_CLEAR_1);

                // Clear Status
                WriteUInt32(status, OP.BCM2835_BSC_S_CLKT | OP.BCM2835_BSC_S_ERR | OP.BCM2835_BSC_S_DONE);

                // Set Data Length to write command
                WriteUInt32(dlen, len);

                /* pre populate FIFO with max buffer */
                while (remaining != 0 && i < OP.BCM2835_BSC_FIFO_SIZE)
                {
                    WriteUInt32(fifo, commands[i]);
                    i++;
                    remaining--;
                }

                /* Enable device and start transfer */
                SafeWriteUInt32(control, OP.BCM2835_BSC_C_I2CEN | OP.BCM2835_BSC_C_ST);

                /* poll for transfer has started (way to do repeated start, from BCM2835 datasheet) */
                while ((SafeReadUInt32(status) & OP.BCM2835_BSC_S_TA) == 0)
                {
                    /* Linux may cause us to miss entire transfer stage */
                    if ((ReadUInt32(status) & OP.BCM2835_BSC_S_DONE) != 0)
                        break;
                }

                remaining = (uint)byteCount;
                i = 0;

                /* Send a repeated start with read bit set in address */
                SafeWriteUInt32(dlen, (uint)byteCount);
                SafeWriteUInt32(control, OP.BCM2835_BSC_C_I2CEN | OP.BCM2835_BSC_C_ST | OP.BCM2835_BSC_C_READ);

                /* Wait for write to complete and first byte back. */
                this.Wait((uint)waitInterval * ((uint)commands.Length + 1));

                /* wait for transfer to complete */
                while ((ReadUInt32(status) & OP.BCM2835_BSC_S_DONE) == 0)
                {
                    /* we must empty the FIFO as it is populated and not use any delay */
                    while (remaining != 0 && (SafeReadUInt32(status) & OP.BCM2835_BSC_S_RXD) != 0)
                    {
                        /* Read from FIFO, no barrier */
                        buffer[i] = (byte)ReadUInt32(fifo);
                        i++;
                        remaining--;
                    }
                }

                /* transfer has finished - grab any remaining stuff in FIFO */
                while (remaining != 0 && (SafeReadUInt32(status) & OP.BCM2835_BSC_S_RXD) != 0)
                {
                    /* Read from FIFO */
                    buffer[i] = (byte)SafeReadUInt32(fifo);
                    i++;
                    remaining--;
                }

                if ((SafeReadUInt32(status) & OP.BCM2835_BSC_S_ERR) != 0) // Received a NACK
                    throw new InvalidOperationException("Read operation failed with BCM2835_I2C_REASON_ERROR_NACK status");
                if ((SafeReadUInt32(status) & OP.BCM2835_BSC_S_CLKT) != 0) // Received Clock Stretch Timeout
                    throw new InvalidOperationException("Read operation failed with BCM2835_I2C_REASON_ERROR_CLKT status");
                if (remaining != 0) // Not all data is received
                    throw new InvalidOperationException(string.Format("Read operation failed with BCM2835_I2C_REASON_ERROR_DATA status, missing {0} bytes", remaining));

                WriteUInt32Mask(control, OP.BCM2835_BSC_S_DONE, OP.BCM2835_BSC_S_DONE);

                return buffer;
            }
        }

        /// <summary>
        /// Reads I2C using repeated start.
        /// </summary>
        /// <param name="deviceAddress"></param>
        /// <param name="register"></param>
        /// <param name="byteCount"></param>
        /// <returns></returns>
        internal byte[] WriteRead(int deviceAddress, byte register, int byteCount)
        {
            lock (driverLock)
            {
                this.EnsureDeviceAddress(deviceAddress);

                var len = (uint)byteCount;
                var buffer = new byte[byteCount];

                var dlen = this.bscAddress + (int)OP.BCM2835_BSC_DLEN;
                var fifo = this.bscAddress + (int)OP.BCM2835_BSC_FIFO;
                var status = this.bscAddress + (int)OP.BCM2835_BSC_S;
                var control = this.bscAddress + (int)OP.BCM2835_BSC_C;

                var remaining = len;
                var i = 0;

                // Clear FIFO
                WriteUInt32Mask(control, OP.BCM2835_BSC_C_CLEAR_1, OP.BCM2835_BSC_C_CLEAR_1);

                // Clear Status
                WriteUInt32(status, OP.BCM2835_BSC_S_CLKT | OP.BCM2835_BSC_S_ERR | OP.BCM2835_BSC_S_DONE);

                // Set Data Length, write the register, one byte
                WriteUInt32(dlen, 1);

                // Enable device and start transfer
                WriteUInt32(control, OP.BCM2835_BSC_C_I2CEN);
                WriteUInt32(fifo, register);
                WriteUInt32(control, OP.BCM2835_BSC_C_I2CEN | OP.BCM2835_BSC_C_ST);

                /* poll for transfer has started */
                while ((ReadUInt32(status) & OP.BCM2835_BSC_S_TA) == 0)
                {
                    /* Linux may cause us to miss entire transfer stage */
                    if ((SafeReadUInt32(status) & OP.BCM2835_BSC_S_DONE) != 0)
                        break;
                }

                /* Send a repeated start with read bit set in address */
                WriteUInt32(dlen, (uint)byteCount);
                WriteUInt32(control, OP.BCM2835_BSC_C_I2CEN | OP.BCM2835_BSC_C_ST | OP.BCM2835_BSC_C_READ);

                /* Wait for write to complete and first byte back. */
                uint wt = (uint)waitInterval * 3;
                //Console.WriteLine(wt);
                this.Wait(wt);


                /* wait for transfer to complete */
                while ((ReadUInt32(status) & OP.BCM2835_BSC_S_DONE) == 0)
                {
                    /* we must empty the FIFO as it is populated and not use any delay */
                    while (remaining != 0 && (ReadUInt32(status) & OP.BCM2835_BSC_S_RXD) != 0)
                    {
                        /* Read from FIFO */
                        buffer[i] = (byte)ReadUInt32(fifo);
                        i++;
                        remaining--;
                    }
                }

                /* transfer has finished - grab any remaining stuff in FIFO */
                while (remaining != 0 && (ReadUInt32(status) & OP.BCM2835_BSC_S_RXD) != 0)
                {
                    /* Read from FIFO */
                    buffer[i] = (byte)ReadUInt32(fifo);
                    i++;
                    remaining--;
                }

                if ((SafeReadUInt32(status) & OP.BCM2835_BSC_S_ERR) != 0) // Received a NACK
                    throw new InvalidOperationException("Read operation failed with BCM2835_I2C_REASON_ERROR_NACK status");
                if ((SafeReadUInt32(status) & OP.BCM2835_BSC_S_CLKT) != 0) // Received Clock Stretch Timeout
                    throw new InvalidOperationException("Read operation failed with BCM2835_I2C_REASON_ERROR_CLKT status");
                if (remaining != 0) // Not all data is received
                    throw new InvalidOperationException(string.Format("Read operation failed with BCM2835_I2C_REASON_ERROR_DATA status, missing {0} bytes", remaining));

                WriteUInt32Mask(control, OP.BCM2835_BSC_S_DONE, OP.BCM2835_BSC_S_DONE);

                return buffer;
            }
        }

        internal void Write(int deviceAddress, byte[] buffer)
        {
            this.EnsureDeviceAddress(deviceAddress);

            var len = (uint)buffer.Length;

            var dlen = this.bscAddress + (int)OP.BCM2835_BSC_DLEN;
            var fifo = this.bscAddress + (int)OP.BCM2835_BSC_FIFO;
            var status = this.bscAddress + (int)OP.BCM2835_BSC_S;
            var control = this.bscAddress + (int)OP.BCM2835_BSC_C;

            var remaining = len;
            var i = 0;

            // Clear FIFO
            WriteUInt32Mask(control, OP.BCM2835_BSC_C_CLEAR_1, OP.BCM2835_BSC_C_CLEAR_1);

            // Clear Status
            WriteUInt32(status, OP.BCM2835_BSC_S_CLKT | OP.BCM2835_BSC_S_ERR | OP.BCM2835_BSC_S_DONE);

            // Set Data Length
            WriteUInt32(dlen, len);

            while (remaining != 0 && i < OP.BCM2835_BSC_FIFO_SIZE)
            {
                WriteUInt32(fifo, buffer[i]);
                i++;
                remaining--;
            }

            // Enable device and start transfer
            WriteUInt32(control, OP.BCM2835_BSC_C_I2CEN | OP.BCM2835_BSC_C_ST);

            while ((ReadUInt32(status) & OP.BCM2835_BSC_S_DONE) == 0)
            {
                while (remaining != 0 && (ReadUInt32(status) & OP.BCM2835_BSC_S_TXD) != 0)
                {
                    // Write to FIFO, no barrier
                    WriteUInt32(fifo, buffer[i]);
                    i++;
                    remaining--;
                }

                this.Wait(remaining);
            }

            if ((SafeReadUInt32(status) & OP.BCM2835_BSC_S_ERR) != 0) // Received a NACK
                throw new InvalidOperationException("Read operation failed with BCM2835_I2C_REASON_ERROR_NACK status");
            if ((SafeReadUInt32(status) & OP.BCM2835_BSC_S_CLKT) != 0) // Received Clock Stretch Timeout
                throw new InvalidOperationException("Read operation failed with BCM2835_I2C_REASON_ERROR_CLKT status");
            if (remaining != 0) // Not all data is sent
                throw new InvalidOperationException(string.Format("Read operation failed with BCM2835_I2C_REASON_ERROR_DATA status, missing {0} bytes", remaining));

            WriteUInt32Mask(control, OP.BCM2835_BSC_S_DONE, OP.BCM2835_BSC_S_DONE);
        }

        internal byte[] Read(int deviceAddress, int byteCount)
        {
            lock (driverLock)
            {
                this.EnsureDeviceAddress(deviceAddress);

                var dlen = this.bscAddress + (int)OP.BCM2835_BSC_DLEN;
                var fifo = this.bscAddress + (int)OP.BCM2835_BSC_FIFO;
                var status = this.bscAddress + (int)OP.BCM2835_BSC_S;
                var control = this.bscAddress + (int)OP.BCM2835_BSC_C;

                var remaining = (uint)byteCount;
                uint i = 0;

                // Clear FIFO
                WriteUInt32Mask(control, OP.BCM2835_BSC_C_CLEAR_1, OP.BCM2835_BSC_C_CLEAR_1);

                // Clear Status
                WriteUInt32(status, OP.BCM2835_BSC_S_CLKT | OP.BCM2835_BSC_S_ERR | OP.BCM2835_BSC_S_DONE);

                // Set Data Length
                WriteUInt32(dlen, (uint)byteCount);

                // Start read
                WriteUInt32(control, OP.BCM2835_BSC_C_I2CEN | OP.BCM2835_BSC_C_ST | OP.BCM2835_BSC_C_READ);

                var buffer = new byte[byteCount];
                while ((ReadUInt32(status) & OP.BCM2835_BSC_S_DONE) == 0)
                {
                    while ((ReadUInt32(status) & OP.BCM2835_BSC_S_RXD) != 0)
                    {
                        // Read from FIFO, no barrier
                        buffer[i] = (byte)ReadUInt32(fifo);
                        i++;
                        remaining--;
                    }

                    this.Wait(remaining);
                }

                while (remaining != 0 && (ReadUInt32(status) & OP.BCM2835_BSC_S_RXD) != 0)
                {
                    buffer[i] = (byte)ReadUInt32(fifo);
                    i++;
                    remaining--;
                }

                if ((SafeReadUInt32(status) & OP.BCM2835_BSC_S_ERR) != 0) // Received a NACK
                    throw new InvalidOperationException("Read operation failed with BCM2835_I2C_REASON_ERROR_NACK status");
                if ((SafeReadUInt32(status) & OP.BCM2835_BSC_S_CLKT) != 0) // Received Clock Stretch Timeout
                    throw new InvalidOperationException("Read operation failed with BCM2835_I2C_REASON_ERROR_CLKT status");
                if (remaining != 0) // Not all data is received
                    throw new InvalidOperationException(string.Format("Read operation failed with BCM2835_I2C_REASON_ERROR_DATA status, missing {0} bytes", remaining));

                WriteUInt32Mask(control, OP.BCM2835_BSC_S_DONE, OP.BCM2835_BSC_S_DONE);

                return buffer;
            }
        }

        private static uint GetProcessorBscAddress(Processor processor)
        {
            switch (processor)
            {
                case Processor.Bcm2708:
                    return OP.BCM2835_BSC1_BASE;

                case Processor.Bcm2709:
                case Processor.Bcm2835:
                    return OP.BCM2836_BSC1_BASE;

                default:
                    throw new ArgumentOutOfRangeException("processor");
            }
        }

        private static uint GetProcessorGpioAddress(Processor processor)
        {
            switch (processor)
            {
                case Processor.Bcm2708:
                    return OP.BCM2835_GPIO_BASE;

                case Processor.Bcm2709:
                case Processor.Bcm2835:
                    return OP.BCM2836_GPIO_BASE;

                default:
                    throw new ArgumentOutOfRangeException("processor");
            }
        }

        private void EnsureDeviceAddress(int deviceAddress)
        {
            if (deviceAddress != currentDeviceAddress)
            {
                var addressAddress = bscAddress + (int)OP.BCM2835_BSC_A;
                SafeWriteUInt32(addressAddress, (uint)deviceAddress);

                currentDeviceAddress = deviceAddress;
            }
        }

        private void Wait(uint remaining)
        {
            // When remaining data is to be received, then wait for a fully FIFO
            if (remaining != 0)
                Timer.Sleep(TimeSpan.FromMilliseconds(waitInterval * (remaining >= OP.BCM2835_BSC_FIFO_SIZE ? OP.BCM2835_BSC_FIFO_SIZE : remaining) / 1000d));
        }

        private static int GetWaitInterval(ushort actualDivider)
        {
            // Calculate time for transmitting one byte
            // 1000000 = micros seconds in a second
            // 9 = Clocks per byte : 8 bits + ACK

            return (int)((decimal)actualDivider * 1000000 * 9 / OP.BCM2835_CORE_CLK_HZ);
        }

        private static uint GetBscBase(ProcessorPin sdaPin, ProcessorPin sclPin)
        {
            switch (GpioConnectionSettings.ConnectorPinout)
            {
                case ConnectorPinout.Rev1:
                    if (sdaPin == ProcessorPin.Gpio0 && sclPin == ProcessorPin.Gpio1)
                        return OP.BCM2835_BSC0_BASE;
                    throw new InvalidOperationException("No I2C device exist on the specified pins");

                case ConnectorPinout.Rev2:
                    if (sdaPin == ProcessorPin.Gpio28 && sclPin == ProcessorPin.Gpio29)
                        return OP.BCM2835_BSC0_BASE;
                    if (sdaPin == ProcessorPin.Gpio2 && sclPin == ProcessorPin.Gpio3)
                        return OP.BCM2835_BSC1_BASE;
                    throw new InvalidOperationException("No I2C device exist on the specified pins");

                case ConnectorPinout.Plus:
                    if (sdaPin == ProcessorPin.Gpio2 && sclPin == ProcessorPin.Gpio3)
                        return GetProcessorBscAddress(Board.Current.Processor);
                    throw new InvalidOperationException("No I2C device exist on the specified pins");

                default:
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Connector pinout {0} is not supported", GpioConnectionSettings.ConnectorPinout));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="mode"></param>
        /// <returns>True when value was changed, false otherwise.</returns>
        private bool SetPinMode(uint pin, uint mode)
        {
            // Function selects are 10 pins per 32 bit word, 3 bits per pin
            var paddr = gpioAddress + (int)(OP.BCM2835_GPFSEL0 + 4 * (pin / 10));
            var shift = (pin % 10) * 3;
            var mask = OP.BCM2835_GPIO_FSEL_MASK << (int)shift;
            var value = mode << (int)shift;

            var existing = ReadUInt32(paddr) & mask;
            if (existing != value)
            {
                //Console.WriteLine($"existing is {x} masked:{x & mask} vs mask:{mask} value:{value}");
                WriteUInt32Mask(paddr, value, mask);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void WriteUInt32Mask(IntPtr address, uint value, uint mask)
        {
            var v = SafeReadUInt32(address);
            v = (v & ~mask) | (value & mask);
            SafeWriteUInt32(address, v);
        }

        private static uint SafeReadUInt32(IntPtr address)
        {
            // Make sure we dont return the _last_ read which might get lost
            // if subsequent code changes to a different peripheral
            unchecked
            {
                var returnValue = (uint)Marshal.ReadInt32(address);
                Marshal.ReadInt32(address);

                return returnValue;
            }
        }

        /// <summary>
        /// Read no barrier
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private static uint ReadUInt32(IntPtr address)
        {
            unchecked
            {
                return (uint)Marshal.ReadInt32(address);
            }
        }

        private static void SafeWriteUInt32(IntPtr address, uint value)
        {
            // Make sure we don't rely on the first write, which may get
            // lost if the previous access was to a different peripheral.
            unchecked
            {
                Marshal.WriteInt32(address, (int)value);
                Marshal.WriteInt32(address, (int)value);
            }
        }

        /// <summary>
        /// Write no barrier
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        private static void WriteUInt32(IntPtr address, uint value)
        {
            unchecked
            {
                Marshal.WriteInt32(address, (int)value);
            }
        }

        #endregion
    }
}
