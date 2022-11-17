using System.Threading;
using Snork.EventBus.Tests.Events;
using Snork.EventBus.Tests.Subscribers;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class StickyEventTest : TestBase
    {
        public StickyEventTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void TestPostSticky()
        {
            EventBus.PostSticky("Sticky");
            EventBus.Register(this);
            Assert.Equal("Sticky", LastEvent);
            Assert.Equal(Thread.CurrentThread, LastThread);
        }

        [Fact]
        public void TestPostStickyTwoEvents()
        {
            EventBus.PostSticky("Sticky");
            EventBus.PostSticky(new IntTestEvent(7));
            EventBus.Register(this);
            Assert.Equal(2, EventCount);
        }

        [Fact]
        public void TestPostStickyTwoSubscribers()
        {
            EventBus.PostSticky("Sticky");
            EventBus.PostSticky(new IntTestEvent(7));
            EventBus.Register(this);
            var subscriber2 = new StickyIntTestSubscriber(this);
            EventBus.Register(subscriber2);
            Assert.Equal(3, EventCount);

            EventBus.PostSticky("Sticky");
            Assert.Equal(4, EventCount);

            EventBus.PostSticky(new IntTestEvent(8));
            Assert.Equal(6, EventCount);
        }

        [Fact]
        public void TestPostStickyRegisterNonSticky()
        {
            EventBus.PostSticky("Sticky");
            EventBus.Register(new NonStickySubscriber(this));
            Assert.Null(LastEvent);
            Assert.Equal(0, EventCount);
        }

        [Fact]
        public void TestPostNonStickyRegisterSticky()
        {
            EventBus.Post("NonSticky");
            EventBus.Register(this);
            Assert.Null(LastEvent);
            Assert.Equal(0, EventCount);
        }

        [Fact]
        public void TestPostStickyTwice()
        {
            EventBus.PostSticky("Sticky");
            EventBus.PostSticky("NewSticky");
            EventBus.Register(this);
            Assert.Equal("NewSticky", LastEvent);
        }

        [Fact]
        public void TestPostStickyThenPostNormal()
        {
            EventBus.PostSticky("Sticky");
            EventBus.Post("NonSticky");
            EventBus.Register(this);
            Assert.Equal("Sticky", LastEvent);
        }

        [Fact]
        public void TestPostStickyWithRegisterAndUnregister()
        {
            EventBus.Register(this);
            EventBus.PostSticky("Sticky");
            Assert.Equal("Sticky", LastEvent);

            EventBus.Unregister(this);
            EventBus.Register(this);
            Assert.Equal("Sticky", LastEvent);
            Assert.Equal(2, EventCount);

            EventBus.PostSticky("NewSticky");
            Assert.Equal(3, EventCount);
            Assert.Equal("NewSticky", LastEvent);

            EventBus.Unregister(this);
            EventBus.Register(this);
            Assert.Equal(4, EventCount);
            Assert.Equal("NewSticky", LastEvent);
        }

        [Fact]
        public void TestPostStickyAndGet()
        {
            EventBus.PostSticky("Sticky");
            Assert.Equal("Sticky", EventBus.GetStickyEvent(typeof(string)));
        }

        [Fact]
        public void TestPostStickyRemoveClass()
        {
            EventBus.PostSticky("Sticky");
            EventBus.RemoveStickyEvent(typeof(string));
            Assert.Null(EventBus.GetStickyEvent(typeof(string)));
            EventBus.Register(this);
            Assert.Null(LastEvent);
            Assert.Equal(0, EventCount);
        }

        [Fact]
        public void TestPostStickyRemoveEvent()
        {
            EventBus.PostSticky("Sticky");
            Assert.True(EventBus.RemoveStickyEvent("Sticky"));
            Assert.Null(EventBus.GetStickyEvent(typeof(string)));
            EventBus.Register(this);
            Assert.Null(LastEvent);
            Assert.Equal(0, EventCount);
        }

        [Fact]
        public void TestPostStickyRemoveAll()
        {
            EventBus.PostSticky("Sticky");
            EventBus.PostSticky(new IntTestEvent(77));
            EventBus.RemoveAllStickyEvents();
            Assert.Null(EventBus.GetStickyEvent(typeof(string)));
            Assert.Null(EventBus.GetStickyEvent(typeof(IntTestEvent)));
            EventBus.Register(this);
            Assert.Null(LastEvent);
            Assert.Equal(0, EventCount);
        }

        [Fact]
        public void TestRemoveStickyEventInSubscriber()
        {
            EventBus.Register(new RemoveStickySubscriber(this));
            EventBus.PostSticky("Sticky");
            EventBus.Register(this);
            Assert.Null(LastEvent);
            Assert.Equal(0, EventCount);
            Assert.Null(EventBus.GetStickyEvent(typeof(string)));
        }

        [Subscribe(sticky: true)]
        public virtual void OnEvent(string @event)
        {
            TrackEvent(@event);
        }

        [Subscribe(sticky: true)]
        public virtual void OnEvent(IntTestEvent @event)
        {
            TrackEvent(@event);
        }
    }
}