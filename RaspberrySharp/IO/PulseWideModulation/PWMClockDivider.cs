using System;

namespace RaspberrySharp.IO.PulseWideModulation
{
    public enum PWMClockDivider: UInt32
    {
        BCM2835_PWM_CLOCK_DIVIDER_2048 = 2048,    /*!< 2048 = 9.375kHz */
        BCM2835_PWM_CLOCK_DIVIDER_1024 = 1024,    /*!< 1024 = 18.75kHz */
        BCM2835_PWM_CLOCK_DIVIDER_512 = 512,     /*!< 512 = 37.5kHz */
        BCM2835_PWM_CLOCK_DIVIDER_256 = 256,     /*!< 256 = 75kHz */
        BCM2835_PWM_CLOCK_DIVIDER_128 = 128,     /*!< 128 = 150kHz */
        BCM2835_PWM_CLOCK_DIVIDER_64 = 64,      /*!< 64 = 300kHz */
        BCM2835_PWM_CLOCK_DIVIDER_32 = 32,      /*!< 32 = 600.0kHz */
        BCM2835_PWM_CLOCK_DIVIDER_16 = 16,      /*!< 16 = 1.2MHz */
        BCM2835_PWM_CLOCK_DIVIDER_8 = 8,       /*!< 8 = 2.4MHz */
        BCM2835_PWM_CLOCK_DIVIDER_4 = 4,       /*!< 4 = 4.8MHz */
        BCM2835_PWM_CLOCK_DIVIDER_2 = 2,       /*!< 2 = 9.6MHz, fastest you can get */
        BCM2835_PWM_CLOCK_DIVIDER_1 = 1        /*!< 1 = 4.6875kHz, same as divider 4096 */
    }
}
