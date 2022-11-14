using System;
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
            assertEventCount(1);
            Assert.Equal(typeof(NoSubscriberMessage), lastEvent.GetType());
            var noSub = (NoSubscriberMessage)lastEvent;
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
            assertEventCount(2);

            Assert.Equal(typeof(SubscriberExceptionMessage), lastEvent.GetType());
            var noSub = (NoSubscriberMessage)((SubscriberExceptionMessage)lastEvent).OriginalMessage;
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

        public class DummySubscriber
        {
            [Subscribe]
            public virtual void OnMessage(string dummy)
            {
            }
        }

        public class BadNoSubscriberSubscriber
        {
            [Subscribe]
            public virtual void OnMessage(NoSubscriberMessage message)
            {
                throw new Exception("I'm bad");
            }
        }
    }
}