using System;
using System.Collections.Generic;
using System.Threading;
using Snork.EventBus.Interfaces;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class RegistrationRacingTest : TestBase
    {
        // On a Nexus 5, bad synchronization always failed on the first iteration or went well completely.
        // So a high number of iterations do not guarantee a better probability of finding bugs.
        private static readonly int Iterations = LongTests ? 1000 : 10;
        private static readonly int ThreadCount = 16;

        //private readonly Executor threadPool = Executors.newCachedThreadPool();
        private volatile CountdownEvent _canUnregisterLatch;
        private volatile CountdownEvent _registeredLatch;

        private volatile CountdownEvent _startLatch;
        private volatile CountdownEvent _unregisteredLatch;

        public RegistrationRacingTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void TestRacingRegistrations()
        {
            for (var i = 0; i < Iterations; i++)
            {
                _startLatch = new CountdownEvent(ThreadCount);
                _registeredLatch = new CountdownEvent(ThreadCount);
                _canUnregisterLatch = new CountdownEvent(1);
                _unregisteredLatch = new CountdownEvent(ThreadCount);

                var threads = StartThreads();
                _registeredLatch.Wait();
                EventBus.Post("42");
                _canUnregisterLatch.Signal();
                for (var t = 0; t < ThreadCount; t++)
                {
                    var eventCount = threads[t].EventCount;
                    if (eventCount != 1)
                        Assert.True(false,
                            $"Failed in iteration {i}: thread #{t} has event count of {eventCount}");
                }

                // Wait for threads to be done
                _unregisteredLatch.Wait();
            }
        }

        private List<SubscriberThread> StartThreads()
        {
            var e = new ExecutorService();
            var threads = new List<SubscriberThread>(ThreadCount);
            for (var i = 0; i < ThreadCount; i++)
            {
                var thread = new SubscriberThread(this);
                e.Execute(thread);
                threads.Add(thread);
            }

            return threads;
        }

        public class SubscriberThread : IRunnable
        {
            private readonly RegistrationRacingTest _outer;
            private volatile int _eventCount;

            public SubscriberThread(RegistrationRacingTest outer)
            {
                _outer = outer;
            }

            public int EventCount
            {
                get => _eventCount;
                set => _eventCount = value;
            }

            public void Run()
            {
                _outer.CountDownAndAwaitLatch(_outer._startLatch, 10);
                _outer.EventBus.Register(this);
                _outer._registeredLatch.Signal();
                try
                {
                    _outer._canUnregisterLatch.Wait();
                }
                catch (OperationCanceledException e)
                {
                    throw new Exception("", e);
                }

                _outer.EventBus.Unregister(this);
                _outer._unregisteredLatch.Signal();
            }

            [Subscribe]
            public virtual void OnEvent(string @event)
            {
                EventCount++;
            }
        }
    }
}