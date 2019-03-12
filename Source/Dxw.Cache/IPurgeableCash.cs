namespace Dxw.Cache
{
    /// <summary>
    /// Interface to have one generic reference to cash instance.
    /// </summary>
    public interface IPurgeableCash<TKey, TItem> : IExpiringCache<TKey, TItem>, IPurgeable {}
}
