// **************************************************************
// Project     : PWMTest
// Filename    : Program.cs
// Github      : https://github.com/JTrotta/RaspberrySharp
// **************************************************************
// Github Wiki : https://github.com/JTrotta/RaspberrySharp/wiki
// DataSheet   : 
// **************************************************************
// Links       :
// **************************************************************
// Notes
// Shows how to use PWM to control GPIO pins
// Connect a LED between GPIO18 and GND to observe the LED changing in brightness
// **************************************************************


using RaspberrySharp.IO.PulseWideModulation;
using RaspberrySharp.System.Timers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PWMTest
{
    public class Program
    {
        private const int RANGE = 1024;

        static void Main(string[] args)
        {
            Console.WriteLine("PWM Test");
            Console.WriteLine("===============");

            PWMDriver pwmDriver = new PWMDriver();
            // Clock divider is set to 16.
            // With a divider of 16 and a RANGE of 1024, in MARKSPACE mode,
            // the pulse repetition frequency will be
            // 1.2MHz/1024 = 1171.875Hz, suitable for driving a DC motor with PWM
            pwmDriver.Set_Clock(PWMClockDivider.BCM2835_PWM_CLOCK_DIVIDER_16);
            pwmDriver.Set_Mode(PWMChannelType.PWMChannel0, true, true);
            pwmDriver.Set_Range(PWMChannelType.PWMChannel0, RANGE);

            // Vary the PWM m/s ratio between 1/RANGE and (RANGE-1)/RANGE
            int direction = 1;
            int data = 1;
            int j;

            for (j = 0; j < 4096; j++)
            {
                if (j == 2048)
                {
                    //bcm2835_gpio_write(CTL_PIN, LOW);
                    //	      bcm2835_gpio_write(CTL_PIN2, LOW);
                }
                if (data == 1)
                    direction = 1;
                else if (data == RANGE - 1)
                    direction = -1;
                data += direction;
                pwmDriver.Set_Data(PWMChannelType.PWMChannel0, (uint)data);
                //pwmDriver.Set_Data(PWMChannelType.PWMChannel1, data);
                Timer.Sleep(TimeSpan.FromMilliseconds(10));

            }
            pwmDriver.Set_Data(PWMChannelType.PWMChannel0, 0);
            //pwmDriver.Set_Data(PWMChannelType.PWMChannel1, 0);

            pwmDriver.Dispose();
        }
    }
}
