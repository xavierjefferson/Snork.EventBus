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
using Snork.EventBus;
using Xunit;



/**
 * @author Markus Junginger, greenrobot
 */

public class EventBusBasicTest : AbstractEventBusTest
{

	public class WithIndex : EventBusBasicTest
	{
		[Fact]
		public void dummy()
		{
		}

	}

	private String lastStringEvent;
	private int countStringEvent;
	private int countIntEvent;
	private int lastIntEvent;
	private int countMyEventExtended;
	private int countMyEvent;
	private int countMyEvent2;

	[Fact]
	public void TestRegisterAndPost()
	{
		// Use an activity to test real life performance
		StringEventSubscriber stringEventSubscriber = new StringEventSubscriber();
		String @event = "Hello";

        Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		
		eventBus.Register(stringEventSubscriber);
		
		log($"Registered in {stopwatch.Elapsed}");

		eventBus.Post(@event);

		Assert.Equal(@event, stringEventSubscriber.lastStringEvent);
	}

	 
	[Fact]
	public void TestPostWithoutSubscriber()
	{
		eventBus.Post("Hello");
	}
	[Fact]
	public void TestUnregisterWithoutRegister()
	{
		// Results in a warning without throwing
		eventBus.Unregister(this);
	}

	//	// This will throw "out of memory" if subscribers are leaked
	//	[Fact]
	//	public void TestUnregisterNotLeaking()
	//	{
	//		int heapMBytes = (int)(Runtime.getRuntime().maxMemory() / (1024L * 1024L));
	//		for (int i = 0; i < heapMBytes * 2; i++)
	//		{


	//			EventBusBasicTest subscriber = new EventBusBasicTest() {
	//				byte[] expensiveObject = new byte[1024 * 1024];
	//		};
	//		eventBus.Register(subscriber);
	//		eventBus.Unregister(subscriber);
	//		log("Iteration " + i + " / max heap: " + heapMBytes);
	//	}
	//}

	[Fact]
	public void TestRegisterTwice()
	{
		eventBus.Register(this);
        Assert.Throws<EventBusException>(() =>
        {
            eventBus.Register(this);
        });
	}

	[Fact]
	public void TestIsRegistered()
	{
		Assert.False(eventBus.IsRegistered(this));
		eventBus.Register(this);
		Assert.True(eventBus.IsRegistered(this));
		eventBus.Unregister(this);
		Assert.False(eventBus.IsRegistered(this));
	}

	[Fact]
	public void TestPostWithTwoSubscriber()
	{
		EventBusBasicTest test2 = new EventBusBasicTest();
		eventBus.Register(this);
		eventBus.Register(test2);
		String @event = "Hello";
		eventBus.Post(@event);
		Assert.Equal(@event, lastStringEvent);
		Assert.Equal(@event, test2.lastStringEvent);
	}

	[Fact]
	public void TestPostMultipleTimes()
	{
		eventBus.Register(this);
		MyEvent @event = new MyEvent();
		int count = 1000;
		Stopwatch stopwatch = Stopwatch.StartNew();
		long start = System.DateTime.Now.Ticks;
		// Debug.startMethodTracing("testPostMultipleTimes" + count);
		for (int i = 0; i < count; i++)
		{
			eventBus.Post(@event);
		}
	 
		log($"Posted {count} events in {stopwatch.Elapsed}");
		Assert.Equal(count, countMyEvent);
	}

	[Fact]
	public void TestMultipleSubscribeMethodsForEvent()
	{
		eventBus.Register(this);
		MyEvent @event = new MyEvent();
		eventBus.Post(@event);
		Assert.Equal(1, countMyEvent);
		Assert.Equal(1, countMyEvent2);
	}

	[Fact]
	public void TestPostAfterUnregister()
	{
		eventBus.Register(this);
		eventBus.Unregister(this);
		eventBus.Post("Hello");
		Assert.Null(lastStringEvent);
	}

	[Fact]
	public void TestRegisterAndPostTwoTypes()
	{
		eventBus.Register(this);
		eventBus.Post(42);
		eventBus.Post("Hello");
		Assert.Equal(1, countIntEvent);
		Assert.Equal(1, countStringEvent);
		Assert.Equal(42, lastIntEvent);
		Assert.Equal("Hello", lastStringEvent);
	}

