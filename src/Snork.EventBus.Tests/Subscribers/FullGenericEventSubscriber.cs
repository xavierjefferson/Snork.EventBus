namespace Snork.EventBus.Tests.Subscribers
{
    public class FullGenericEventSubscriber<T> : OuterTestSubscriberBase
    {
        public FullGenericEventSubscriber(TestBase outerTest) : base(outerTest)
        {
        }

        [Subscribe]
        public void OnGenericEvent(T @event)
        {
            OuterTest.TrackEvent(@event);
        }
    }
}