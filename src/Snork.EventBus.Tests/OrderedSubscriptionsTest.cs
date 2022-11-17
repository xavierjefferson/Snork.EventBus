using Snork.EventBus.Tests.Events;
using Snork.EventBus.Tests.Subscribers;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class OrderedSubscriptionsTest : TestBase
    {
        public OrderedSubscriptionsTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void TestOrdered()
        {
            RunTestOrdered("42", false, 5);
        }

        [Fact]
        public void TestOrderedMainThread()
        {
            RunTestOrdered(new IntTestEvent(42), false, 3);
        }

        [Fact]
        public void TestOrderedBackgroundThread()
        {
            RunTestOrdered(42, false, 3);
        }

        [Fact]
        public void TestOrderedSticky()
        {
            RunTestOrdered("42", true, 5);
        }

        [Fact]
        public void TestOrderedMainThreadSticky()
        {
            RunTestOrdered(new IntTestEvent(42), true, 3);
        }

        [Fact]
        public void TestOrderedBackgroundThreadSticky()
        {
            RunTestOrdered(42, true, 3);
        }


        protected void RunTestOrdered(object @event, bool sticky, int expectedEventCount)
        {
            var subscriber = sticky ? (object)new StickyPrioritySubscriber(this) : new PrioritySubscriber(this);
            EventBus.Register(subscriber);
            EventBus.Post(@event);

            WaitForEventCount(expectedEventCount, 10000);
            Assert.Null(Fail);

            EventBus.Unregister(subscriber);
        }
    }
}