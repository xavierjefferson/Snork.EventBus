using Snork.EventBus.Tests.Subscribers;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class FallbackToReflectionTest : TestBase
    {
        //public class PublicWithPrivateSuperClass : PrivateClass
        //{
        //    [Subscribe]
        //    public virtual void OnEvent(string any)
        //    {
        //        OuterTest.TrackEvent(any);
        //    }
        //}

        //public class PublicClassWithPrivateEvent : OuterTestSubscriberBase
        //{
        //    public PublicClassWithPrivateEvent(TestBase outerTest) : base(outerTest)
        //    {
        //    }

        //    [Subscribe]
        //    public virtual void OnEvent(PrivateEvent any)
        //    {
        //        OuterTest.TrackEvent(any);
        //    }
        //}

        //public class PublicClassWithPublicAndPrivateEvent : OuterTestSubscriberBase
        //{
        //    public PublicClassWithPublicAndPrivateEvent(TestBase outerTest) : base(
        //        outerTest)
        //    {
        //    }

        //    [Subscribe]
        //    public virtual void OnEvent(string any)
        //    {
        //        OuterTest.TrackEvent(any);
        //    }

        //    [Subscribe]
        //    public virtual void OnEvent(PrivateEvent any)
        //    {
        //        OuterTest.TrackEvent(any);
        //    }
        //}

        //public class PublicWithPrivateEventInSuperclass : PublicClassWithPrivateEvent
        //{
        //    public PublicWithPrivateEventInSuperclass(TestBase outerTest) : base(
        //        outerTest)
        //    {
        //    }

        //    [Subscribe]
        //    public virtual void OnEvent(object any)
        //    {
        //        OuterTest.TrackEvent(any);
        //    }
        //}
        public FallbackToReflectionTest(ITestOutputHelper output) : base(output)
        {
        }
        //public FallbackToReflectionTest() : base(true)
        //{
        //}

        //[Fact]
        //public void TestAnonymousSubscriberClass() {
        //    object subscriber = new object() {
        //        [Subscribe()]            public virtual void OnEvent(string @event) {
        //        trackEvent(@event);
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
        //        [Subscribe()]            public virtual void OnEvent(string @event) {
        //        trackEvent(@event);
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

        public class PublicClass : OuterTestSubscriberBase
        {
            public PublicClass(TestBase outerTest) : base(outerTest)
            {
            }

            [Subscribe]
            public virtual void OnEvent(object any)
            {
                OuterTest.TrackEvent(any);
            }
        }

        internal class PrivateClass : OuterTestSubscriberBase
        {
            public PrivateClass(TestBase outerTest) : base(outerTest)
            {
            }

            [Subscribe]
            public virtual void OnEvent(object any)
            {
                OuterTest.TrackEvent(any);
            }
        }
    }
}