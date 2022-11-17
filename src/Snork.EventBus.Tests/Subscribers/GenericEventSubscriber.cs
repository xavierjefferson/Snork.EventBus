namespace Snork.EventBus.Tests.Subscribers
{
    public class GenericEventSubscriber<T> : OuterTestSubscriberBase
    {
        public GenericEventSubscriber(TestBase outerTest) : base(outerTest)
        {
        }

        [Subscribe]
        public void OnGenericEvent(T @event)
        {
            OuterTest.TrackEvent(@event);
        }
    }
}