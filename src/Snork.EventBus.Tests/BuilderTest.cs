using System;
using Xunit;

namespace Snork.EventBus.Tests
{
    /**
 * @author Markus Junginger, greenrobot
 */
    public class BuilderTest : TestBase
    {
        [Fact]
        public void TestThrowSubscriberException()
        {
            EventBus = EventBus.Builder().WithThrowSubscriberException(true).Build();
            EventBus.Register(new SubscriberExceptionMessageTracker(this));
            EventBus.Register(new ThrowingSubscriber());
            try
            {
                EventBus.Post("Foo");
                Assert.True(false, "Should have thrown");
            }
            catch (EventBusException e)
            {
                // Expected
            }
        }

        [Fact]
        public void TestDoNotSendSubscriberExceptionMessage()
        {
            EventBus = EventBus.Builder().WithLogSubscriberExceptions(false).WithSendSubscriberExceptionMessage(false)
                .Build();
            EventBus.Register(new SubscriberExceptionMessageTracker(this));
            EventBus.Register(new ThrowingSubscriber());
            EventBus.Post("Foo");
            assertEventCount(0);
        }

        [Fact]
        public void TestDoNotSendNoSubscriberMessage()
        {
            EventBus = EventBus.Builder().WithLogNoSubscriberMessages(false).WithSendNoSubscriberMessage(false).Build();
            EventBus.Register(new NoSubscriberMessageTracker(this));
            EventBus.Post("Foo");
            assertEventCount(0);
        }

        [Fact]
        public void TestInstallDefaultEventBus()
        {
            var builder = EventBus.Builder();
            try
            {
                // Either this should throw when another unit test got the default message bus...
                EventBus = builder.InstallDefaultEventBus();
                Assert.Equal(EventBus, EventBus.Default);

                // ...or this should throw
                EventBus = builder.InstallDefaultEventBus();
                Assert.True(false, "Should have thrown");
            }
            catch (EventBusException e)
            {
                // Expected
            }
        }

        [Fact]
        public void TestEventInheritance()
        {
            EventBus = EventBus.Builder().WithMessageInheritance(false).Build();
            EventBus.Register(new ThrowingSubscriber());
            EventBus.Post("Foo");
        }

        public class SubscriberExceptionMessageTracker : OuterTestHandlerBase
        {
            public SubscriberExceptionMessageTracker(TestBase outerTest) : base(
                outerTest)
            {
            }

            [Subscribe]
            public virtual void OnMessage(SubscriberExceptionMessage message)
            {
                OuterTest.TrackMessage(message);
            }
        }

        public class NoSubscriberMessageTracker : OuterTestHandlerBase
        {
            public NoSubscriberMessageTracker(TestBase outerTest) : base(outerTest)
            {
            }

            [Subscribe]
            public virtual void OnMessage(NoSubscriberMessage message)
            {
                OuterTest.TrackMessage(message);
            }
        }

        public class ThrowingSubscriber
        {
            [Subscribe]
            public virtual void OnMessage(object message)
            {
                throw new Exception();
            }
        }
    }
}