/*
 * Copyright (C) 2012-2017 Markus Junginger, greenrobot (http://greenrobot.org)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Diagnostics;
using Xunit;

namespace Snork.EventBus.Tests
{
    /**
 * @author Markus Junginger, greenrobot
 */
    public class BasicTest : TestBase
    {
        private int countIntEvent;
        private int countMyEvent;
        private int countMyEvent2;
        private int countMyEventExtended;
        private int countStringEvent;
        private int lastIntEvent;

        private string lastStringEvent;

        [Fact]
        public void TestRegisterAndPost()
        {
            // Use an activity to test real life performance
            var stringEventSubscriber = new StringEventSubscriber();
            var message = "Hello";

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            EventBus.Register(stringEventSubscriber);

            Log($"Registered in {stopwatch.Elapsed}");

            EventBus.Post(message);

            Assert.Equal(message, stringEventSubscriber.lastStringEvent);
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
            var test2 = new BasicTest();
            EventBus.Register(this);
            EventBus.Register(test2);
            var message = "Hello";
            EventBus.Post(message);
            Assert.Equal(message, lastStringEvent);
            Assert.Equal(message, test2.lastStringEvent);
        }

        [Fact]
        public void TestPostMultipleTimes()
        {
            EventBus.Register(this);
            var message = new MyEvent();
            var count = 1000;
            var stopwatch = Stopwatch.StartNew();
            var start = DateTime.Now.Ticks;
            // Debug.startMethodTracing("testPostMultipleTimes" + count);
            for (var i = 0; i < count; i++) EventBus.Post(message);

            Log($"Posted {count} events in {stopwatch.Elapsed}");
            Assert.Equal(count, countMyEvent);
        }

        [Fact]
        public void TestMultipleSubscribeMethodsForEvent()
        {
            EventBus.Register(this);
            var message = new MyEvent();
            EventBus.Post(message);
            Assert.Equal(1, countMyEvent);
            Assert.Equal(1, countMyEvent2);
        }

        [Fact]
        public void TestPostAfterUnregister()
        {
            EventBus.Register(this);
            EventBus.Unregister(this);
            EventBus.Post("Hello");
            Assert.Null(lastStringEvent);
        }

        [Fact]
        public void TestRegisterAndPostTwoTypes()
        {
            EventBus.Register(this);
            EventBus.Post(42);
            EventBus.Post("Hello");
            Assert.Equal(1, countIntEvent);
            Assert.Equal(1, countStringEvent);
            Assert.Equal(42, lastIntEvent);
            Assert.Equal("Hello", lastStringEvent);
        }

        [Fact]
        public void TestRegisterUnregisterAndPostTwoTypes()
        {
            EventBus.Register(this);
            EventBus.Unregister(this);
            EventBus.Post(42);
            EventBus.Post("Hello");
            Assert.Equal(0, countIntEvent);
            Assert.Equal(0, lastIntEvent);
            Assert.Equal(0, countStringEvent);
        }

        [Fact]
        public void TestPostOnDifferentEventBus()
        {
            EventBus.Register(this);
            new EventBus().Post("Hello");
            Assert.Equal(0, countStringEvent);
        }

        [Fact]
        public void TestPostInEventHandler()
        {
            var reposter = new RepostInteger(EventBus);
            EventBus.Register(reposter);
            EventBus.Register(this);
            EventBus.Post(1);
            Assert.Equal(10, countIntEvent);
            Assert.Equal(10, lastIntEvent);
            Assert.Equal(10, reposter.countEvent);
            Assert.Equal(10, reposter.lastEvent);
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

            object subscriber = new objectSubscriber();
            EventBus.Register(subscriber);
            Assert.True(EventBus.HasSubscriberForEvent(typeof(string)));

            EventBus.Unregister(subscriber);
            Assert.False(EventBus.HasSubscriberForEvent(typeof(string)));
        }

        [Fact]
        public void TestHasSubscriberForEventImplementedInterface()
        {
            Assert.False(EventBus.HasSubscriberForEvent(typeof(string)));

            object subscriber = new DateTimeSubscriber();
            EventBus.Register(subscriber);
            Assert.True(EventBus.HasSubscriberForEvent(typeof(DateTime)));
            Assert.True(EventBus.HasSubscriberForEvent(typeof(string)));

            EventBus.Unregister(subscriber);
            Assert.False(EventBus.HasSubscriberForEvent(typeof(DateTime)));
            Assert.False(EventBus.HasSubscriberForEvent(typeof(string)));
        }

        [Subscribe]
        public virtual void OnMessage(string message)
        {
            lastStringEvent = message;
            countStringEvent++;
        }

        [Subscribe]
        public virtual void OnMessage(int message)
        {
            lastIntEvent = message;
            countIntEvent++;
        }

        [Subscribe]
        public virtual void OnMessage(MyEvent message)
        {
            countMyEvent++;
        }

        [Subscribe]
        public virtual void OnMessage2(MyEvent message)
        {
            countMyEvent2++;
        }

        [Subscribe]
        public virtual void OnMessage(MyEventExtended message)
        {
            countMyEventExtended++;
        }

        public class WithIndex : BasicTest
        {
            [Fact]
            public void dummy()
            {
            }
        }

        public class StringEventSubscriber
        {
            public string lastStringEvent;

            [Subscribe]
            public virtual void OnMessage(string message)
            {
                lastStringEvent = message;
            }
        }

        public class DateTimeSubscriber
        {
            [Subscribe]
            public virtual void OnMessage(DateTime message)
            {
            }
        }

        public class objectSubscriber
        {
            [Subscribe]
            public virtual void OnMessage(object message)
            {
            }
        }

        public class MyEvent
        {
        }

        public class MyEventExtended : MyEvent
        {
        }

        public class RepostInteger
        {
            private readonly EventBus _eventBus;
            public int countEvent;
            public int lastEvent;

            public RepostInteger(EventBus eventBus)
            {
                _eventBus = eventBus;
            }

            [Subscribe]
            public virtual void OnMessage(int message)
            {
                lastEvent = message;
                countEvent++;
                Assert.Equal(countEvent, message);

                if (message < 10)
                {
                    var countIntEventBefore = countEvent;
                    _eventBus.Post(message + 1);
                    // All our Post calls will just enqueue the @message, so check count is unchanged
                    Assert.Equal(countIntEventBefore, countIntEventBefore);
                }
            }
        }
    }
}