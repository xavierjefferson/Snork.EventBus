using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public abstract class TestBase : LoggingTestBase
    {
        /// <summary>
        ///     Activates long(er) running tests e.g. testing multi-threading more thoroughly.
        /// </summary>
        protected const bool LongTests = true;

        public string Fail;

        protected volatile object LastEvent;
        protected volatile Thread LastThread;


        protected TestBase(ITestOutputHelper output, bool collectEventsReceived = true) : base(output)
        {
            if (collectEventsReceived)
                EventsReceived = new ConcurrentBag<object>();
            else
                EventsReceived = null;
        }

        protected ConcurrentBag<object> EventsReceived { get; }

        protected int EventCount => EventsReceived?.Count ?? 0;


        public Exception LastException { get; set; }
        public int LastPriority { get; set; } = int.MinValue;


        protected override void Setup()
        {
           
            EventBus = new EventBusBuilder().WithLogger(Logger).Build();
        }


        protected void WaitForEventCount(int expectedCount, int maxMillis)
        {
            for (var i = 0; i < maxMillis; i++)
            {
                var currentCount = EventCount;
                if (currentCount == expectedCount)
                    break;
                Assert.False(currentCount > expectedCount,
                    $"Current count ({currentCount}) is already higher than expected count ({expectedCount})");

                try
                {
                    Thread.Sleep(1);
                }
                catch (OperationCanceledException e)
                {
                    throw new InvalidOperationException("", e);
                }
            }

            Assert.Equal(expectedCount, EventCount);
        }

        public void TrackEvent(object @event)
        {
            LastEvent = @event;
            LastThread = Thread.CurrentThread;
            EventsReceived?.Add(@event);
        }

        protected void AssertEventCount(int expectedEventCount)
        {
            Assert.Equal(expectedEventCount, EventCount);
        }

        protected void CountDownAndAwaitLatch(CountdownEvent latch, long seconds)
        {
            latch.Signal();
            latch.Wait(TimeSpan.FromSeconds(seconds));
        }

        protected void AwaitLatch(CountdownEvent latch, long seconds)
        {
            try
            {
                Assert.True(latch.Wait(TimeSpan.FromSeconds(seconds)));
            }
            catch (OperationCanceledException e)
            {
                throw new Exception("", e);
            }
        }

        public void Log(string msg)
        {
            EventBus.Logger.LogDebug(msg);
        }

        public void Log(string msg, Exception e)
        {
            EventBus.Logger.LogDebug(e, msg);
        }
    }
}