using Demo.Utilities;
using RaspberrySharp.IO.OneWire;
using RaspberrySharp.System.Timers;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using UnitsNet;

namespace Demo.Modules
{
    public class DS18B20EventArgs : EventArgs
    {
        public Temperature T { get; set; }

        public Temperature PreviuosT { get; set; }
    }

    /// <summary>
    /// Service for temperature sensor DS18B20, using OneWire BUS
    /// </summary>
    public partial class DS18B20Service : OneWireDriver
    {
        #region Fields
        public delegate void DelegateTemperature(DS18B20EventArgs args);
        public event DelegateTemperature OnTemperatureChanged;
        #endregion

        #region Ctrs
        public DS18B20Service()
            : base("28")
        {
            CommonHelper.Logger.Info("DS18B20Service");
        }
        #endregion

        #region Methods
        public Temperature GetTemperature()
        {
            if (!File.Exists(DeviceFile))
            {
                Random r = new Random(DateTime.Now.Second);
                return Temperature.FromDegreesCelsius(r.NextDouble() * 100);
            }

            var lines = File.ReadAllLines(DeviceFile);
            while (!lines[0].Trim().EndsWith("YES"))
            {
                Timer.Sleep(2);
                lines = File.ReadAllLines(DeviceFile);
            }

            var equalsPos = lines[1].IndexOf("t=", StringComparison.InvariantCultureIgnoreCase);
            if (equalsPos == -1)
                CommonHelper.Logger.Warn("Unable to read temperature");

            var temperatureString = lines[1].Substring(equalsPos + 2);

            if (double.TryParse(temperatureString, NumberStyles.Any, CultureInfo.InvariantCulture, out double temp))
                return Temperature.FromDegreesCelsius(temp / 1000.0);
            else
                return Temperature.FromDegreesCelsius(0);
        }

        public override void Start()
        {
            this.DoWork = true;
            //var s = new ThreadStart(ReadTemperatureEveryXSecond);
            //var work = new Thread(s);
            //work.Start();
            ReadTemperatureEveryXSecond();

            CommonHelper.Logger.Info("DS18B20: Started");
        }

        public override void Stop()
        {
            base.Stop();
            CommonHelper.Logger.Info("DS18B20: Stopped");
        }


        private async void ReadTemperatureEveryXSecond()
        {
            DS18B20EventArgs eventArgs = new DS18B20EventArgs();

            while (this.DoWork)
            {
                eventArgs.T = GetTemperature();
                if (eventArgs.T.CompareTo(eventArgs.PreviuosT) != 0)
                {
                    if (OnTemperatureChanged != null)
                        OnTemperatureChanged(eventArgs);
                }
                eventArgs.PreviuosT = eventArgs.T;
                await Task.Delay(5000).ConfigureAwait(false);
            }
        }
        #endregion
    }
}
