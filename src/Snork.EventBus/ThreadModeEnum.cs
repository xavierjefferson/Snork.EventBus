namespace Snork.EventBus
{
    /// <summary>
    ///     Each subscriber method has a thread mode, which determines in which thread the method is to be called by EventBus.
    ///     EventBus takes care of threading independently from the posting thread. See <see cref="EventBus.Register"/>.
    /// </summary>
    public enum ThreadModeEnum
    {
        /// <summary>
        ///     Subscriber will be called directly in the same thread, which is posting the message. This is the default. Event
        ///     delivery
        ///     implies the least overhead because it avoids thread switching completely. Thus this is the recommended mode for
        ///     simple tasks that are known to complete in a very short time without requiring the main thread. Event handlers
        ///     using this mode must return quickly to avoid blocking the posting thread, which may be the main thread.
        /// </summary>
        Posting,

        /// <summary>
        ///     On Android, subscriber will be called in Android's main thread (UI thread). If the posting thread is
        ///     the main thread, subscriber methods will be called directly, blocking the posting thread. Otherwise the message
        ///     is queued for delivery (non-blocking). Subscribers using this mode must return quickly to avoid blocking the main
        ///     thread.
        ///     If not on Android, behaves the same as <see cref="Posting"/>.
        /// </summary>
        Main,

        /// <summary>
        ///     On Android, subscriber will be called in Android's main thread (UI thread). Different from <see cref="Main"/>,
        ///     the message will always be queued for delivery. This ensures that the post call is non-blocking.
        /// </summary>
        MainOrdered,

        /// <summary>
        ///     On Android, subscriber will be called in a background thread. If posting thread is not the main thread, subscriber
        ///     methods
        ///     will be called directly in the posting thread. If the posting thread is the main thread, EventBus uses a single
        ///     background thread, that will deliver all its messages sequentially. Subscribers using this mode should try to
        ///     return quickly to avoid blocking the background thread. If not on Android, always uses a background thread.
        /// </summary>
        Background,

        /// <summary>
        ///     Subscriber will be called in a separate thread. This is always independent from the posting thread and the
        ///     main thread. Posting messages never wait for subscriber methods using this mode. Subscriber methods should
        ///     use this mode if their execution might take some time, e.g. for network access. Avoid triggering a large number
        ///     of long running asynchronous subscriber methods at the same time to limit the number of concurrent threads.
        ///     EventBus
        ///     uses a thread pool to efficiently reuse threads from completed asynchronous subscriber notifications.
        /// </summary>
        Async
    }
}