using RaspberrySharp.IO.GeneralPurpose;
using RaspberrySharp.IO.InterIntegratedCircuit;
using System;

namespace Tests.I2C.LTC2943
{
    class Program
    {
        private static I2cDriver driver;
        private static I2cDeviceConnection i2cConnection;

        static void Main(string[] args)
        {

            Console.WriteLine("LTC2943 Test");
            Console.WriteLine("===========");

            Console.WriteLine("Opening Connection...");

            try
            {
                driver = new I2cDriver(ProcessorPin.Gpio02, ProcessorPin.Gpio03);
                i2cConnection = driver.Connect(0x64);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unable to open connection.");
                Console.WriteLine(ex);
                Console.WriteLine("Press any key to close.");
                Console.Read();
                Environment.Exit(1);
            }

            Console.WriteLine("Connection open!");

            AskForKey();
        }

        public static void AskForKey()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("A = EnableLTC");
            Console.WriteLine("B = Temperature 0x14");
            Console.WriteLine("C = Voltage 0x08");
            Console.WriteLine("X = Exit");
            Console.ForegroundColor = ConsoleColor.Cyan;

            while (!Console.KeyAvailable)
            {
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.A:
                        EnableLTC();
                        Console.WriteLine("EnableLTC, send 0xF4");
                        break;
                    case ConsoleKey.B:
                        Console.WriteLine("Temperature 0x14,0x15");
                        var resT = ReadTemperature();
                        Console.WriteLine("result: {0}", resT);
                        break;
                    case ConsoleKey.C:
                        Console.WriteLine("Voltage 0x08, 0x09");
                        var resV = ReadVoltage();
                        Console.WriteLine("result: {0}", resV);
                        break;
                    case ConsoleKey.X:
                        driver.Dispose();
                        Environment.Exit(0);
                        break;
                    default:
                        break;
                }

                AskForKey();
            }
        }





        //I2C METHODS

        private static void EnableLTC()
        {
            i2cConnection.Write(0x01, 0xF4);
        }

        private static float ReadTemperature()
        {
            //byte[] result = i2cConnection.Read(0x14, 2);
            //foreach (var b in result)
            //    Console.WriteLine("result: {0}", b);

            byte[] result = new byte[2];
            result[0] = i2cConnection.Read(0x14, 1)[0];
            result[1] = i2cConnection.Read(0x15, 1)[0];
            var tempCode = result[0] * 256 + result[1];
            return tempCode * ((float)(510f) / 65535) - 273.15f;

        }

        private static float ReadVoltage()
        {
            byte[] result = i2cConnection.Read(0x08, 2);
            foreach (var b in result)
                Console.WriteLine("result: {0}", b);

            var voltageADCCode = result[0] * 256 + result[1];

            return ((float)voltageADCCode / 65535) * 23.6f;
        }
    }
}
