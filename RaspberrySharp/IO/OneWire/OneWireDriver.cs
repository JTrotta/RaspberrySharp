using System;
using System.Linq;
using System.IO;

namespace RaspberrySharp.IO.OneWire
{
    /// <summary>
    /// Abstract class inherited by ALL OneWire services.
    /// </summary>
    public abstract class OneWireDriver : IDisposable
    {
        #region Fields

        private const string _baseDir = @"/sys/bus/w1/devices/";
        private readonly string _deviceFolder;
        public bool DoWork;

        #endregion

        public string DeviceFile { get { return _deviceFolder + @"/w1_slave"; } }

        /// <summary>
        /// Abstract class to be inheroted by all OneWire Sensors
        /// </summary>
        /// <param name="onewireId">Sensor Id (i.e. DS18B20 = 28)</param>
        /// <param name="deviceIndex">OneWire may connect multiple sensor with same Id, each one has index: 0,1 etc.</param>
        public OneWireDriver(string onewireId, int deviceIndex = 0)
        {
            if (Directory.Exists(_baseDir))
            {
                var deviceFolders = Directory.GetDirectories(_baseDir, onewireId + "*").ToList();
                var deviceCount = deviceFolders.Count();
                if (deviceCount == 0)
                    throw new Exception(string.Format("No device found on OneWire with ID: {0}", onewireId));
                else
                    _deviceFolder = deviceFolders[deviceIndex];
            }
            else
                throw new Exception("OneWire folder does not exists.");
        }

        public abstract void Start();

        public virtual void Stop()
        {
            DoWork = false;
        }

        public virtual void Dispose()
        {
            if (DoWork)
                Stop();
        }

    }
}
