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
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Snork.EventBus;
using Xunit;

/**
 * @author Markus Junginger, greenrobot
 */
public abstract class AbstractEventBusTest
{
    /**
     * Activates long(er) running tests e.g. testing multi-threading more thoroughly.
     */
    protected static readonly bool LONG_TESTS = false;

    protected readonly List<object> eventsReceived;

    protected EventBus eventBus;

    protected volatile int eventCount;

    protected volatile object lastEvent;
    protected volatile Thread lastThread;

    public AbstractEventBusTest() : this(false)
    {
    }

    public AbstractEventBusTest(bool collectEventsReceived)
    {
        if (collectEventsReceived)
            eventsReceived = new List<object>();
        else
            eventsReceived = null;
        setUpBase();
    }


    public void setUpBase()
    {
        EventBus.ClearCaches();
        eventBus = new EventBus();
    }

    protected void waitForEventCount(int expectedCount, int maxMillis)
    {
        for (var i = 0; i < maxMillis; i++)
        {
            var currentCount = eventCount;
            if (currentCount == expectedCount)
                break;
            if (currentCount > expectedCount)
                Assert.True(false,
                    $"Current count ({currentCount}) is already higher than expected count ({expectedCount})");
            else
                try
                {
                    Thread.Sleep(1);
                }
                catch (OperationCanceledException e)
                {
                    throw new InvalidOperationException("", e);
                }
        }

        Assert.Equal(expectedCount, eventCount);
    }

    protected void trackEvent(object @event)
    {
        lastEvent = @event;
        lastThread = Thread.CurrentThread;
        if (eventsReceived != null) eventsReceived.Add(@event);
        // Must the the last one because we wait for this
        Interlocked.Increment(ref eventCount);
    }

    protected void assertEventCount(int expectedEventCount)
    {
        Assert.Equal(expectedEventCount, eventCount);
    }

    protected void countDownAndAwaitLatch(CountdownEvent latch, long seconds)
    {
        latch.AddCount(-1);
        latch.Wait(TimeSpan.FromSeconds(seconds));
    }

    protected void awaitLatch(CountdownEvent latch, long seconds)
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

    protected void log(string msg)
    {
        eventBus.Logger.LogDebug(msg);
    }

    protected void log(string msg, Exception e)
    {
        eventBus.Logger.LogDebug(e, msg);
    }
}