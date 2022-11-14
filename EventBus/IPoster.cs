namespace Snork.EventBus
{
    /// <summary>
    ///     Posts events.
    ///     @author William Ferguson
    /// </summary>
    public interface IPoster
    {
        /// <summary>
        ///     Enqueue an message to be posted for a particular subscription.
        ///     @param subscription Subscription which will receive the message.
        ///     @param message        Event that will be posted to subscribers.
        /// </summary>
        void Enqueue(Subscription subscription, object message);
    }
}