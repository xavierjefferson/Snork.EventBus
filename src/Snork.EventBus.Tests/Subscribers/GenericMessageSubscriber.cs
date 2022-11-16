namespace Snork.EventBus.Tests.Subscribers
{
    public class GenericMessageSubscriber<T> : OuterTestSubscriberBase
    {
        public GenericMessageSubscriber(TestBase outerTest) : base(outerTest)
        {
        }

        [Subscribe]
        public void OnGenericMessage(GenericMessage<T> message)
        {
            OuterTest.TrackMessage(message);
        }
    }
}