namespace Scoundrel.UI.Mobile
{
    /// <summary>
    /// Interface for objects that can be managed by an object pool.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Called when the object is retrieved from the pool.
        /// Reset state for reuse.
        /// </summary>
        void OnPoolGet();

        /// <summary>
        /// Called when the object is returned to the pool.
        /// Clean up references and prepare for storage.
        /// </summary>
        void OnPoolReturn();
    }
}
