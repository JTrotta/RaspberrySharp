// **************************************************************
// Project     : Blink
// Filename    : Program.cs
// Details     : https://github.com/JTrotta/RaspberrySharp
// **************************************************************
// Github Wiki : https://github.com/JTrotta/RaspberrySharp/wiki
// DataSheet   : 
// **************************************************************
// Links       : 
// **************************************************************
// Notes       : 

#region Usings

using System;
using System.Linq;
using System.Threading;
using RaspberrySharp.IO.GeneralPurpose;

#endregion

namespace Blink
{
    /// <summary>
    /// Sample for blink an LED on a GPIO Port
    /// </summary>
    public class Program
    {
        private static GpioConnection conPin;

        static void Main()
        {
            conPin = new GpioConnection(ProcessorPin.Gpio18.Output());

            Console.WriteLine("Blink Sample");
            Console.WriteLine("===============");
            Console.WriteLine($"GPIO : {conPin.Pins.First().Configuration.Pin}");
            Console.WriteLine(RaspberrySharp.System.Board.Current.Processor);

            while (true)
            {
                // Blink the LED
                Console.WriteLine("LED On");
                conPin.Pins.First().Enabled = true;
                Thread.Sleep(1000);

                Console.WriteLine("LED Off");
                conPin.Pins.First().Enabled = false;
                Thread.Sleep(1000);

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.X:
                            Console.WriteLine("Blink End ...");
                            Environment.Exit(0);
                            break;
                    }
                }
            }
        }
    }
}