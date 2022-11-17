using Snork.EventBus.Tests.Events;

namespace Snork.EventBus.Tests.Subscribers
{
    public class NonStickySubscriber : OuterTestSubscriberBase
    {
        public NonStickySubscriber(TestBase outerTest) : base(outerTest)
        {
        }

        [Subscribe]
        public virtual void OnEvent(string @event)
        {
            OuterTest.TrackEvent(@event);
        }

        [Subscribe]
        public virtual void OnEvent(IntTestEvent @event)
        {
            OuterTest.TrackEvent(@event);
        }
    }
}