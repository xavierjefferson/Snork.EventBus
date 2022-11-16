using Snork.EventBus.Tests.Messages;

namespace Snork.EventBus.Tests.Subscribers
{
    public class NonStickySubscriber : OuterTestSubscriberBase
    {
        public NonStickySubscriber(TestBase outerTest) : base(outerTest)
        {
        }

        [Subscribe]
        public virtual void OnMessage(string message)
        {
            OuterTest.TrackMessage(message);
        }

        [Subscribe]
        public virtual void OnMessage(IntTestMessage message)
        {
            OuterTest.TrackMessage(message);
        }
    }
}