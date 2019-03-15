namespace Dxw.Cache.Lru
{
    using System;
    using System.Timers;
    using Dxw.Core.Timers;

    /// <summary>
    /// WeakReference implementation to weakly bind Timer to cache handler.
    /// This allows to avoid IDispose implementation in the <see cref="ActiveLruCash"/>.
    /// </summary>
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
            { // if target object is still alive call its event handler
                try
                {
                    listener.Elapsed();
                }
                finally
                {
                    this.timer.Start();
                }
            }
            else
            { // if not unsubscribe this wrapper
                this.timer.Elapsed -= this.OnElapsed;
                this.timer.Stop();
            }
        }
    }
}
