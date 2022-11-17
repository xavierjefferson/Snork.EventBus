namespace Snork.EventBus.Tests.Subscribers
{
    public class SubscriberExceptionEventTracker : OuterTestSubscriberBase
    {
        public SubscriberExceptionEventTracker(TestBase outerTest) : base(
            outerTest)
        {
        }

        [Subscribe]
        public virtual void OnEvent(SubscriberExceptionEvent @event)
        {
            OuterTest.TrackEvent(@event);
        }
    }
}