using System;

namespace Snork.EventBus.Tests.Subscribers
{
    public class GenericNumberMessageSubscriber<T> : OuterTestSubscriberBase
    {
        public GenericNumberMessageSubscriber(TestBase outerTest) : base(outerTest)
        {
        }

        [Subscribe]
        public void OnGenericMessage<U>(U message) where U : notnull
        {
            OuterTest.TrackMessage(message);
        }
    }
}