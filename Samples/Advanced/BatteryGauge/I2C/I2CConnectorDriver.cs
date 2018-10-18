using RaspberrySharp.IO.GeneralPurpose;
using RaspberrySharp.IO.InterIntegratedCircuit;

namespace BatteryGauge.I2C
{
    /// <summary>
    /// Static classe to be shared among all I2C services, because I2c pins are always the same
    /// </summary>
    public static class I2CConnectorDriver
    {
        static ProcessorPin SerialDataPin = ProcessorPin.Gpio2;//.P1Pin03;
        static ProcessorPin SerialClockPin = ProcessorPin.Gpio3;//.P1Pin05;
        private static I2cDriver _driver;
        public static I2cDriver Driver
        {
            get
            {
                if (_driver == null)
                    _driver = new I2cDriver(SerialDataPin, SerialClockPin);
                return _driver;
            }
        }
    }
}
