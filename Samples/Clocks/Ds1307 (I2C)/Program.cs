// **************************************************************
// Project     : Ds1307
// Filename    : Program.cs
// Github      : https://github.com/JTrotta/RaspberrySharp
// **************************************************************
// Github Wiki : https://github.com/JTrotta/RaspberrySharp/wiki/DS1307---I2C---Clock
// DataSheet   : http://www.alldatasheet.com/datasheet-pdf/pdf/58481/DALLAS/DS1307.html
// **************************************************************
// Links       : http://www.linux-ratgeber.de/realtime-clock-rtc-ds1307-am-raspberry-pi-betreiben/
//               https://github.com/PaulStoffregen/DS1307RTC (Arduino)
// **************************************************************
// Notes
// * The Clock Module needs to be modified to work with the Raspberry Pi according to this article
//   http://electronics.stackexchange.com/questions/98361/how-to-modify-ds1307-rtc-to-use-3-3v-for-raspberry-pi
// * The Datasheet can be found here:  (the Memory Map is on Page 5. Note that the values in RAM are stored in Nibbles).
// **************************************************************

#region Usings

using System;
using System.Threading;
using RaspberrySharp.Components.Clocks;
using RaspberrySharp.IO.GeneralPurpose;
using RaspberrySharp.IO.InterIntegratedCircuit;

#endregion

namespace Ds1307
{
    /// <summary>
    /// Sample for reading the DS1307 Clock Chip
    /// </summary>
    public class Program
    {
        private static Ds1307_I2C _clock;
        private static I2cDriver _driver;
        private static I2cDeviceConnection _i2CConnection;

        public static void AskForKey()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("A = Enable Rtc");
            Console.WriteLine("B = Disable Rtc");
            Console.WriteLine("C = Get Date (reads and shows date from the RTC every second)");
            Console.WriteLine("D = Set Date");
            Console.WriteLine("E = Reset To Factory Defaults");
            Console.WriteLine("F = Is Rtc Enabled");
            Console.WriteLine("X = Exit");
            Console.ForegroundColor = ConsoleColor.Cyan;

            while (!Console.KeyAvailable)
            {
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.A:
                        _clock.EnableRtc();
                        Console.WriteLine("Clock Enabled");
                        break;
                    case ConsoleKey.B:
                        _clock.DisableRtc();
                        Console.WriteLine("Clock Disabled");
                        break;
                    case ConsoleKey.C:
                        Console.WriteLine("Press any key to stop");
                        while (!Console.KeyAvailable)
                        {
                            Console.WriteLine(_clock.GetDate());
                            Thread.Sleep(1000);
                        }

                        Console.Read();
                        break;
                    case ConsoleKey.D:
                        Console.WriteLine("Enter Year ");
                        int year = int.Parse(Console.ReadLine() ?? throw new InvalidOperationException());
                        Console.WriteLine("Enter Month ");
                        int month = int.Parse(Console.ReadLine() ?? throw new InvalidOperationException());
                        Console.WriteLine("Enter Day ");
                        int day = int.Parse(Console.ReadLine() ?? throw new InvalidOperationException());
                        Console.WriteLine("Enter Hour ");
                        int hour = int.Parse(Console.ReadLine() ?? throw new InvalidOperationException());
                        Console.WriteLine("Enter Minutes ");
                        int minutes = int.Parse(Console.ReadLine() ?? throw new InvalidOperationException());
                        Console.WriteLine("Enter Seconds ");
                        int seconds = int.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

                        DateTime newDateTime = new DateTime(year, month, day, hour, minutes, seconds);

                        _clock.SetDate(newDateTime);

                        Console.WriteLine("Clock set to {0}", newDateTime);

                        break;
                    case ConsoleKey.E:
                        _clock.ResetToFactoryDefaults();
                        Console.WriteLine("Clock reset to factory defaults");
                        break;
                    case ConsoleKey.F:
                        Console.WriteLine("Clock is ticking: {0}", _clock.IsRtcEnabled());
                        break;
                    case ConsoleKey.X:
                        _driver.Dispose();
                        Environment.Exit(0);
                        break;
                }

                AskForKey();
            }
        }

        static void Main()
        {
            Console.WriteLine("DS1307 I2C Test");
            Console.WriteLine("===============");

            Console.WriteLine("Opening Connection...");

            try
            {
                _driver = new I2cDriver(ProcessorPin.Gpio02, ProcessorPin.Gpio03);
                _i2CConnection = _driver.Connect(0x68);
                _clock = new Ds1307_I2C(_i2CConnection);
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
    }
}