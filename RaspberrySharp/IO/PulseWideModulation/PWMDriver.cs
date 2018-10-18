using RaspberrySharp.IO.Interop;
using RaspberrySharp.System;
using RaspberrySharp.System.Timers;
using System;
using System.Runtime.InteropServices;

namespace RaspberrySharp.IO.PulseWideModulation
{
    public class PWMDriver: IDisposable
    {
        #region Fields

        private readonly IntPtr bcm2835_clk;
        private readonly IntPtr bcm2835_pwm;

        //private int waitInterval;

        #endregion

        #region Instance Management

        public PWMDriver()
        {
            var memoryFile = OP.open("/dev/mem", OP.O_RDWR | OP.O_SYNC);
            try
            {
                bcm2835_pwm = OP.mmap(
                    IntPtr.Zero,
                    OP.BCM2835_BLOCK_SIZE,
                    OP.PROT_READ | OP.PROT_WRITE,
                    OP.MAP_SHARED,
                    memoryFile,
                    GetProcessorPwmAddress(Board.Current.Processor));

                bcm2835_clk = OP.mmap(
                    IntPtr.Zero,
                    OP.BCM2835_BLOCK_SIZE,
                    OP.PROT_READ | OP.PROT_WRITE,
                    OP.MAP_SHARED,
                    memoryFile,
                    GetProcessorClkAddress(Board.Current.Processor));
            }
            finally
            {
                OP.close(memoryFile);
            }

            ////// Read the clock divider register
            ////var dividerAddress = bscAddress + (int)OP.BCM2835_BSC_DIV;
            ////var divider = (ushort)SafeReadUInt32(dividerAddress);
            ////waitInterval = GetWaitInterval(divider);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            OP.munmap(bcm2835_pwm, OP.BCM2835_BLOCK_SIZE);
            OP.munmap(bcm2835_clk, OP.BCM2835_BLOCK_SIZE);
        }
        #endregion

        #region Methods
        /// <summary>
        /// implements bcm2835_pwm_set_clock
        /// </summary>
        /// <param name="divisor"></param>
        public void  Set_Clock(PWMClockDivider clockDivider)
        {
            /* From Gerts code */
            UInt32 divisor = (UInt32)clockDivider & 0xfff;
            /* Stop PWM clock */
            //bcm2835_peri_write(bcm2835_clk + BCM2835_PWMCLK_CNTL, BCM2835_PWM_PASSWRD | 0x01);
            SafeWriteUInt32(bcm2835_clk + (int)OP.BCM2835_PWMCLK_CNTL, OP.BCM2835_PWM_PASSWRD | 0x01);
            //bcm2835_delay(110); /* Prevents clock going slow */
            Wait(110);

            /* Wait for the clock to be not busy */
            //while ((bcm2835_peri_read(bcm2835_clk + BCM2835_PWMCLK_CNTL) & 0x80) != 0)
            //    bcm2835_delay(1);
            while ((SafeReadUInt32(bcm2835_clk + (int)OP.BCM2835_PWMCLK_CNTL) & 0x80) == 0)
            {
                Wait(1);
            }

            /* set the clock divider and enable PWM clock */
            //bcm2835_peri_write(bcm2835_clk + BCM2835_PWMCLK_DIV, BCM2835_PWM_PASSWRD | (divisor << 12));
            SafeWriteUInt32(bcm2835_clk + (int)OP.BCM2835_PWMCLK_DIV, OP.BCM2835_PWM_PASSWRD | (divisor << 12));
            //bcm2835_peri_write(bcm2835_clk + BCM2835_PWMCLK_CNTL, BCM2835_PWM_PASSWRD | 0x11); /* Source=osc and enable */
            SafeWriteUInt32(bcm2835_clk + (int)OP.BCM2835_PWMCLK_CNTL, OP.BCM2835_PWM_PASSWRD | 0x11);
        }

