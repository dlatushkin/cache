namespace Dxw.Core.Times
{
    using System;

    public class DateTimeSource : ITimeSource
    {
        public DateTime GetNow() => DateTime.UtcNow;
    }
}
