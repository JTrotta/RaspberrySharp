using RaspberrySharp.IO.GeneralPurpose;
using RaspberrySharp.IO.InterIntegratedCircuit;

namespace SampleCommon.Buses
{
    /// <summary>
    /// Static class to be shared among all I2C services, because I2c pins are always the same
    /// </summary>
    public static class I2CConnectorDriver
    {
        private static readonly ProcessorPin _serialDataPin = ProcessorPin.Gpio2;
        private static readonly ProcessorPin _serialClockPin = ProcessorPin.Gpio3;
        private static I2cDriver _driver;
        public static I2cDriver Driver
        {
            get
            {
                if (_driver == null)
                    _driver = new I2cDriver(_serialDataPin, _serialClockPin, true);
                return _driver;
            }
        }
    }
}