	[Fact]
	public void TestRegisterUnregisterAndPostTwoTypes()
	{
		eventBus.Register(this);
		eventBus.Unregister(this);
		eventBus.Post(42);
		eventBus.Post("Hello");
		Assert.Equal(0, countIntEvent);
		Assert.Equal(0, lastIntEvent);
		Assert.Equal(0, countStringEvent);
	}

	[Fact]
	public void TestPostOnDifferentEventBus()
	{
		eventBus.Register(this);
		new EventBus().Post("Hello");
		Assert.Equal(0, countStringEvent);
	}

	[Fact]
	public void TestPostInEventHandler()
	{
		RepostInteger reposter = new RepostInteger(eventBus);
		eventBus.Register(reposter);
		eventBus.Register(this);
		eventBus.Post(1);
		Assert.Equal(10, countIntEvent);
		Assert.Equal(10, lastIntEvent);
		Assert.Equal(10, reposter.countEvent);
		Assert.Equal(10, reposter.lastEvent);
	}

	[Fact]
	public void TestHasSubscriberForEvent()
	{
		Assert.False(eventBus.HasSubscriberForEvent(typeof(string)));

		eventBus.Register(this);
		Assert.True(eventBus.HasSubscriberForEvent(typeof(string)));

		eventBus.Unregister(this);
		Assert.False(eventBus.HasSubscriberForEvent(typeof(string)));
	}

	[Fact]
	public void TestHasSubscriberForEventSuperclass()
	{
		Assert.False(eventBus.HasSubscriberForEvent(typeof(string)));

		Object subscriber = new ObjectSubscriber();
		eventBus.Register(subscriber);
		Assert.True(eventBus.HasSubscriberForEvent(typeof(string)));

		eventBus.Unregister(subscriber);
		Assert.False(eventBus.HasSubscriberForEvent(typeof(string)));
	}

	[Fact]
	public void TestHasSubscriberForEventImplementedInterface()
	{
		Assert.False(eventBus.HasSubscriberForEvent(typeof(string)));

		Object subscriber = new DateTimeSubscriber();
		eventBus.Register(subscriber);
		Assert.True(eventBus.HasSubscriberForEvent(typeof(DateTime)));
		Assert.True(eventBus.HasSubscriberForEvent(typeof(string)));

		eventBus.Unregister(subscriber);
		Assert.False(eventBus.HasSubscriberForEvent(typeof(DateTime)));
		Assert.False(eventBus.HasSubscriberForEvent(typeof(string)));
	}

	[Subscribe()]
	public void onEvent(String @event)
	{
		lastStringEvent = @event;
		countStringEvent++;
	}

	[Subscribe()]
	public void onEvent(int @event)
	{
		lastIntEvent = @event;
		countIntEvent++;
	}

	[Subscribe()]
	public void onEvent(MyEvent @event)
	{
		countMyEvent++;
	}

	[Subscribe()]
	public void onEvent2(MyEvent @event)
	{
		countMyEvent2++;
	}

	[Subscribe()]
	public void onEvent(MyEventExtended @event)
	{
		countMyEventExtended++;
	}

	public class StringEventSubscriber
	{
		public String lastStringEvent;

		[Subscribe()]
		public void onEvent(String @event)
		{
			lastStringEvent = @event;
		}
	}

	public class DateTimeSubscriber
	{
		[Subscribe()]
		public void onEvent(DateTime @event)
		{
		}
	}

	public class ObjectSubscriber
	{
		[Subscribe()]
		public void onEvent(Object @event)
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
        public int lastEvent;
		public int countEvent;

        public RepostInteger(EventBus eventBus)
        {
            _eventBus = eventBus;
        }
		[Subscribe()]
		public void onEvent(int @event)
		{
			lastEvent = @event;
			countEvent++;
			Assert.Equal(countEvent, @event);

			if (@event < 10)
			{
				int countIntEventBefore = countEvent;
				_eventBus.Post(@event + 1);
				// All our Post calls will just enqueue the @event, so check count is unchanged
				Assert.Equal(countIntEventBefore, countIntEventBefore);
			}
		}
	}

}
