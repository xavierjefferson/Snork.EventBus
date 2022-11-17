namespace Snork.EventBus.Interfaces
{
    /// <summary>
    ///     Posts events.
    /// </summary>
    public interface IPoster
    {
        /// <summary>
        ///     Enqueue a event to be posted for a particular subscription.
        /// </summary>
        /// <param name="subscription">Subscription which will receive the eventl</param>
        /// <param name="event"> Event that will be posted to subscribers.</param>
        void Enqueue(Subscription subscription, object @event);
    }
}