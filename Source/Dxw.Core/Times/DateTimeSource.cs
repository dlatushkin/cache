using System;

namespace Dxw.Core.Times
{
    public class DateTimeSource : ITimeSource
    {
        public DateTime GetNow() => DateTime.UtcNow;
    }
}
