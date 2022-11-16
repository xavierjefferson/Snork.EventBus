using System.Threading;
using Snork.EventBus.Tests.Messages;
using Snork.EventBus.Tests.Subscribers;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    /**
 * @author Markus Junginger, greenrobot
 */
    public class StickyMessageTest : TestBase
    {
        [Fact]
        public void TestPostSticky()
        {
            EventBus.PostSticky("Sticky");
            EventBus.Register(this);
            Assert.Equal("Sticky", LastMessage);
            Assert.Equal(Thread.CurrentThread, LastThread);
        }

        [Fact]
        public void TestPostStickyTwoMessages()
        {
            EventBus.PostSticky("Sticky");
            EventBus.PostSticky(new IntTestMessage(7));
            EventBus.Register(this);
            Assert.Equal(2, MessageCount);
        }

        [Fact]
        public void TestPostStickyTwoSubscribers()
        {
            EventBus.PostSticky("Sticky");
            EventBus.PostSticky(new IntTestMessage(7));
            EventBus.Register(this);
            var subscriber2 = new StickyIntTestSubscriber(this);
            EventBus.Register(subscriber2);
            Assert.Equal(3, MessageCount);

            EventBus.PostSticky("Sticky");
            Assert.Equal(4, MessageCount);

            EventBus.PostSticky(new IntTestMessage(8));
            Assert.Equal(6, MessageCount);
        }

        [Fact]
        public void TestPostStickyRegisterNonSticky()
        {
            EventBus.PostSticky("Sticky");
            EventBus.Register(new NonStickySubscriber(this));
            Assert.Null(LastMessage);
            Assert.Equal(0, MessageCount);
        }

        [Fact]
        public void TestPostNonStickyRegisterSticky()
        {
            EventBus.Post("NonSticky");
            EventBus.Register(this);
            Assert.Null(LastMessage);
            Assert.Equal(0, MessageCount);
        }

        [Fact]
        public void TestPostStickyTwice()
        {
            EventBus.PostSticky("Sticky");
            EventBus.PostSticky("NewSticky");
            EventBus.Register(this);
            Assert.Equal("NewSticky", LastMessage);
        }

        [Fact]
        public void TestPostStickyThenPostNormal()
        {
            EventBus.PostSticky("Sticky");
            EventBus.Post("NonSticky");
            EventBus.Register(this);
            Assert.Equal("Sticky", LastMessage);
        }

        [Fact]
        public void TestPostStickyWithRegisterAndUnregister()
        {
            EventBus.Register(this);
            EventBus.PostSticky("Sticky");
            Assert.Equal("Sticky", LastMessage);

            EventBus.Unregister(this);
            EventBus.Register(this);
            Assert.Equal("Sticky", LastMessage);
            Assert.Equal(2, MessageCount);

            EventBus.PostSticky("NewSticky");
            Assert.Equal(3, MessageCount);
            Assert.Equal("NewSticky", LastMessage);

            EventBus.Unregister(this);
            EventBus.Register(this);
            Assert.Equal(4, MessageCount);
            Assert.Equal("NewSticky", LastMessage);
        }

        [Fact]
        public void TestPostStickyAndGet()
        {
            EventBus.PostSticky("Sticky");
            Assert.Equal("Sticky", EventBus.GetStickyMessage(typeof(string)));
        }

        [Fact]
        public void TestPostStickyRemoveClass()
        {
            EventBus.PostSticky("Sticky");
            EventBus.RemoveStickyMessage(typeof(string));
            Assert.Null(EventBus.GetStickyMessage(typeof(string)));
            EventBus.Register(this);
            Assert.Null(LastMessage);
            Assert.Equal(0, MessageCount);
        }

        [Fact]
        public void TestPostStickyRemoveMessage()
        {
            EventBus.PostSticky("Sticky");
            Assert.True(EventBus.RemoveStickyMessage("Sticky"));
            Assert.Null(EventBus.GetStickyMessage(typeof(string)));
            EventBus.Register(this);
            Assert.Null(LastMessage);
            Assert.Equal(0, MessageCount);
        }

        [Fact]
        public void TestPostStickyRemoveAll()
        {
            EventBus.PostSticky("Sticky");
            EventBus.PostSticky(new IntTestMessage(77));
            EventBus.RemoveAllStickyMessages();
            Assert.Null(EventBus.GetStickyMessage(typeof(string)));
            Assert.Null(EventBus.GetStickyMessage(typeof(IntTestMessage)));
            EventBus.Register(this);
            Assert.Null(LastMessage);
            Assert.Equal(0, MessageCount);
        }

        [Fact]
        public void TestRemoveStickyMessageInSubscriber()
        {
            EventBus.Register(new RemoveStickySubscriber(this));
            EventBus.PostSticky("Sticky");
            EventBus.Register(this);
            Assert.Null(LastMessage);
            Assert.Equal(0, MessageCount);
            Assert.Null(EventBus.GetStickyMessage(typeof(string)));
        }

        [Subscribe(sticky: true)]
        public virtual void OnMessage(string message)
        {
            TrackMessage(message);
        }

        [Subscribe(sticky: true)]
        public virtual void OnMessage(IntTestMessage message)
        {
            TrackMessage(message);
        }

        public StickyMessageTest(ITestOutputHelper output) : base(output)
        {
        }
    }
}