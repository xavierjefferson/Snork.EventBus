namespace Snork.EventBus.Tests.Subscribers
{
    public class CharSequenceSubscriber
    {
        [Subscribe]
        public virtual void OnMessage(string message)
        {
        }
    }
}