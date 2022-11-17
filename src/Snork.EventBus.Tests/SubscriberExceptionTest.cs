using System;
using Snork.EventBus.Tests.Subscribers;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class SubscriberExceptionTest : TestBase
    {
        public SubscriberExceptionTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void TestSubscriberExceptionEvent()
        {
            EventBus = EventBus.Builder().WithLogSubscriberExceptions(false).Build();
            EventBus.Register(this);
            EventBus.Post("Foo");

            AssertEventCount(1);
            Assert.Equal(typeof(SubscriberExceptionEvent), LastEvent.GetType());
            var exEvent = (SubscriberExceptionEvent)LastEvent;
            Assert.Equal("Foo", exEvent.OriginalEvent);
            Assert.Same(this, exEvent.OriginalSubscriber);
            Assert.Equal("Bar", exEvent.Exception.Message);
        }

        [Fact]
        public void TestBadExceptionSubscriber()
        {
            EventBus = EventBus.Builder().WithLogSubscriberExceptions(false).Build();
            EventBus.Register(this);
            EventBus.Register(new BadExceptionSubscriber());
            EventBus.Post("Foo");
            AssertEventCount(1);
        }

        [Subscribe]
        public virtual void OnEvent(string @event)
        {
            throw new Exception("Bar");
        }

        [Subscribe]
        public virtual void OnEvent(SubscriberExceptionEvent @event)
        {
            TrackEvent(@event);
        }
    }
}