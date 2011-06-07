using System;
using System.Threading;
using System.Windows;

namespace NotationalFerocity.Utilities
{
    public class TimeoutDeferredAction
    {
        private Timer _timer;

        private readonly int _maximumDelayTime;
        private DateTime? _startTime;

        private readonly Action _action;

        public bool IsReset { get; private set; }

        private void InstantiateTimer()
        {
            IsReset = false;

            _startTime = null;

            _timer = new Timer(delegate
            {
                IsReset = true;

                Application.Current.Dispatcher.Invoke(_action);
            });
        }

        public TimeoutDeferredAction(Action action, int maximumDelayTime)
        {
            _maximumDelayTime = maximumDelayTime;
            _action = action;

            InstantiateTimer();
        }

        /// <summary>
        /// Defers performing the action until after time elapses. 
        /// Repeated calls will reschedule the action
        /// if it has not already been performed, up to the maximum 
        /// cumulative delay specified.
        /// </summary>
        /// <param name="delay">
        /// The amount of time to wait before performing the action.
        /// </param>
        public void Defer(int delay)
        {
            if (IsReset)
            {
                InstantiateTimer();
            }

            if (!_startTime.HasValue)
            {
                _startTime = DateTime.Now;
            }

            var elapsed = DateTime.Now - _startTime.Value;
            
            // Fire action when time elapses (with no subsequent calls).
            _timer.Change(elapsed.TotalMilliseconds < _maximumDelayTime ? delay : 0, -1);
        }
    }
}