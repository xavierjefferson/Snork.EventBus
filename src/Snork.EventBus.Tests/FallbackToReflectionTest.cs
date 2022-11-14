using Xunit;

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
        //        trackEvent(message);
        //    }
        //    };
        //    EventBus.Register(subscriber);

        //    EventBus.Post("Hello");
        //    Assert.Equal("Hello", lastEvent);
        //    Assert.Equal(1, eventsReceived.Count);
        //}

        //[Fact]
        //public void TestAnonymousSubscriberClassWithPublicSuperclass() {
        //    object subscriber = new PublicClass() {
        //        [Subscribe()]            public virtual void OnMessage(string message) {
        //        trackEvent(message);
        //    }
        //    };
        //    EventBus.Register(subscriber);

        //    EventBus.Post("Hello");
        //    Assert.Equal("Hello", lastEvent);
        //    Assert.Equal(2, eventsReceived.Count);
        //}

        //[Fact]
        //public void TestAnonymousSubscriberClassWithPrivateSuperclass() {
        //    EventBus.Register(new PublicWithPrivateSuperClass());
        //    EventBus.Post("Hello");
        //    Assert.Equal("Hello", lastEvent);
        //    Assert.Equal(2, eventsReceived.Count);
        //}

        //[Fact]
        //public void TestSubscriberClassWithPrivateEvent()
        //{
        //    EventBus.Register(new PublicClassWithPrivateEvent(this));
        //    var privateEvent = new PrivateEvent();
        //    EventBus.Post(privateEvent);
        //    Assert.Equal(privateEvent, lastEvent);
        //    Assert.Equal(1, eventsReceived.Count);
        //}

        //[Fact]
        //public void TestSubscriberClassWithPublicAndPrivateEvent()
        //{
        //    EventBus.Register(new PublicClassWithPublicAndPrivateEvent());

        //    EventBus.Post("Hello");
        //    Assert.Equal("Hello", lastEvent);
        //    Assert.Equal(1, eventsReceived.Count);

        //    var privateEvent = new PrivateEvent();
        //    EventBus.Post(privateEvent);
        //    Assert.Equal(privateEvent, lastEvent);
        //    Assert.Equal(2, eventsReceived.Count);
        //}

        //[Fact]
        //public void TestSubscriberExtendingClassWithPrivateEvent()
        //{
        //    EventBus.Register(new PublicWithPrivateEventInSuperclass(this));
        //    var privateEvent = new PrivateEvent();
        //    EventBus.Post(privateEvent);
        //    Assert.Equal(privateEvent, lastEvent);
        //    Assert.Equal(2, eventsReceived.Count);
        //}

        private class PrivateEvent
        {
        }

        public class PublicClass : OuterTestHandlerBase
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

        internal class PrivateClass : OuterTestHandlerBase
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

        //public class PublicClassWithPrivateEvent : OuterTestHandlerBase
        //{
        //    public PublicClassWithPrivateEvent(TestBase outerTest) : base(outerTest)
        //    {
        //    }

        //    [Subscribe]
        //    public virtual void OnMessage(PrivateEvent any)
        //    {
        //        OuterTest.TrackMessage(any);
        //    }
        //}

        //public class PublicClassWithPublicAndPrivateEvent : OuterTestHandlerBase
        //{
        //    public PublicClassWithPublicAndPrivateEvent(TestBase outerTest) : base(
        //        outerTest)
        //    {
        //    }

        //    [Subscribe]
        //    public virtual void OnMessage(string any)
        //    {
        //        OuterTest.TrackMessage(any);
        //    }

        //    [Subscribe]
        //    public virtual void OnMessage(PrivateEvent any)
        //    {
        //        OuterTest.TrackMessage(any);
        //    }
        //}

        //public class PublicWithPrivateEventInSuperclass : PublicClassWithPrivateEvent
        //{
        //    public PublicWithPrivateEventInSuperclass(TestBase outerTest) : base(
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