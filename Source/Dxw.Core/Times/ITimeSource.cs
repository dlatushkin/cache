namespace Dxw.Core.Times
{
    using System;

    public interface ITimeSource
    {
        DateTime GetNow();
    }
}
