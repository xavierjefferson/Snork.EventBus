using System;
using System.Collections.Generic;

namespace Snork.EventBus.Tests.Subscribers
{
    public class GenericEnumerableMessageSubscriber<T> : OuterTestSubscriberBase
    {
        public GenericEnumerableMessageSubscriber(TestBase outerTest) : base(outerTest)
        {
        }

        [Subscribe]
        public void OnGenericMessage(IEnumerable<T> message) 
        {
            OuterTest.TrackMessage(message);
        }
    }
}