        /// <summary>
        /// Implements bcm2835_pwm_set_mode
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="markspace"></param>
        /// <param name="enabled"></param>
        public void Set_Mode(PWMChannelType channel, bool markspace, bool enabled)
        {
            ////if (bcm2835_clk == MAP_FAILED
            ////     || bcm2835_pwm == MAP_FAILED)
            ////    return; /* bcm2835_init() failed or not root */

            UInt32 control = SafeReadUInt32(bcm2835_pwm + (int)OP.BCM2835_PWM_CONTROL);

            switch (channel)
            {
                case PWMChannelType.PWMChannel0:
                    if (markspace)
                        control |= OP.BCM2835_PWM0_MS_MODE;
                    else
                        control &= ~OP.BCM2835_PWM0_MS_MODE;
                    if (enabled)
                        control |= OP.BCM2835_PWM0_ENABLE;
                    else
                        control &= ~OP.BCM2835_PWM0_ENABLE;
                    break;
                case PWMChannelType.PWMChannel1:
                    if (markspace)
                        control |= OP.BCM2835_PWM1_MS_MODE;
                    else
                        control &= ~OP.BCM2835_PWM1_MS_MODE;
                    if (enabled)
                        control |= OP.BCM2835_PWM1_ENABLE;
                    else
                        control &= ~OP.BCM2835_PWM1_ENABLE;
                    break;
            }

            /* If you use the barrier here, wierd things happen, and the commands dont work */
            //bcm2835_peri_write_nb(bcm2835_pwm + BCM2835_PWM_CONTROL, control);
            WriteUInt32(bcm2835_pwm + (int)OP.BCM2835_PWM_CONTROL, control);
            /*  bcm2835_peri_write_nb(bcm2835_pwm + BCM2835_PWM_CONTROL, BCM2835_PWM0_ENABLE | BCM2835_PWM1_ENABLE | BCM2835_PWM0_MS_MODE | BCM2835_PWM1_MS_MODE); */
        }

        /// <summary>
        /// Implements bcm2835_pwm_set_range
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="range"></param>
        public void Set_Range(PWMChannelType channel, UInt32 range)
        {
            //if (bcm2835_clk == MAP_FAILED
            //     || bcm2835_pwm == MAP_FAILED)
            //    return; /* bcm2835_init() failed or not root */

            switch (channel)
            {
                case PWMChannelType.PWMChannel0:
                    WriteUInt32(bcm2835_pwm + (int)OP.BCM2835_PWM0_RANGE, range);
                    break;
                case PWMChannelType.PWMChannel1:
                    WriteUInt32(bcm2835_pwm + (int)OP.BCM2835_PWM1_RANGE, range);
                    break;
            }
        }

        /// <summary>
        /// Implements bcm2835_pwm_set_data
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="data"></param>
        public void Set_Data(PWMChannelType channel, UInt32 data)
        {
            //if (bcm2835_clk == MAP_FAILED
            //     || bcm2835_pwm == MAP_FAILED)
            //    return; /* bcm2835_init() failed or not root */

            switch (channel)
            {
                case PWMChannelType.PWMChannel0:
                    WriteUInt32(bcm2835_pwm + (int)OP.BCM2835_PWM0_DATA, data);
                    break;
                case PWMChannelType.PWMChannel1:
                    WriteUInt32(bcm2835_pwm + (int)OP.BCM2835_PWM1_DATA, data);
                    break;
            }
        }
        #endregion

        #region helpers
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
        /// Read with memory barriers from peripheral (bcm2835_peri_read)
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
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
        /// Write no barrier (bcm2835_peri_write_nb)
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

        /// <summary>
        /// Read no barrier (bcm2835_peri_read_nb)
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

        private void Wait(uint remaining)
        {
            //////// When remaining data is to be received, then wait for a fully FIFO
            //////if (remaining != 0)
            //////    Timer.Sleep(TimeSpan.FromMilliseconds(waitInterval * (remaining >= OP.BCM2835_BSC_FIFO_SIZE ? OP.BCM2835_BSC_FIFO_SIZE : remaining) / 1000d));
        }

        private uint GetProcessorPwmAddress(Processor processor)
        {
            switch (processor)
            {
                case Processor.Bcm2708:
                    return OP.BCM2835_GPIO_PWM;

                case Processor.Bcm2709:
                    return OP.BCM2836_GPIO_PWM;

                default:
                    throw new ArgumentOutOfRangeException("processor");
            }
        }

        private uint GetProcessorClkAddress(Processor processor)
        {
            switch (processor)
            {
                case Processor.Bcm2708:
                    return OP.BCM2835_CLOCK_BASE;

                case Processor.Bcm2709:
                    return OP.BCM2836_CLOCK_BASE;

                default:
                    throw new ArgumentOutOfRangeException("processor");
            }
        }
        #endregion
    }
}
