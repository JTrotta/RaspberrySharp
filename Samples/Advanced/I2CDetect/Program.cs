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
using System.Collections.Generic;
using RaspberrySharp.IO.InterIntegratedCircuit;

#endregion

namespace I2CDetect
{
    /// <summary>
    /// Sample for reading the I2C Bus
    /// </summary>
    public class Program
    {
        private static int rows = 8;
        private static int cols = 16;
        private static byte[,] nDevices = new byte[rows, cols];
        public static I2cDetect _I2CDetect;

        static void Main(string[] args)
        {
            Console.WriteLine("I2C Detect");
            Console.WriteLine("===============");

            _I2CDetect = new I2cDetect();
            var list = _I2CDetect.Detect();
            //List<byte> list = new List<byte>();
            //list.Add(15);
            //list.Add(33);
            //list.Add(3);
            foreach (var i2c in list)
            {
                int r = i2c / 16;
                int c = i2c % 16;                
                nDevices[r, c] = 1;
            }
            Console.WriteLine("     0  1  2  3  4  5  6  7  8  9  a  b  c  d  e  f");


            for (int i = 0; i < rows; i++)
            {
                PrintRow(i);
            }

            Console.ReadKey();
        }

        static void PrintRow(int rowId)
        {
            string row = string.Format("{0:00}: ", rowId);
            for (int i = 0; i < cols; i++)
            {
                if ((rowId == 0 && i <3) || (rowId == 7 && i > 7))
                    row += "   ";
                else
                if (nDevices[rowId, i] == 0)
                    row += string.Format("-- ");
                else
                    row += string.Format("{0}{1:X} ", rowId, i);
            }
            
            Console.WriteLine(row);
        }
    }
}