using System.Collections.Generic;
using Snork.EventBus.Tests.Messages;
using Snork.EventBus.Tests.Subscribers;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    /**
 * @author Markus Junginger, greenrobot
 */
    public class OrderedSubscriptionsTest : TestBase
    {

        [Fact]
        public void TestOrdered()
        {
            RunTestOrdered("42", false, 5);
        }

        [Fact]
        public void TestOrderedMainThread()
        {
            RunTestOrdered(new IntTestMessage(42), false, 3);
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
            RunTestOrdered(new IntTestMessage(42), true, 3);
        }

        [Fact]
        public void TestOrderedBackgroundThreadSticky()
        {
            RunTestOrdered(42, true, 3);
        }


        protected void RunTestOrdered(object message, bool sticky, int expectedMessageCount)
        {
            var subscriber = sticky ? (object)new StickyPrioritySubscriber(this) : new PrioritySubscriber(this);
            EventBus.Register(subscriber);
            EventBus.Post(message);

            WaitForMessageCount(expectedMessageCount, 10000);
            Assert.Null(Fail);

            EventBus.Unregister(subscriber);
        }

        public OrderedSubscriptionsTest(ITestOutputHelper output) : base(output)
        {
        }
    }
}