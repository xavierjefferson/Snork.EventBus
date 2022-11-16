using Snork.EventBus.Tests.Messages;

namespace Snork.EventBus.Tests.Subscribers
{
    public class StickyIntTestSubscriber : OuterTestSubscriberBase
    {
        public StickyIntTestSubscriber(TestBase outerTest) : base(outerTest)
        {
        }

        [Subscribe(sticky: true)]
        public virtual void OnMessage(IntTestMessage message)
        {
            OuterTest.TrackMessage(message);
        }
    }
}