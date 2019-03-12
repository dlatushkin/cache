using System;

namespace Dxw.Core.Times
{
    public interface ITimeSource
    {
        DateTime GetNow();
    }
}
