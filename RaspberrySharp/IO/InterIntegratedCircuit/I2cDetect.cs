using RaspberrySharp.IO.GeneralPurpose;
using System.Collections.Generic;

namespace RaspberrySharp.IO.InterIntegratedCircuit
{
    public class I2cDetect
    {
        private static I2cDriver _driver;
        private I2cDeviceConnection _deviceConnection;
        private int nDevices;

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
                byte error = _deviceConnection.Read(1)[0];

                if (error == 0)
                {
                    //Serial.print("I2C device found at address 0x");
                    //if (address < 16)
                    //    Serial.print("0");
                    //Serial.print(address, HEX);
                    //Serial.println("  !");

                    nDevices.Add(address);
                }
                else if (error == 4)
                {
                    //Serial.print("Unknow error at address 0x");
                    //if (address < 16)
                    //    Serial.print("0");
                    //Serial.println(address, HEX);
                }
            }
            //if (nDevices.Count == 0)
            //    Serial.println("No I2C devices found\n");
            //else
            //    Serial.println("done\n");

            //delay(5000);           // wait 5 seconds for next scan
            return nDevices;
        }
    }
    
}
