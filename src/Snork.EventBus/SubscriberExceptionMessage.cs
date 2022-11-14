using System;

namespace Snork.EventBus
{
    /// <summary>
    ///     This Event is posted by EventBus when an exception occurs inside a subscriber's message handling method.
    ///     @author Markus
    /// </summary>
    public sealed class SubscriberExceptionMessage
    {
        /// <summary>
        ///     The original message that could not be delivered to any subscriber.
        /// </summary>
        public object OriginalMessage { get; }

        /// <summary>
        ///     The subscriber that threw the Exception.
        /// </summary>
        public object OriginalSubscriber { get; }

        /// <summary>
        ///     The <see cref="EventBus"/> instance to with the original message was posted to.
        /// </summary>
        public EventBus EventBus { get; }

        /// <summary>
        ///     The Exception thrown by a subscriber.
        /// </summary>
        public Exception Exception { get; }

        public SubscriberExceptionMessage(EventBus eventBus, Exception exception, object originalMessage,
            object originalSubscriber)
        {
            this.EventBus = eventBus;
            this.Exception = exception;
            this.OriginalMessage = originalMessage;
            this.OriginalSubscriber = originalSubscriber;
        }
    }
}