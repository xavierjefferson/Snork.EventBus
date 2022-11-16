namespace Snork.EventBus.Tests.Subscribers
{
    public class StringMessageSubscriber
    {
        public string LastStringMessage { get; set; }

        [Subscribe]
        public virtual void OnMessage(string message)
        {
            LastStringMessage = message;
        }
    }
}