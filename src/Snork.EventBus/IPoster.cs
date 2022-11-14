namespace Snork.EventBus
{
    /// <summary>
    ///     Posts messages.
    /// </summary>
    public interface IPoster
    {
        /// <summary>
        ///     Enqueue a message to be posted for a particular subscription.
        /// </summary>
        /// <param name="subscription">Subscription which will receive the messagel</param>
        /// <param name="message"> Event that will be posted to subscribers.</param>
        void Enqueue(Subscription subscription, object message);
    }
}