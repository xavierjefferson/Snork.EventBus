namespace Snork.EventBus.Tests.Subscribers
{
    public class NoSubscriberMessageTracker : OuterTestSubscriberBase
    {
        public NoSubscriberMessageTracker(TestBase outerTest) : base(outerTest)
        {
        }

        [Subscribe]
        public virtual void OnMessage(NoSubscriberMessage message)
        {
            OuterTest.TrackMessage(message);
        }
    }
}