using BaseSystem  = System;


namespace RaspberrySharp.System.Timers
{
    /// <summary>
    /// Represents a timer.
    /// </summary>
    public class StandardTimer : ITimer
    {
        #region Fields

        private BaseSystem.TimeSpan interval;
        private BaseSystem.Action action;

        private bool isStarted;
        private BaseSystem.Threading.Timer timer;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the interval, in milliseconds.
        /// </summary>
        /// <value>
        /// The interval, in milliseconds.
        /// </value>
        public BaseSystem.TimeSpan Interval
        {
            get { return interval; }
            set
            {
                interval = value;
                if (isStarted)
                    Start(BaseSystem.TimeSpan.Zero);
            }
        }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public BaseSystem.Action Action
        {
            get { return action; }
            set
            {
                if (value == null)
                    Stop();

                action = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <param name="startDelay">The delay before the first occurence, in milliseconds.</param>
        public void Start(BaseSystem.TimeSpan startDelay)
        {
            lock (this)
            {
                if (!isStarted && interval.TotalMilliseconds >= 1)
                {
                    isStarted = true;
                    timer = new BaseSystem.Threading.Timer(OnElapsed, null, startDelay, interval);
                }
                else
                    Stop();
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                if (isStarted)
                {
                    isStarted = false;
                    timer.Dispose();
                    timer = null;
                }
            }
        }

        #endregion

        #region Private Helpers

        private void NoOp() { }

        private void OnElapsed(object state)
        {
            (Action ?? NoOp)();
        }

        #endregion
    }
}
