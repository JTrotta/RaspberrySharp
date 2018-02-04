using RaspberrySharp.System;
using System;

namespace RaspberrySharp.IO.GeneralPurpose
{
    /// <summary>
    /// Represents settings for <see cref="GpioConnection"/>.
    /// </summary>
    public class GpioConnectionSettings
    {
        #region Constants

        /// <summary>
        /// The default poll interval, in milliseconds.
        /// </summary>
        private const decimal _defaultPollInterval = 50.0m;

        #endregion

        #region Fields

        private IGpioConnectionDriver driver;

        private TimeSpan blinkDuration;
        private TimeSpan pollInterval;

        #endregion

        #region Instance Management

        /// <summary>
        /// Initializes a new instance of the <see cref="GpioConnectionSettings"/> class.
        /// </summary>
        public GpioConnectionSettings()
        {
            Driver = DefaultDriver;
            BlinkDuration = DefaultBlinkDuration;
            PollInterval = TimeSpan.FromMilliseconds((double)_defaultPollInterval);
            Opened = true;
        }

        #endregion

        #region Constants

        /// <summary>
        /// Gets the default blink duration.
        /// </summary>
        public static readonly TimeSpan DefaultBlinkDuration = TimeSpan.FromMilliseconds(250);

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="GpioConnectionSettings"/> is opened on initialization.
        /// </summary>
        /// <value>
        ///   <c>true</c> if opened on initialization; otherwise, <c>false</c>.
        /// </value>
        public bool Opened { get; set; }

        /// <summary>
        /// Gets or sets the duration of the blink.
        /// </summary>
        /// <value>
        /// The duration of the blink, in milliseconds.
        /// </value>
        public TimeSpan BlinkDuration
        {
            get { return blinkDuration; }
            set { blinkDuration = value >= TimeSpan.Zero ? value : DefaultBlinkDuration; }
        }

        /// <summary>
        /// Gets or sets the driver.
        /// </summary>
        /// <value>
        /// The driver.
        /// </value>
        public IGpioConnectionDriver Driver
        {
            get { return driver; }
            set { driver = value ?? DefaultDriver; }
        }

        /// <summary>
        /// Gets or sets the poll interval.
        /// </summary>
        /// <value>
        /// The poll interval.
        /// </value>
        public TimeSpan PollInterval
        {
            get { return pollInterval; }
            set { pollInterval = value >= TimeSpan.Zero ? value : TimeSpan.FromMilliseconds((double)_defaultPollInterval); }
        }

        ///// <summary>
        ///// Gets the default poll interval.
        ///// </summary>
        //public static TimeSpan DefaultPollInterval
        //{
        //    get
        //    {                
        //        return TimeSpan.FromMilliseconds((double)GpioConnectionConfigurationSection.DefaultPollInterval);
        //    }
        //}

        /// <summary>
        /// Gets the board connector pinout.
        /// </summary>
        /// <value>
        /// The board connector pinout.
        /// </value>
        public static ConnectorPinout ConnectorPinout
        {
            get
            {
                return Board.Current.ConnectorPinout;
            }
        }

        /// <summary>
        /// Gets the default driver.
        /// </summary>
        public static IGpioConnectionDriver DefaultDriver
        {
            get
            {
                return GetBestDriver(Board.Current.IsRaspberryPi ? GpioConnectionDriverCapabilities.None : GpioConnectionDriverCapabilities.CanWorkOnThirdPartyComputers);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the best driver for the specified capabilities.
        /// </summary>
        /// <param name="capabilities">The capabilities.</param>
        /// <returns>The best driver, if found; otherwise, <c>null</c>.</returns>
        public static IGpioConnectionDriver GetBestDriver(GpioConnectionDriverCapabilities capabilities)
        {
            if ((GpioConnectionDriver.GetCapabilities() & capabilities) == capabilities)
                return new GpioConnectionDriver();
            if ((MemoryGpioConnectionDriver.GetCapabilities() & capabilities) == capabilities)
                return new MemoryGpioConnectionDriver();
            if ((FileGpioConnectionDriver.GetCapabilities() & capabilities) == capabilities)
                return new FileGpioConnectionDriver();

            return null;
        }

        #endregion
    }
}
