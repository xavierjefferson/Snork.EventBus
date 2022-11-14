namespace Snork.EventBus
{
    /// <summary>
    ///     Interface to the "main" thread, which can be whatever you like. Typically on Android, Android's main thread is
    ///     used.
    /// </summary>
    public interface IMainThreadSupport
    {
        bool IsMainThread();

        IPoster CreatePoster(EventBus eventBus);
    }
}