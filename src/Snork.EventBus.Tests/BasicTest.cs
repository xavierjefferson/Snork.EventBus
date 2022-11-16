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
        public int CountIntMessage { get; private set; }
        public int CountMyMessage { get; private set; }
        public int CountMyMessage2 { get; private set; }
        public int CountMyMessageExtended { get; private set; }
        public int CountStringMessage { get; private set; }
        public int LastIntMessage { get; private set; }

        public string LastStringMessage { get; private set; }

        [Fact]
        public void TestRegisterAndPost()
        {
            // Use an activity to test real life performance
            var stringMessageSubscriber = new StringMessageSubscriber();
            var message = "Hello";

            var stopwatch = Stopwatch.StartNew();

            EventBus.Register(stringMessageSubscriber);

            Log($"Registered in {stopwatch.Elapsed}");

            EventBus.Post(message);

            Assert.Equal(message, stringMessageSubscriber.LastStringMessage);
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
            var message = "Hello";
            EventBus.Post(message);
            Assert.Equal(message, LastStringMessage);
            Assert.Equal(message, test2.LastStringMessage);
        }

        [Fact]
        public void TestPostMultipleTimes()
        {
            EventBus.Register(this);
            var message = new MyBasicMessage();
            var count = 1000;
            var stopwatch = Stopwatch.StartNew();
            var start = DateTime.Now.Ticks;
            // Debug.startMethodTracing("testPostMultipleTimes" + count);
            for (var i = 0; i < count; i++) EventBus.Post(message);

            Log($"Posted {count} events in {stopwatch.Elapsed}");
            Assert.Equal(count, CountMyMessage);
        }

        [Fact]
        public void TestMultipleSubscribeMethodsForMessage()
        {
            EventBus.Register(this);
            var message = new MyBasicMessage();
            EventBus.Post(message);
            Assert.Equal(1, CountMyMessage);
            Assert.Equal(1, CountMyMessage2);
        }

        [Fact]
        public void TestPostAfterUnregister()
        {
            EventBus.Register(this);
            EventBus.Unregister(this);
            EventBus.Post("Hello");
            Assert.Null(LastStringMessage);
        }

        [Fact]
        public void TestRegisterAndPostTwoTypes()
        {
            EventBus.Register(this);
            EventBus.Post(42);
            EventBus.Post("Hello");
            Assert.Equal(1, CountIntMessage);
            Assert.Equal(1, CountStringMessage);
            Assert.Equal(42, LastIntMessage);
            Assert.Equal("Hello", LastStringMessage);
        }

        [Fact]
        public void TestRegisterUnregisterAndPostTwoTypes()
        {
            EventBus.Register(this);
            EventBus.Unregister(this);
            EventBus.Post(42);
            EventBus.Post("Hello");
            Assert.Equal(0, CountIntMessage);
            Assert.Equal(0, LastIntMessage);
            Assert.Equal(0, CountStringMessage);
        }

        [Fact]
        public void TestPostOnDifferentEventBus()
        {
            EventBus.Register(this);
            new EventBusBuilder().WithLogger(Logger).Build().Post("Hello");
            Assert.Equal(0, CountStringMessage);
        }

        [Fact]
        public void TestPostInMessageHandler()
        {
            var reposter = new RepostInteger(EventBus);
            EventBus.Register(reposter);
            EventBus.Register(this);
            EventBus.Post(1);
            Assert.Equal(10, CountIntMessage);
            Assert.Equal(10, LastIntMessage);
            Assert.Equal(10, reposter.CountMessage);
            Assert.Equal(10, reposter.LastMessage);
        }

        [Fact]
        public void TestHasSubscriberForMessage()
        {
            Assert.False(EventBus.HasSubscriberForMessage(typeof(string)));

            EventBus.Register(this);
            Assert.True(EventBus.HasSubscriberForMessage(typeof(string)));

            EventBus.Unregister(this);
            Assert.False(EventBus.HasSubscriberForMessage(typeof(string)));
        }

        [Fact]
        public void TestHasSubscriberForMessageSuperclass()
        {
            Assert.False(EventBus.HasSubscriberForMessage(typeof(string)));

            object subscriber = new ObjectSubscriber();
            EventBus.Register(subscriber);
            Assert.True(EventBus.HasSubscriberForMessage(typeof(string)));

            EventBus.Unregister(subscriber);
            Assert.False(EventBus.HasSubscriberForMessage(typeof(string)));
        }

        [Fact]
        public void TestHasSubscriberForMessageImplementedInterface()
        {
            Assert.False(EventBus.HasSubscriberForMessage(typeof(List<object>)));

            object subscriber = new IEnumerableSubscriber();
            EventBus.Register(subscriber);
            Assert.True(EventBus.HasSubscriberForMessage(typeof(List<object>)));
            Assert.True(EventBus.HasSubscriberForMessage(typeof(IEnumerable<object>)));

            EventBus.Unregister(subscriber);
            Assert.False(EventBus.HasSubscriberForMessage(typeof(List<object>)));
            Assert.False(EventBus.HasSubscriberForMessage(typeof(IEnumerable<object>)));
        }

        [Subscribe]
        public virtual void OnMessage(string message)
        {
            LastStringMessage = message;
            CountStringMessage++;
        }

        [Subscribe]
        public virtual void OnMessage(int message)
        {
            LastIntMessage = message;
            CountIntMessage++;
        }

        [Subscribe]
        public virtual void OnMessage(MyBasicMessage message)
        {
            CountMyMessage++;
        }

        [Subscribe]
        public virtual void OnMessage2(MyBasicMessage message)
        {
            CountMyMessage2++;
        }

        [Subscribe]
        public virtual void OnMessage(MyBasicMessageExtended message)
        {
            CountMyMessageExtended++;
        }

        public class WithIndex : BasicTest
        {
            [Fact]
            public void Dummy()
            {
            }

            public WithIndex(ITestOutputHelper output) : base(output)
            {
            }
        }

        public BasicTest(ITestOutputHelper output ) : base(output )
        {
        }
    }
}