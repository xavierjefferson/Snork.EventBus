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
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public abstract class TestBase : LoggingTestBase
    {
       


        /**
     * Activates long(er) running tests e.g. testing multi-threading more thoroughly.
     */
        protected static readonly bool LONG_TESTS = false;

        public string Fail;

        protected volatile object LastMessage;
        protected volatile Thread LastThread;


        protected TestBase(ITestOutputHelper output, bool collectMessagesReceived = true) : base(output)
        {
           
            if (collectMessagesReceived)
                MessagesReceived = new ConcurrentBag<object>();
            else
                MessagesReceived = null;
            setUpBase();
        }

        protected ConcurrentBag<object> MessagesReceived { get; }

        protected int MessageCount => MessagesReceived?.Count ?? 0;

        public EventBus EventBus { get; set; }
        public Exception LastException { get; set; }
        public int LastPriority { get; set; } = int.MinValue;


        public void setUpBase()
        {
            EventBus.ClearCaches();
            GetInitialEventBus();
        }

        public virtual void GetInitialEventBus()
        {
            EventBus = new EventBusBuilder().WithLogger(Logger).Build();
        }

        protected void WaitForMessageCount(int expectedCount, int maxMillis)
        {
            for (var i = 0; i < maxMillis; i++)
            {
                var currentCount = MessageCount;
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

            Assert.Equal(expectedCount, MessageCount);
        }

        public void TrackMessage(object message)
        {
            LastMessage = message;
            LastThread = Thread.CurrentThread;
            MessagesReceived?.Add(message);
        }

        protected void AssertMessageCount(int expectedMessageCount)
        {
            Assert.Equal(expectedMessageCount, MessageCount);
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