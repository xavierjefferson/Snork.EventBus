namespace Snork.EventBus
{
    /// <summary>
    ///     This event is posted when no subscriber is found for a posted event.
    /// </summary>
    public sealed class NoSubscriberEvent
    {
        /// <summary>
        ///     The <see cref="EventBus"/> instance to which the original event was posted to.
        /// </summary>
        public EventBus EventBus { get; }

        /// <summary>
        ///     The original event that could not be delivered to any subscriber.
        /// </summary>
        public object OriginalEvent { get; }

        public NoSubscriberEvent(EventBus eventBus, object originalEvent)
        {
            this.EventBus = eventBus;
            this.OriginalEvent = originalEvent;
        }
    }
}