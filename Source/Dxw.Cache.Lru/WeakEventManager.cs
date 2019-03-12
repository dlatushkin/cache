namespace Dxw.Cache.Lru
{
    using System;
    using System.Timers;
    using Dxw.Core.Timers;

    public class WeakEventManager
    {
        private readonly Timer timer;
        private readonly WeakReference<IElapsedListener> elapsedReference;

        private WeakEventManager(Timer timer, IElapsedListener elapsedListener)
        {
            this.timer = timer;
            this.elapsedReference = new WeakReference<IElapsedListener>(elapsedListener);
            this.timer.Elapsed += this.OnElapsed;
        }

        public static WeakEventManager Register(Timer timer, IElapsedListener elapsedListener) =>
            new WeakEventManager(timer, elapsedListener);

        private void OnElapsed(object sender, ElapsedEventArgs e)
        {
            if (this.elapsedReference.TryGetTarget(out var listener))
            {
                listener.Elapsed();
            }
            else
            {
                this.timer.Elapsed -= this.OnElapsed;
            }
        }
    }
}
