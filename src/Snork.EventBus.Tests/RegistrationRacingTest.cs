using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    /**
 * @author Markus Junginger, greenrobot
 */
    public class RegistrationRacingTest : TestBase
    {
        // On a Nexus 5, bad synchronization always failed on the first iteration or went well completely.
        // So a high number of iterations do not guarantee a better probability of finding bugs.
        private static readonly int ITERATIONS = LONG_TESTS ? 1000 : 10;
        private static readonly int THREAD_COUNT = 16;

        //private readonly Executor threadPool = Executors.newCachedThreadPool();
        private volatile CountdownEvent canUnregisterLatch;
        private volatile CountdownEvent registeredLatch;

        private volatile CountdownEvent startLatch;
        private volatile CountdownEvent unregisteredLatch;

        [Fact]
        public void TestRacingRegistrations()
        {
            for (var i = 0; i < ITERATIONS; i++)
            {
                startLatch = new CountdownEvent(THREAD_COUNT);
                registeredLatch = new CountdownEvent(THREAD_COUNT);
                canUnregisterLatch = new CountdownEvent(1);
                unregisteredLatch = new CountdownEvent(THREAD_COUNT);

                var threads = startThreads();
                registeredLatch.Wait();
                EventBus.Post("42");
                canUnregisterLatch.Signal();
                for (var t = 0; t < THREAD_COUNT; t++)
                {
                    var eventCount = threads[t].MessageCount;
                    if (eventCount != 1)
                        Assert.True(false,
                            "Failed in iteration " + i + ": thread #" + t + " has message count of " + eventCount);
                }

                // Wait for threads to be done
                unregisteredLatch.Wait();
            }
        }

        private List<SubscriberThread> startThreads()
        {
            var e = new ExecutorService();
            var threads = new List<SubscriberThread>(THREAD_COUNT);
            for (var i = 0; i < THREAD_COUNT; i++)
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

            public int MessageCount
            {
                get => _eventCount;
                set => _eventCount = value;
            }

            public void Run()
            {
                _outer.CountDownAndAwaitLatch(_outer.startLatch, 10);
                _outer.EventBus.Register(this);
                _outer.registeredLatch.Signal();
                try
                {
                    _outer.canUnregisterLatch.Wait();
                }
                catch (OperationCanceledException e)
                {
                    throw new Exception("", e);
                }

                _outer.EventBus.Unregister(this);
                _outer.unregisteredLatch.Signal();
            }

            [Subscribe]
            public virtual void OnMessage(string message)
            {
                MessageCount++;
            }
        }

        public RegistrationRacingTest(ITestOutputHelper output) : base(output)
        {
        }
    }
}