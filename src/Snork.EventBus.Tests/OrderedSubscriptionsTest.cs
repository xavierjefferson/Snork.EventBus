using System.Collections.Generic;
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
            runTestOrdered(new IntTestEvent(42), false, 3);
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
            runTestOrdered(new IntTestEvent(42), true, 3);
        }

        [Fact]
        public void TestOrderedBackgroundThreadSticky()
        {
            runTestOrdered(42, true, 3);
        }

        protected void runTestOrdered(object message, bool sticky, int expectedEventCount)
        {
            var subscriber = sticky ? (object)new PrioSubscriberSticky(this) : new PrioSubscriber(this);
            EventBus.Register(subscriber);
            EventBus.Post(message);

            waitForEventCount(expectedEventCount, 10000);
            Assert.Equal(null, Fail);

            EventBus.Unregister(subscriber);
        }

        public  class PrioSubscriber : OuterTestHandlerBase
        {
            public PrioSubscriber(TestBase text) : base(text)
            {
            }

            [Subscribe(priority: 1)]
            public virtual void OnMessageP1(string message)
            {
                HandleEvent(1, message);
            }

            [Subscribe(priority: -1)]
            public virtual void OnMessageM1(string message)
            {
                HandleEvent(-1, message);
            }

            [Subscribe(priority: 0)]
            public virtual void OnMessageP0(string message)
            {
                HandleEvent(0, message);
            }

            [Subscribe(priority: 10)]
            public virtual void OnMessageP10(string message)
            {
                HandleEvent(10, message);
            }

            [Subscribe(priority: -100)]
            public virtual void OnMessageM100(string message)
            {
                HandleEvent(-100, message);
            }
            //[Subscribe(priority:1)]

            [Subscribe(ThreadModeEnum.Main, priority: -1)]
            public virtual void OnMessageMainThreadM1(IntTestEvent message)
            {
                HandleEvent(-1, message);
            }

            [Subscribe(ThreadModeEnum.Main)]
            public virtual void OnMessageMainThreadP0(IntTestEvent message)
            {
                HandleEvent(0, message);
            }

            [Subscribe(ThreadModeEnum.Main, priority: 1)]
            public virtual void OnMessageMainThreadP1(IntTestEvent message)
            {
                HandleEvent(1, message);
            }

            [Subscribe(ThreadModeEnum.Background, priority: 1)]
            public virtual void OnMessageBackgroundThreadP1(int message)
            {
                HandleEvent(1, message);
            }

            [Subscribe(ThreadModeEnum.Background)]
            public virtual void OnMessageBackgroundThreadP0(int message)
            {
                HandleEvent(0, message);
            }

            [Subscribe(ThreadModeEnum.Background, priority: -1)]
            public virtual void OnMessageBackgroundThreadM1(int message)
            {
                HandleEvent(-1, message);
            }
        }

        public   class PrioSubscriberSticky : OuterTestHandlerBase
        {
            

            public PrioSubscriberSticky(OrderedSubscriptionsTest outerTest) : base(outerTest)
            {
            }

            [Subscribe(priority: 1, sticky: true)]
            public virtual void OnMessageP1(string message)
            {
                HandleEvent(1, message);
            }


            [Subscribe(priority: -1, sticky: true)]
            public virtual void OnMessageM1(string message)
            {
                HandleEvent(-1, message);
            }

            [Subscribe(priority: 0, sticky: true)]
            public virtual void OnMessageP0(string message)
            {
                HandleEvent(0, message);
            }

            [Subscribe(priority: 10, sticky: true)]
            public virtual void OnMessageP10(string message)
            {
                HandleEvent(10, message);
            }

            [Subscribe(priority: -100, sticky: true)]
            public virtual void OnMessageM100(string message)
            {
                HandleEvent(-100, message);
            }

            [Subscribe(ThreadModeEnum.Main, priority: -1, sticky: true)]
            public virtual void OnMessageMainThreadM1(IntTestEvent message)
            {
                HandleEvent(-1, message);
            }

            [Subscribe(ThreadModeEnum.Main, true)]
            public virtual void OnMessageMainThreadP0(IntTestEvent message)
            {
                HandleEvent(0, message);
            }

            [Subscribe(ThreadModeEnum.Main, priority: 1, sticky: true)]
            public virtual void OnMessageMainThreadP1(IntTestEvent message)
            {
                HandleEvent(1, message);
            }

            [Subscribe(ThreadModeEnum.Background, priority: 1, sticky: true)]
            public virtual void OnMessageBackgroundThreadP1(int message)
            {
                HandleEvent(1, message);
            }

            [Subscribe(ThreadModeEnum.Background, true)]
            public virtual void OnMessageBackgroundThreadP0(int message)
            {
                HandleEvent(0, message);
            }

            [Subscribe(ThreadModeEnum.Background, priority: -1, sticky: true)]
            public virtual void OnMessageBackgroundThreadM1(int message)
            {
                HandleEvent(-1, message);
            }
        }
    }
}