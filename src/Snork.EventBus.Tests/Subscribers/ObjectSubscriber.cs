namespace Snork.EventBus.Tests.Subscribers
{
    public class ObjectSubscriber
    {
        [Subscribe]
        public virtual void OnMessage(object message)
        {
        }
    }
}