using System;

namespace Snork.EventBus
{
    /// <summary>
    ///     This event is posted when an exception occurs inside a subscriber's event handling method.
    /// </summary>
    public sealed class SubscriberExceptionEvent
    {
        /// <summary>
        ///     The original event that could not be delivered to any subscriber.
        /// </summary>
        public object OriginalEvent { get; }

        /// <summary>
        ///     The subscriber that threw the Exception.
        /// </summary>
        public object OriginalSubscriber { get; }

        /// <summary>
        ///     The <see cref="EventBus"/> instance to with the original event was posted to.
        /// </summary>
        public EventBus EventBus { get; }

        /// <summary>
        ///     The <see cref="System.Exception"/> thrown by a subscriber.
        /// </summary>
        public Exception Exception { get; }

        public SubscriberExceptionEvent(EventBus eventBus, Exception exception, object originalEvent,
            object originalSubscriber)
        {
            this.EventBus = eventBus;
            this.Exception = exception;
            this.OriginalEvent = originalEvent;
            this.OriginalSubscriber = originalSubscriber;
        }
    }
}