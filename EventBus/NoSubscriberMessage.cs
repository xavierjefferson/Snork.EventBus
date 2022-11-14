namespace Snork.EventBus
{
    /// <summary>
    ///     This Event is posted by EventBus when no subscriber is found for a posted message.
    /// </summary>
    public sealed class NoSubscriberMessage
    {
        /// <summary>
        ///     The <see cref="EventBus"/> instance to which the original message was posted to.
        /// </summary>
        public EventBus EventBus { get; }

        /// <summary>
        ///     The original message that could not be delivered to any subscriber.
        /// </summary>
        public object OriginalMessage { get; }

        public NoSubscriberMessage(EventBus eventBus, object originalMessage)
        {
            this.EventBus = eventBus;
            this.OriginalMessage = originalMessage;
        }
    }
}