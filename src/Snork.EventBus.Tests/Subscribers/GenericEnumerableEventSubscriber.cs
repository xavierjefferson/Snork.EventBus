using System.Collections.Generic;

namespace Snork.EventBus.Tests.Subscribers
{
    public class GenericEnumerableEventSubscriber<T> : OuterTestSubscriberBase
    {
        public GenericEnumerableEventSubscriber(TestBase outerTest) : base(outerTest)
        {
        }

        [Subscribe]
        public void OnGenericEvent(IEnumerable<T> @event)
        {
            OuterTest.TrackEvent(@event);
        }
    }
}