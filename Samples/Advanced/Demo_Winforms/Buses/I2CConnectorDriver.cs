using RaspberrySharp.IO.GeneralPurpose;
using RaspberrySharp.IO.InterIntegratedCircuit;

namespace Demo.Buses
{
    /// <summary>
    /// This static class should be shared among I2C services, that's because I2C pins are qlways the same
    /// </summary>
    public static class I2CConnectorDriver
    {
        static ProcessorPin SerialDataPin = ProcessorPin.Gpio02;//.P1Pin03;
        static ProcessorPin SerialClockPin = ProcessorPin.Gpio03;//.P1Pin05;
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
