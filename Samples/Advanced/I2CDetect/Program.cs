// **************************************************************
// Project     : I2CDetect
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
using RaspberrySharp.IO.InterIntegratedCircuit;

#endregion

namespace I2CDetect
{
    /// <summary>
    /// Sample for reading the I2C Bus
    /// </summary>
    public class Program
    {
        public static I2cDetect _I2CDetect;

        static void Main(string[] args)
        {
            Console.WriteLine("I2C Detect");
            Console.WriteLine("===============");

            _I2CDetect = new I2cDetect();
            var list = _I2CDetect.Detect();
            foreach (var item in list) Console.WriteLine(item);
        }
    }
}