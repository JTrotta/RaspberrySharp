using RaspberrySharp.IO.GeneralPurpose;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BatteryGauge.GPIO
{
    public static class GpioGlobalConnection
    {
        #region Fields

        private static GpioConnection _gpioConnectionGlobalPin;
        //Create the settings for the connection
        private static readonly GpioConnectionSettings _settings;
        private static CancellationTokenSource _cancellationTokenSource;
        private static ConcurrentDictionary<ProcessorPin, TimeSpan> _blinkPins;
        private static Task BlinkTask;
        #endregion

        #region Properties

        /// <summary>
        /// Gets global  GpioConnection
        /// </summary>
        public static GpioConnection GlobalConnectionPin => _gpioConnectionGlobalPin;

        #endregion

        #region Ctrs
        static GpioGlobalConnection()
        {
        //    if (!CommonHelper.IsBoard)
        //        return;
            _settings = new GpioConnectionSettings
            {
                //Interval between pin checks. This is *really* important - higher values lead to missed values/borking. Lower 
                //values are apparently possible, but may have 'severe' performance impact. Further testing needed.
                PollInterval = TimeSpan.FromMilliseconds(500)
            };
            _gpioConnectionGlobalPin = new GpioConnection(_settings);
            _blinkPins = new ConcurrentDictionary<ProcessorPin, TimeSpan>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Stop and close Gpio Connection
        /// </summary>
        public static void Stop()
        {
            _cancellationTokenSource.Cancel();
            //if (CommonHelper.IsBoard)
                _gpioConnectionGlobalPin.Close();
        }

        private static void ConnectionGlobalPinAdd(PinConfiguration pc)
        {
            //if (CommonHelper.IsBoard)
                _gpioConnectionGlobalPin.Add(pc);
        }

        /// <summary>
        /// Add a gpio to the global connection if not already added setting direction
        /// </summary>
        /// <param name="processorPin"></param>
        /// <param name="output"></param>
        public static void Add(ProcessorPin processorPin, bool output)
        {
            //if (!CommonHelper.IsBoard)
            //    return;

            if (output)
            {
                OutputPinConfiguration opc = processorPin.Output();
                ConnectionGlobalPinAdd(opc);
            }
            else
            {
                InputPinConfiguration ipc = processorPin.Input();
                ConnectionGlobalPinAdd(ipc);
            }
        }

        public static void Remove(ProcessorPin processorPin)
        {
            //if (CommonHelper.IsBoard)
                _gpioConnectionGlobalPin.Remove(processorPin);
        }

        public static bool Read(ProcessorPin processorPin)
        {
            var pin = GlobalConnectionPin.Pins[processorPin];
            if (pin != null)
            {
                return pin.Enabled;
            }
            return false;
        }


        /// <summary>
        /// Enable or not the pin
        /// </summary>
        /// <param name="processorPin"></param>
        /// <param name="enable"></param>
        /// <returns></returns>
        public static bool Enable(ProcessorPin processorPin, bool enable)
        {
            //if (!CommonHelper.IsBoard)
            //    return false;
            var pin = GlobalConnectionPin.Pins[processorPin];
            if (pin != null)
            {
                pin.Enabled = enable;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Toogle a pin switch from On to off or Off to on. One time
        /// </summary>
        /// <param name="processorPin"></param>
        public static void Toogle(ProcessorPin processorPin)
        {
            GlobalConnectionPin.Toggle(processorPin);
        }

        /// <summary>
        /// Blink a pin with a given period. Blink is a sequence of true/false, on/off 
        /// of the gpio connected to that pin.
        /// </summary>
        /// <param name="processorPin">The pin to blink</param>
        /// <param name="blinkPeriod">The blinking period in millisecond</param>
        public static void Blink(ProcessorPin processorPin, int blinkPeriod)
        {
            Add(processorPin, true);
            TimeSpan blinkDuration = new TimeSpan(0, 0, 0, 0, blinkPeriod);
            _blinkPins.TryAdd(processorPin, blinkDuration);

            if (BlinkTask == null || BlinkTask.IsCompleted)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                BlinkTask = Task.Factory.StartNew(() => StartTask(_cancellationTokenSource.Token), _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        private static async Task StartTask(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _blinkPins.Count > 0)
            {
                foreach (var pin in _blinkPins)
                {
                    GlobalConnectionPin.Blink(pin.Key, pin.Value);
                }
                await Task.Delay(1, token);
            }
        }

        #endregion
    }
}
