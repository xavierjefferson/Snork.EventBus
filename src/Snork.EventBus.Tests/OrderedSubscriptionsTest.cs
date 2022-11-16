using System.Collections.Generic;
using Snork.EventBus.Tests.Messages;
using Snork.EventBus.Tests.Subscribers;
using Xunit;

namespace Snork.EventBus.Tests
{
    /**
 * @author Markus Junginger, greenrobot
 */
    public class OrderedSubscriptionsTest : TestBase
    {
        private readonly List<PrioSubscriber> registered = new List<PrioSubscriber>();

        [Fact]
        public void TestOrdered()
        {
            runTestOrdered("42", false, 5);
        }

        [Fact]
        public void TestOrderedMainThread()
        {
            runTestOrdered(new IntTestMessage(42), false, 3);
        }

        [Fact]
        public void TestOrderedBackgroundThread()
        {
            runTestOrdered(42, false, 3);
        }

        [Fact]
        public void TestOrderedSticky()
        {
            runTestOrdered("42", true, 5);
        }

        [Fact]
        public void TestOrderedMainThreadSticky()
        {
            runTestOrdered(new IntTestMessage(42), true, 3);
        }

        [Fact]
        public void TestOrderedBackgroundThreadSticky()
        {
            runTestOrdered(42, true, 3);
        }

        protected void runTestOrdered(object message, bool sticky, int expectedMessageCount)
        {
            var subscriber = sticky ? (object)new PrioSubscriberSticky(this) : new PrioSubscriber(this);
            EventBus.Register(subscriber);
            EventBus.Post(message);

            WaitForMessageCount(expectedMessageCount, 10000);
            Assert.Equal(null, Fail);

            EventBus.Unregister(subscriber);
        }

        public class PrioSubscriberSticky : MessageOrderedPriorityTestSubscriberBase
        {
            public PrioSubscriberSticky(OrderedSubscriptionsTest outerTest) : base(outerTest)
            {
            }

            [Subscribe(priority: 1, sticky: true)]
            public virtual void OnMessageP1(string message)
            {
                HandleMessage(1, message);
            }


            [Subscribe(priority: -1, sticky: true)]
            public virtual void OnMessageM1(string message)
            {
                HandleMessage(-1, message);
            }

            [Subscribe(priority: 0, sticky: true)]
            public virtual void OnMessageP0(string message)
            {
                HandleMessage(0, message);
            }

            [Subscribe(priority: 10, sticky: true)]
            public virtual void OnMessageP10(string message)
            {
                HandleMessage(10, message);
            }

            [Subscribe(priority: -100, sticky: true)]
            public virtual void OnMessageM100(string message)
            {
                HandleMessage(-100, message);
            }

            [Subscribe(ThreadModeEnum.Main, priority: -1, sticky: true)]
            public virtual void OnMessageMainThreadM1(IntTestMessage message)
            {
                HandleMessage(-1, message);
            }

            [Subscribe(ThreadModeEnum.Main, true)]
            public virtual void OnMessageMainThreadP0(IntTestMessage message)
            {
                HandleMessage(0, message);
            }

            [Subscribe(ThreadModeEnum.Main, priority: 1, sticky: true)]
            public virtual void OnMessageMainThreadP1(IntTestMessage message)
            {
                HandleMessage(1, message);
            }

            [Subscribe(ThreadModeEnum.Background, priority: 1, sticky: true)]
            public virtual void OnMessageBackgroundThreadP1(int message)
            {
                HandleMessage(1, message);
            }

            [Subscribe(ThreadModeEnum.Background, true)]
            public virtual void OnMessageBackgroundThreadP0(int message)
            {
                HandleMessage(0, message);
            }

            [Subscribe(ThreadModeEnum.Background, priority: -1, sticky: true)]
            public virtual void OnMessageBackgroundThreadM1(int message)
            {
                HandleMessage(-1, message);
            }
        }
    }
}