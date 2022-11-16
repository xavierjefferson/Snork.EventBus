using Snork.EventBus.Tests.Subscribers;
using Xunit;

namespace Snork.EventBus.Tests
{
    public class NoSubscriberMessageTest : TestBase
    {
        [Fact]
        public void TestNoSubscriberMessage()
        {
            EventBus.Register(this);
            EventBus.Post("Foo");
            AssertMessageCount(1);
            Assert.Equal(typeof(NoSubscriberMessage), LastMessage.GetType());
            var noSub = (NoSubscriberMessage)LastMessage;
            Assert.Equal("Foo", noSub.OriginalMessage);
            Assert.Same(EventBus, noSub.EventBus);
        }

        [Fact]
        public void TestNoSubscriberMessageAfterUnregister()
        {
            object subscriber = new DummySubscriber();
            EventBus.Register(subscriber);
            EventBus.Unregister(subscriber);
            TestNoSubscriberMessage();
        }

        [Fact]
        public void TestBadNoSubscriberSubscriber()
        {
            EventBus = EventBus.Builder().WithLogNoSubscriberMessages(false).Build();
            EventBus.Register(this);
            EventBus.Register(new BadNoSubscriberSubscriber());
            EventBus.Post("Foo");
            AssertMessageCount(2);

            Assert.Equal(typeof(SubscriberExceptionMessage), LastMessage.GetType());
            var noSub = (NoSubscriberMessage)((SubscriberExceptionMessage)LastMessage).OriginalMessage;
            Assert.Equal("Foo", noSub.OriginalMessage);
        }

        [Subscribe]
        public virtual void OnMessage(NoSubscriberMessage message)
        {
            TrackMessage(message);
        }

        [Subscribe]
        public virtual void OnMessage(SubscriberExceptionMessage message)
        {
            TrackMessage(message);
        }
    }
}