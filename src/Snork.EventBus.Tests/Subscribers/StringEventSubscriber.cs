namespace Snork.EventBus.Tests.Subscribers
{
    public class StringEventSubscriber
    {
        public string LastStringEvent { get; set; }

        [Subscribe]
        public virtual void OnEvent(string @event)
        {
            LastStringEvent = @event;
        }
    }
}