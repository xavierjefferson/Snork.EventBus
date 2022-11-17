using Snork.EventBus.Interfaces;

namespace Snork.EventBus
{
    /// <summary>
    ///     Each subscriber method has a thread mode, which determines in which thread the method is to be called.
    ///     Theading is handled independently of the posting thread. See <see cref="EventBus.Register"/>.
    /// </summary>
    public enum ThreadModeEnum
    {
        /// <summary>
        ///     This is the default.  Subscriber will be called directly in the same thread, which is posting the event.  Event
        ///     delivery
        ///     implies the least overhead because it avoids thread switching completely. Thus this is the recommended mode for
        ///     simple tasks that are known to complete in a very short time without requiring the main thread. Event handlers
        ///     using this mode must return quickly to avoid blocking the posting thread, which may be the main thread.
        /// </summary>
        Posting,

        /// <summary>
        ///     When an implementation of <see cref="Interfaces.IMainThreadSupport"/> is supplied via configuration, subscriber will be called in the thread
        ///     referenced by such "main" <see cref="Interfaces.IMainThreadSupport"/> instance.  If the posting thread is
        ///     the main thread, subscriber methods will be called directly, blocking the posting thread. Otherwise the event
        ///     is queued for delivery (non-blocking). Subscribers using this mode must return quickly to avoid blocking the main
        ///     thread.
        ///     If no <see cref="Interfaces.IMainThreadSupport"/> is configured, behaves the same as <see cref="Posting"/>.
        /// </summary>
        Main,

        /// <summary>
        ///     When an implementation of <see cref="Interfaces.IMainThreadSupport"/> is supplied via configuration,
        ///     subscriber will be called in the main thread per
        ///     <see cref="IMainThreadSupport.IsMainThread()"/>.   Different from <see cref="Main"/>,
        ///     the event will always be queued for delivery. This ensures that the post call is non-blocking.
        /// </summary>
        MainOrdered,

        /// <summary>
        ///     When an implementation of <see cref="Interfaces.IMainThreadSupport"/> is supplied via configuration,
        ///     subscriber methods will be called on a background thread.  If posting thread is not the "main" thread per
        ///     <see cref="IMainThreadSupport.IsMainThread()"/>, subscriber
        ///     methods  will be called directly in the posting thread. If the posting thread is the "main" thread, all events
        ///     will be delivered to a single background thread, sequentially. Subscribers using this mode should try to
        ///     return quickly to avoid blocking the background thread. If not on a "main" thread, always uses a background thread.
        /// </summary>
        Background,

        /// <summary>
        ///     Subscriber will be called in a separate thread. This is always independent from the posting thread and the
        ///     main thread. Posting events never wait for subscriber methods using this mode. Subscriber methods should
        ///     use this mode if their execution might take some time, e.g. for network access. Avoid triggering a large number
        ///     of long-running asynchronous subscriber methods at the same time to limit the number of concurrent threads.
        ///     A thread pool is used to efficiently reuse threads from completed asynchronous subscriber notifications.
        /// </summary>
        Async
    }
}