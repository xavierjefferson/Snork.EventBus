namespace Snork.EventBus.Tests.Subscribers
{
    public class RemoveStickySubscriber : OuterTestSubscriberBase
    {
        public RemoveStickySubscriber(TestBase outerTest) : base(outerTest)
        {
        }

        [Subscribe(sticky: true)]
        public virtual void OnEvent(string @event)
        {
            OuterTest.EventBus.RemoveStickyEvent(@event);
        }
    }
}