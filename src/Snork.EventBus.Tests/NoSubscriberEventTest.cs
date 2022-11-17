using Snork.EventBus.Tests.Subscribers;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class NoSubscriberEventTest : TestBase
    {
        public NoSubscriberEventTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void TestNoSubscriberEvent()
        {
            EventBus.Register(this);
            EventBus.Post("Foo");
            AssertEventCount(1);
            Assert.Equal(typeof(NoSubscriberEvent), LastEvent.GetType());
            var noSub = (NoSubscriberEvent)LastEvent;
            Assert.Equal("Foo", noSub.OriginalEvent);
            Assert.Same(EventBus, noSub.EventBus);
        }

        [Fact]
        public void TestNoSubscriberEventAfterUnregister()
        {
            object subscriber = new DummySubscriber();
            EventBus.Register(subscriber);
            EventBus.Unregister(subscriber);
            TestNoSubscriberEvent();
        }

        [Fact]
        public void TestBadNoSubscriberSubscriber()
        {
            EventBus = EventBus.Builder().WithLogNoSubscriberEvents(false).Build();
            EventBus.Register(this);
            EventBus.Register(new BadNoSubscriberSubscriber());
            EventBus.Post("Foo");
            AssertEventCount(2);

            Assert.Equal(typeof(SubscriberExceptionEvent), LastEvent.GetType());
            var noSub = (NoSubscriberEvent)((SubscriberExceptionEvent)LastEvent).OriginalEvent;
            Assert.Equal("Foo", noSub.OriginalEvent);
        }

        [Subscribe]
        public virtual void OnEvent(NoSubscriberEvent @event)
        {
            TrackEvent(@event);
        }

        [Subscribe]
        public virtual void OnEvent(SubscriberExceptionEvent @event)
        {
            TrackEvent(@event);
        }
    }
}