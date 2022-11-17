using Snork.EventBus.Tests.Subscribers;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class BuilderTest : TestBase
    {
        public BuilderTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void TestThrowSubscriberException()
        {
            EventBus = EventBus.Builder().WithThrowSubscriberException(true).WithLogger(Logger).Build();
            EventBus.Register(new SubscriberExceptionEventTracker(this));
            EventBus.Register(new ThrowingSubscriber());
            Assert.Throws<EventBusException>(() => { EventBus.Post("Foo"); });
        }

        [Fact]
        public void TestDoNotSendSubscriberExceptionEvent()
        {
            EventBus = EventBus.Builder().WithLogSubscriberExceptions(false).WithSendSubscriberExceptionEvent(false)
                .WithLogger(Logger)
                .Build();
            EventBus.Register(new SubscriberExceptionEventTracker(this));
            EventBus.Register(new ThrowingSubscriber());
            EventBus.Post("Foo");
            AssertEventCount(0);
        }

        [Fact]
        public void TestDoNotSendNoSubscriberEvent()
        {
            EventBus = EventBus.Builder().WithLogNoSubscriberEvents(false).WithSendNoSubscriberEvent(false)
                .WithLogger(Logger).Build();
            EventBus.Register(new NoSubscriberEventTracker(this));
            EventBus.Post("Foo");
            AssertEventCount(0);
        }

        [Fact]
        public void TestInstallDefaultEventBus()
        {
            var builder = EventBus.Builder().WithLogger(Logger);
            Assert.Throws<EventBusException>(() =>
            {
                // Either this should throw when another unit test got the default event bus...
                EventBus = builder.InstallDefaultEventBus();
                Assert.Equal(EventBus, EventBus.Default);

                // ...or this should throw
                EventBus = builder.InstallDefaultEventBus();
            });
        }

        [Fact]
        public void TestEventInheritance()
        {
            EventBus = EventBus.Builder().WithEventInheritance(false).WithLogger(Logger).Build();
            EventBus.Register(new ThrowingSubscriber());
            EventBus.Post("Foo");
        }
    }
}