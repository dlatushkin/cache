using Dxw.Cache.Lru;
using Dxw.Core.Times;
using System;
using System.Threading.Tasks;

namespace Dxw.Cache.ConsoleTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IExpiringCache<string, string> cache =
                new ActiveLruCache<string, string>(
                    new DateTimeSource(),
                    purgeInterval: TimeSpan.FromMilliseconds(500));

            GC.Collect();

            await Task.Delay(2000);

            Console.WriteLine(cache);

            GC.Collect();

            await Task.Delay(2000);

            GC.Collect();

            Console.WriteLine("Hello World!"); Console.ReadKey();
        }
    }
}
