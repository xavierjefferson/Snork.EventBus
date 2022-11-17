using Snork.EventBus.Tests.Events;

namespace Snork.EventBus.Tests.Subscribers
{
    public class StickyIntTestSubscriber : OuterTestSubscriberBase
    {
        public StickyIntTestSubscriber(TestBase outerTest) : base(outerTest)
        {
        }

        [Subscribe(sticky: true)]
        public virtual void OnEvent(IntTestEvent @event)
        {
            OuterTest.TrackEvent(@event);
        }
    }
}