using System.Threading;
using Xunit;

namespace Snork.EventBus.Tests
{
    public class CancelEventDeliveryTest : TestBase
    {
        [Fact]
        public void TestCancel()
        {
            var canceler = new Subscriber(1, true);
            EventBus.Register(new Subscriber(0, false));
            EventBus.Register(canceler);
            EventBus.Register(new Subscriber(0, false));
            EventBus.Post("42");
            Assert.Equal(1, eventCount);

            EventBus.Unregister(canceler);
            EventBus.Post("42");
            Assert.Equal(1 + 2, eventCount);
        }

        [Fact]
        public void TestCancelInBetween()
        {
            EventBus.Register(new Subscriber(2, true));
            EventBus.Register(new Subscriber(1, false));
            EventBus.Register(new Subscriber(3, false));
            EventBus.Post("42");
            Assert.Equal(2, eventCount);
        }

        [Fact]
        public void TestCancelOutsideEventHandler()
        {
            try
            {
                EventBus.CancelEventDelivery(this);
                Assert.True(false, "Should have thrown");
            }
            catch (EventBusException e)
            {
                // Expected
            }
        }

        [Fact]
        public void TestCancelWrongEvent()
        {
            EventBus.Register(new SubscriberCancelOtherEvent(this));
            EventBus.Post("42");
            Assert.Equal(0, eventCount);
            Assert.NotNull(failed);
        }

        public class Subscriber
        {
            private readonly CancelEventDeliveryTest _test;
            private readonly bool cancel;
            private readonly int prio;

            public Subscriber(CancelEventDeliveryTest test)
            {
                _test = test;
            }

            public Subscriber(int prio, bool cancel)
            {
                this.prio = prio;
                this.cancel = cancel;
            }

            [Subscribe]
            public virtual void OnMessage(string message)
            {
                handleEvent(message, 0);
            }

            [Subscribe(priority: 1)]
            public virtual void OnMessage1(string message)
            {
                handleEvent(message, 1);
            }

            [Subscribe(priority: 2)]
            public virtual void OnMessage2(string message)
            {
                handleEvent(message, 2);
            }

            [Subscribe(priority: 3)]
            public virtual void OnMessage3(string message)
            {
                handleEvent(message, 3);
            }

            private void handleEvent(string message, int prio)
            {
                if (this.prio == prio)
                {
                    _test.TrackMessage(message);
                    if (cancel) _test.EventBus.CancelEventDelivery(message);
                }
            }
        }

        public class SubscriberCancelOtherEvent : OuterTestHandlerBase
        {
            public SubscriberCancelOtherEvent(TestBase outerTest) : base(outerTest)
            {
            }

            [Subscribe]
            public virtual void OnMessage(string message)
            {
                try
                {
                    OuterTest.EventBus.CancelEventDelivery(this);
                }
                catch (EventBusException e)
                {
                    OuterTest.failed = e;
                }
            }
        }

        public class SubscriberMainThread : OuterTestHandlerBase
        {
            private readonly CountdownEvent done = new CountdownEvent(1);

            public SubscriberMainThread(TestBase outerTest) : base(outerTest)
            {
            }

            [Subscribe(ThreadModeEnum.Main)]
            public virtual void OnMessageMainThread(string message)
            {
                try
                {
                    OuterTest.EventBus.CancelEventDelivery(message);
                }
                catch (EventBusException e)
                {
                    OuterTest.failed = e;
                }

                done.AddCount(-1);
            }
        }
    }
}