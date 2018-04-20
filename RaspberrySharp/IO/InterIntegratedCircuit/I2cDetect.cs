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
            for (byte address = 3; address < 120; address++)
            //byte address = 0x62;
            {
                _deviceConnection = _driver.Connect(address);
                try
                {
                    var result = _deviceConnection.Read(1)[0];
                    nDevices.Add(address);
                }
                catch(Exception e)
                {
                    //Console.WriteLine($"No Dev {address}: {e.Message}");
                }
            }
            return nDevices;
        }
    }
    
}
