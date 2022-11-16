namespace Snork.EventBus.Tests.Subscribers
{
    public class FullGenericMessageSubscriber<T> : OuterTestSubscriberBase
    {
        public FullGenericMessageSubscriber(TestBase outerTest) : base(outerTest)
        {
        }

        [Subscribe]
        public void OnGenericMessage(T message)
        {
            OuterTest.TrackMessage(message);
        }
    }
}