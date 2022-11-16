namespace Snork.EventBus.Tests.Subscribers
{
    public class SubscriberExceptionMessageTracker : OuterTestSubscriberBase
    {
        public SubscriberExceptionMessageTracker(TestBase outerTest) : base(
            outerTest)
        {
        }

        [Subscribe]
        public virtual void OnMessage(SubscriberExceptionMessage message)
        {
            OuterTest.TrackMessage(message);
        }
    }
}