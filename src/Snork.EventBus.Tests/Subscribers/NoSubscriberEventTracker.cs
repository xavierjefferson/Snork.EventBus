namespace Snork.EventBus.Tests.Subscribers
{
    public class NoSubscriberEventTracker : OuterTestSubscriberBase
    {
        public NoSubscriberEventTracker(TestBase outerTest) : base(outerTest)
        {
        }

        [Subscribe]
        public virtual void OnEvent(NoSubscriberEvent @event)
        {
            OuterTest.TrackEvent(@event);
        }
    }
}