using System;
using RaspberrySharp.IO.GeneralPurpose;
using System.Collections.Generic;

namespace RaspberrySharp.IO.InterIntegratedCircuit
{
    public class I2cDetect
    {
        private static I2cDriver _driver;
        private I2cDeviceConnection _deviceConnection;
        //private int nDevices;

        public I2cDetect()
        {
            ProcessorPin SerialDataPin = ProcessorPin.Gpio2;//.P1Pin03;
            ProcessorPin SerialClockPin = ProcessorPin.Gpio3;//.P1Pin05;
            _driver = new I2cDriver(SerialDataPin, SerialClockPin);
        }

        public List<byte> Detect()
        {
            List<byte> nDevices = new List<byte>();
            for (byte address = 1; address < 127; address++)
            {
                // The i2c_scanner uses the return value of
                // the Write.endTransmisstion to see if
                // a device did acknowledge to the address.
                _deviceConnection = _driver.Connect(address);
                byte result = 4;
                try
                {
                    result = _deviceConnection.Read(1)[0];
                    if (result != 0)
                    {
                        nDevices.Add(address);
                    }
                }
                catch
                {
                    Console.WriteLine($"No Dev {address}");
                }
            }
            return nDevices;
        }
    }
    
}
