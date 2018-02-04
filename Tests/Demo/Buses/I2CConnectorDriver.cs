using RaspberrySharp.IO.GeneralPurpose;
using RaspberrySharp.IO.InterIntegratedCircuit;

namespace Demo.Buses
{
    /// <summary>
    /// Classe da condividere con tutti i service I2C, poichè i pin sono sempre gli stessi
    /// </summary>
    public static class I2CConnectorDriver
    {
        static ProcessorPin SerialDataPin = ProcessorPin.Gpio02;//.P1Pin03;
        static ProcessorPin SerialClockPin = ProcessorPin.Gpio05;//.P1Pin05;
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
