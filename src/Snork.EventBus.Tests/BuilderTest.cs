using Snork.EventBus.Tests.Subscribers;
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
            Assert.Throws<EventBusException>(() => { EventBus.Post("Foo"); });
        }

        [Fact]
        public void TestDoNotSendSubscriberExceptionMessage()
        {
            EventBus = EventBus.Builder().WithLogSubscriberExceptions(false).WithSendSubscriberExceptionMessage(false)
                .Build();
            EventBus.Register(new SubscriberExceptionMessageTracker(this));
            EventBus.Register(new ThrowingSubscriber());
            EventBus.Post("Foo");
            AssertMessageCount(0);
        }

        [Fact]
        public void TestDoNotSendNoSubscriberMessage()
        {
            EventBus = EventBus.Builder().WithLogNoSubscriberMessages(false).WithSendNoSubscriberMessage(false).Build();
            EventBus.Register(new NoSubscriberMessageTracker(this));
            EventBus.Post("Foo");
            AssertMessageCount(0);
        }

        [Fact]
        public void TestInstallDefaultEventBus()
        {
            var builder = EventBus.Builder();
            Assert.Throws<EventBusException>(() =>
            {
                // Either this should throw when another unit test got the default message bus...
                EventBus = builder.InstallDefaultEventBus();
                Assert.Equal(EventBus, EventBus.Default);

                // ...or this should throw
                EventBus = builder.InstallDefaultEventBus();
            });
        }

        [Fact]
        public void TestMessageInheritance()
        {
            EventBus = EventBus.Builder().WithMessageInheritance(false).Build();
            EventBus.Register(new ThrowingSubscriber());
            EventBus.Post("Foo");
        }
    }
}