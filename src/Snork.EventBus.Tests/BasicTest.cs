using System;
using System.Collections.Generic;
using System.Diagnostics;
using Snork.EventBus.Tests.Subscribers;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class BasicTest : TestBase
    {
        public BasicTest(ITestOutputHelper output) : base(output)
        {
        }

        public int CountIntEvent { get; private set; }
        public int CountMyEvent { get; private set; }
        public int CountMyEvent2 { get; private set; }
        public int CountMyEventExtended { get; private set; }
        public int CountStringEvent { get; private set; }
        public int LastIntEvent { get; private set; }

        public string LastStringEvent { get; private set; }

        [Fact]
        public void TestRegisterAndPost()
        {
            // Use an activity to test real life performance
            var stringEventSubscriber = new StringEventSubscriber();
            var @event = "Hello";

            var stopwatch = Stopwatch.StartNew();

            EventBus.Register(stringEventSubscriber);

            Log($"Registered in {stopwatch.Elapsed}");

            EventBus.Post(@event);

            Assert.Equal(@event, stringEventSubscriber.LastStringEvent);
        }


        [Fact]
        public void TestPostWithoutSubscriber()
        {
            EventBus.Post("Hello");
        }

        [Fact]
        public void TestUnregisterWithoutRegister()
        {
            // Results in a warning without throwing
            EventBus.Unregister(this);
        }

        //	// This will throw "out of memory" if subscribers are leaked
        //	[Fact]
        //	public void TestUnregisterNotLeaking()
        //	{
        //		int heapMBytes = (int)(Runtime.getRuntime().maxMemory() / (1024L * 1024L));
        //		for (int i = 0; i < heapMBytes * 2; i++)
        //		{


        //			BasicTest subscriber = new BasicTest() {
        //				byte[] expensiveobject = new byte[1024 * 1024];
        //		};
        //		eventBus.Register(subscriber);
        //		eventBus.Unregister(subscriber);
        //		log("Iteration " + i + " / max heap: " + heapMBytes);
        //	}
        //}

        [Fact]
        public void TestRegisterTwice()
        {
            EventBus.Register(this);
            Assert.Throws<EventBusException>(() => { EventBus.Register(this); });
        }

        [Fact]
        public void TestIsRegistered()
        {
            Assert.False(EventBus.IsRegistered(this));
            EventBus.Register(this);
            Assert.True(EventBus.IsRegistered(this));
            EventBus.Unregister(this);
            Assert.False(EventBus.IsRegistered(this));
        }

        [Fact]
        public void TestPostWithTwoSubscriber()
        {
            var test2 = new BasicTest(Output);
            EventBus.Register(this);
            EventBus.Register(test2);
            var @event = "Hello";
            EventBus.Post(@event);
            Assert.Equal(@event, LastStringEvent);
            Assert.Equal(@event, test2.LastStringEvent);
        }

        [Fact]
        public void TestPostMultipleTimes()
        {
            EventBus.Register(this);
            var @event = new MyBasicEvent();
            var count = 1000;
            var stopwatch = Stopwatch.StartNew();
            var start = DateTime.Now.Ticks;
            // Debug.startMethodTracing("testPostMultipleTimes" + count);
            for (var i = 0; i < count; i++) EventBus.Post(@event);

            Log($"Posted {count} events in {stopwatch.Elapsed}");
            Assert.Equal(count, CountMyEvent);
        }

        [Fact]
        public void TestMultipleSubscribeMethodsForEvent()
        {
            EventBus.Register(this);
            var @event = new MyBasicEvent();
            EventBus.Post(@event);
            Assert.Equal(1, CountMyEvent);
            Assert.Equal(1, CountMyEvent2);
        }

        [Fact]
        public void TestPostAfterUnregister()
        {
            EventBus.Register(this);
            EventBus.Unregister(this);
            EventBus.Post("Hello");
            Assert.Null(LastStringEvent);
        }

        [Fact]
        public void TestRegisterAndPostTwoTypes()
        {
            EventBus.Register(this);
            EventBus.Post(42);
            EventBus.Post("Hello");
            Assert.Equal(1, CountIntEvent);
            Assert.Equal(1, CountStringEvent);
            Assert.Equal(42, LastIntEvent);
            Assert.Equal("Hello", LastStringEvent);
        }

        [Fact]
        public void TestRegisterUnregisterAndPostTwoTypes()
        {
            EventBus.Register(this);
            EventBus.Unregister(this);
            EventBus.Post(42);
            EventBus.Post("Hello");
            Assert.Equal(0, CountIntEvent);
            Assert.Equal(0, LastIntEvent);
            Assert.Equal(0, CountStringEvent);
        }

        [Fact]
        public void TestPostOnDifferentEventBus()
        {
            EventBus.Register(this);
            new EventBusBuilder().WithLogger(Logger).Build().Post("Hello");
            Assert.Equal(0, CountStringEvent);
        }

        [Fact]
        public void TestPostInEventHandler()
        {
            var reposter = new RepostInteger(EventBus);
            EventBus.Register(reposter);
            EventBus.Register(this);
            EventBus.Post(1);
            Assert.Equal(10, CountIntEvent);
            Assert.Equal(10, LastIntEvent);
            Assert.Equal(10, reposter.CountEvent);
            Assert.Equal(10, reposter.LastEvent);
        }

        [Fact]
        public void TestHasSubscriberForEvent()
        {
            Assert.False(EventBus.HasSubscriberForEvent(typeof(string)));

            EventBus.Register(this);
            Assert.True(EventBus.HasSubscriberForEvent(typeof(string)));

            EventBus.Unregister(this);
            Assert.False(EventBus.HasSubscriberForEvent(typeof(string)));
        }

        [Fact]
        public void TestHasSubscriberForEventSuperclass()
        {
            Assert.False(EventBus.HasSubscriberForEvent(typeof(string)));

            object subscriber = new ObjectSubscriber();
            EventBus.Register(subscriber);
            Assert.True(EventBus.HasSubscriberForEvent(typeof(string)));

            EventBus.Unregister(subscriber);
            Assert.False(EventBus.HasSubscriberForEvent(typeof(string)));
        }

        [Fact]
        public void TestHasSubscriberForEventImplementedInterface()
        {
            Assert.False(EventBus.HasSubscriberForEvent(typeof(List<object>)));

            object subscriber = new IEnumerableSubscriber();
            EventBus.Register(subscriber);
            Assert.True(EventBus.HasSubscriberForEvent(typeof(List<object>)));
            Assert.True(EventBus.HasSubscriberForEvent(typeof(IEnumerable<object>)));

            EventBus.Unregister(subscriber);
            Assert.False(EventBus.HasSubscriberForEvent(typeof(List<object>)));
            Assert.False(EventBus.HasSubscriberForEvent(typeof(IEnumerable<object>)));
        }

        [Subscribe]
        public virtual void OnEvent(string @event)
        {
            LastStringEvent = @event;
            CountStringEvent++;
        }

        [Subscribe]
        public virtual void OnEvent(int @event)
        {
            LastIntEvent = @event;
            CountIntEvent++;
        }

        [Subscribe]
        public virtual void OnEvent(MyBasicEvent @event)
        {
            CountMyEvent++;
        }

        [Subscribe]
        public virtual void OnEvent2(MyBasicEvent @event)
        {
            CountMyEvent2++;
        }

        [Subscribe]
        public virtual void OnEvent(MyBasicEventExtended @event)
        {
            CountMyEventExtended++;
        }

        public class WithIndex : BasicTest
        {
            public WithIndex(ITestOutputHelper output) : base(output)
            {
            }

            [Fact]
            public void Dummy()
            {
            }
        }
    }
}