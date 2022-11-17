namespace Snork.EventBus.Interfaces
{
    /// <summary>
    ///     Interface to the "main" thread, which can be whatever you like. 
    /// </summary>
    public interface IMainThreadSupport
    {
        bool IsMainThread();

        IPoster CreatePoster(EventBus eventBus);
    }
}