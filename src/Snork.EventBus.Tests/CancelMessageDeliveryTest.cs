using System.Threading;
using Snork.EventBus.Tests.Subscribers;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class CancelMessageDeliveryTest : TestBase
    {
        [Fact]
        public void TestCancel()
        {
            var canceler = new ConfigurableCancellingSubscriber(1, true, this);
            EventBus.Register(new ConfigurableCancellingSubscriber(0, false, this));
            EventBus.Register(canceler);
            EventBus.Register(new ConfigurableCancellingSubscriber(0, false, this));
            EventBus.Post("42");
            Assert.Equal(1, MessageCount);

            EventBus.Unregister(canceler);
            EventBus.Post("42");
            Assert.Equal(1 + 2, MessageCount);
        }

        [Fact]
        public void TestCancelInBetween()
        {
            EventBus.Register(new ConfigurableCancellingSubscriber(2, true, this));
            EventBus.Register(new ConfigurableCancellingSubscriber(1, false, this));
            EventBus.Register(new ConfigurableCancellingSubscriber(3, false, this));
            EventBus.Post("42");
            Assert.Equal(2, MessageCount);
        }

        [Fact]
        public void TestCancelOutsideMessageHandler()
        {
            Assert.Throws<EventBusException>(() => { EventBus.CancelMessageDelivery(this); });
        }

        [Fact]
        public void TestCancelWrongMessage()
        {
            EventBus.Register(new CancellingSubscriber(this));
            EventBus.Post("42");
            Assert.Equal(0, MessageCount);
            Assert.NotNull(LastException);
        }

        public class SubscriberMainThread : OuterTestSubscriberBase
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
                    OuterTest.EventBus.CancelMessageDelivery(message);
                }
                catch (EventBusException e)
                {
                    OuterTest.LastException = e;
                }

                done.Signal();
            }
        }

        public CancelMessageDeliveryTest(ITestOutputHelper output) : base(output)
        {
        }
    }
}