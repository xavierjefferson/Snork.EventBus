using System.Threading;
using Snork.EventBus.Tests.Subscribers;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class CancelEventDeliveryTest : TestBase
    {
        public CancelEventDeliveryTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void TestCancel()
        {
            var canceler = new ConfigurableCancellingSubscriber(1, true, this);
            EventBus.Register(new ConfigurableCancellingSubscriber(0, false, this));
            EventBus.Register(canceler);
            EventBus.Register(new ConfigurableCancellingSubscriber(0, false, this));
            EventBus.Post("42");
            Assert.Equal(1, EventCount);

            EventBus.Unregister(canceler);
            EventBus.Post("42");
            Assert.Equal(1 + 2, EventCount);
        }

        [Fact]
        public void TestCancelInBetween()
        {
            EventBus.Register(new ConfigurableCancellingSubscriber(2, true, this));
            EventBus.Register(new ConfigurableCancellingSubscriber(1, false, this));
            EventBus.Register(new ConfigurableCancellingSubscriber(3, false, this));
            EventBus.Post("42");
            Assert.Equal(2, EventCount);
        }

        [Fact]
        public void TestCancelOutsideEventHandler()
        {
            Assert.Throws<EventBusException>(() => { EventBus.CancelEventDelivery(this); });
        }

        [Fact]
        public void TestCancelWrongEvent()
        {
            EventBus.Register(new CancellingSubscriber(this));
            EventBus.Post("42");
            Assert.Equal(0, EventCount);
            Assert.NotNull(LastException);
        }

        public class SubscriberMainThread : OuterTestSubscriberBase
        {
            private readonly CountdownEvent done = new CountdownEvent(1);

            public SubscriberMainThread(TestBase outerTest) : base(outerTest)
            {
            }

            [Subscribe(ThreadModeEnum.Main)]
            public virtual void OnEventMainThread(string @event)
            {
                try
                {
                    OuterTest.EventBus.CancelEventDelivery(@event);
                }
                catch (EventBusException e)
                {
                    OuterTest.LastException = e;
                }

                done.Signal();
            }
        }
    }
}