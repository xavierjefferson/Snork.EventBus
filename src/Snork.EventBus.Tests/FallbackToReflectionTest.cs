using Snork.EventBus.Tests.Subscribers;

namespace Snork.EventBus.Tests
{
    public class FallbackToReflectionTest : TestBase
    {
        public FallbackToReflectionTest() : base(true)
        {
        }

        //[Fact]
        //public void TestAnonymousSubscriberClass() {
        //    object subscriber = new object() {
        //        [Subscribe()]            public virtual void OnMessage(string message) {
        //        trackMessage(message);
        //    }
        //    };
        //    EventBus.Register(subscriber);

        //    EventBus.Post("Hello");
        //    Assert.Equal("Hello", lastMessage);
        //    Assert.Equal(1, eventsReceived.Count);
        //}

        //[Fact]
        //public void TestAnonymousSubscriberClassWithPublicSuperclass() {
        //    object subscriber = new PublicClass() {
        //        [Subscribe()]            public virtual void OnMessage(string message) {
        //        trackMessage(message);
        //    }
        //    };
        //    EventBus.Register(subscriber);

        //    EventBus.Post("Hello");
        //    Assert.Equal("Hello", lastMessage);
        //    Assert.Equal(2, eventsReceived.Count);
        //}

        //[Fact]
        //public void TestAnonymousSubscriberClassWithPrivateSuperclass() {
        //    EventBus.Register(new PublicWithPrivateSuperClass());
        //    EventBus.Post("Hello");
        //    Assert.Equal("Hello", lastMessage);
        //    Assert.Equal(2, eventsReceived.Count);
        //}

        //[Fact]
        //public void TestSubscriberClassWithPrivateMessage()
        //{
        //    EventBus.Register(new PublicClassWithPrivateMessage(this));
        //    var privateMessage = new PrivateMessage();
        //    EventBus.Post(privateMessage);
        //    Assert.Equal(privateMessage, lastMessage);
        //    Assert.Equal(1, eventsReceived.Count);
        //}

        //[Fact]
        //public void TestSubscriberClassWithPublicAndPrivateMessage()
        //{
        //    EventBus.Register(new PublicClassWithPublicAndPrivateMessage());

        //    EventBus.Post("Hello");
        //    Assert.Equal("Hello", lastMessage);
        //    Assert.Equal(1, eventsReceived.Count);

        //    var privateMessage = new PrivateMessage();
        //    EventBus.Post(privateMessage);
        //    Assert.Equal(privateMessage, lastMessage);
        //    Assert.Equal(2, eventsReceived.Count);
        //}

        //[Fact]
        //public void TestSubscriberExtendingClassWithPrivateMessage()
        //{
        //    EventBus.Register(new PublicWithPrivateMessageInSuperclass(this));
        //    var privateMessage = new PrivateMessage();
        //    EventBus.Post(privateMessage);
        //    Assert.Equal(privateMessage, lastMessage);
        //    Assert.Equal(2, eventsReceived.Count);
        //}

        private class PrivateMessage
        {
        }

        public class PublicClass : OuterTestSubscriberBase
        {
            public PublicClass(TestBase outerTest) : base(outerTest)
            {
            }

            [Subscribe]
            public virtual void OnMessage(object any)
            {
                OuterTest.TrackMessage(any);
            }
        }

        internal class PrivateClass : OuterTestSubscriberBase
        {
            public PrivateClass(TestBase outerTest) : base(outerTest)
            {
            }

            [Subscribe]
            public virtual void OnMessage(object any)
            {
                OuterTest.TrackMessage(any);
            }
        }

        //public class PublicWithPrivateSuperClass : PrivateClass
        //{
        //    [Subscribe]
        //    public virtual void OnMessage(string any)
        //    {
        //        OuterTest.TrackMessage(any);
        //    }
        //}

        //public class PublicClassWithPrivateMessage : OuterTestSubscriberBase
        //{
        //    public PublicClassWithPrivateMessage(TestBase outerTest) : base(outerTest)
        //    {
        //    }

        //    [Subscribe]
        //    public virtual void OnMessage(PrivateMessage any)
        //    {
        //        OuterTest.TrackMessage(any);
        //    }
        //}

        //public class PublicClassWithPublicAndPrivateMessage : OuterTestSubscriberBase
        //{
        //    public PublicClassWithPublicAndPrivateMessage(TestBase outerTest) : base(
        //        outerTest)
        //    {
        //    }

        //    [Subscribe]
        //    public virtual void OnMessage(string any)
        //    {
        //        OuterTest.TrackMessage(any);
        //    }

        //    [Subscribe]
        //    public virtual void OnMessage(PrivateMessage any)
        //    {
        //        OuterTest.TrackMessage(any);
        //    }
        //}

        //public class PublicWithPrivateMessageInSuperclass : PublicClassWithPrivateMessage
        //{
        //    public PublicWithPrivateMessageInSuperclass(TestBase outerTest) : base(
        //        outerTest)
        //    {
        //    }

        //    [Subscribe]
        //    public virtual void OnMessage(object any)
        //    {
        //        OuterTest.TrackMessage(any);
        //    }
        //}
    }
}