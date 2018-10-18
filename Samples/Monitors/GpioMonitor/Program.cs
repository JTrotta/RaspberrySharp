using RaspberrySharp.IO.GeneralPurpose;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GpioMonitor
{
    class Program
    {
        #region Fields
        static GpioConnection _conPin;
        static ProcessorPin testPin = ProcessorPin.Gpio33;
        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine("Gpio Monitor Sample");
            Console.WriteLine("Press X to close");
            Console.WriteLine("===============");

            Start();

            //var pin = (ProcessorPins)((ulong)1 << (int)testPin);
            //long pp = (long)ProcessorPins.Pin33;
            //ulong pins2 = (ulong)pin >> 32;
            //Console.WriteLine(pin);

            
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.X:
                        Console.WriteLine("Exiting...");
                        _conPin.Close();
                        Environment.Exit(0);
                        break;
                }
            }
        }

        private static void Start()
        {
            var _settings = new GpioConnectionSettings();
            _settings.PollInterval = TimeSpan.FromSeconds(1);//.FromMilliseconds(50);
            _conPin = new GpioConnection(_settings);
            _conPin.Add(testPin.Input());
            //_conPin.Toggle(testPin);
            _conPin.PinStatusChanged += _conPin_PinStatusChanged;
        }
        private static void _conPin_PinStatusChanged(object sender, PinStatusEventArgs e)
        {
            Console.WriteLine("Gpio{0}", e.Enabled);
        }
    }
}
