using Dxw.Core.Timers;
using System;
using System.Timers;

namespace Dxw.Cache.Lru
{
    public class WeakEventManager
    {
        private readonly Timer _timer;
        private readonly WeakReference<IElapsedListener> _elapsedReference;

        public static WeakEventManager Register(Timer timer, IElapsedListener elapsedListener) =>
            new WeakEventManager(timer, elapsedListener);

        private WeakEventManager(Timer timer, IElapsedListener elapsedListener)
        {
            _timer = timer;
            _elapsedReference = new WeakReference<IElapsedListener>(elapsedListener);
            _timer.Elapsed += OnElapsed;
        }

        private void OnElapsed(object sender, ElapsedEventArgs e)
        {
            if (_elapsedReference.TryGetTarget(out var listener))
            {
                listener.Elapsed();
            }
            else
            {
                _timer.Elapsed -= OnElapsed;
            }
        }
    }
}
