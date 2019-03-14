namespace Dxw.Cache
{
    /// <summary>
    /// Interface to have one generic reference to cash instance.
    /// </summary>
    public interface ICleanableCache<TKey, TItem> : IExpiringCache<TKey, TItem>, ICleanable
    {
    }
}
