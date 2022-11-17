namespace Snork.EventBus.Tests.Subscribers
{
    public class DummySubscriber
    {
        [Subscribe]
        public virtual void OnEvent(string dummy)
        {
        }
    }
}