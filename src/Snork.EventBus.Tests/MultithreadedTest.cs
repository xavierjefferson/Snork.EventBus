using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xunit;

namespace Snork.EventBus.Tests
{
    public class MultithreadedTest : TestBase
    {
        private static readonly int COUNT = LONG_TESTS ? 100000 : 1000;
        private readonly AtomicInteger countIntegerEvent = new AtomicInteger();
        private readonly AtomicInteger countIntTestEvent = new AtomicInteger();
        private readonly AtomicInteger countobjectEvent = new AtomicInteger();

        private readonly AtomicInteger countStringEvent = new AtomicInteger();
        private int lastIntegerEvent;

        private IntTestEvent lastIntTestEvent;

        private string lastStringEvent;

        [Fact]
        public void TestPost01Thread()
        {
            runThreadsSingleEventType(1);
        }

        [Fact]
        public void TestPost04Threads()
        {
            runThreadsSingleEventType(4);
        }

        [Fact]
        public void TestPost40Threads()
        {
            runThreadsSingleEventType(40);
        }

        [Fact]
        public void TestPostMixedEventType01Thread()
        {
            runThreadsMixedEventType(1);
        }

        [Fact]
        public void TestPostMixedEventType04Threads()
        {
            runThreadsMixedEventType(4);
        }

        [Fact]
        public void TestPostMixedEventType40Threads()
        {
            runThreadsMixedEventType(40);
        }

        private void runThreadsSingleEventType(int threadCount)
        {
            var iterations = COUNT / threadCount;
            EventBus.Register(this);

            var latch = new CountdownEvent(threadCount + 1);
            var threads = startThreads(latch, threadCount, iterations, "Hello");
            var time = triggerAndWaitForThreads(threads, latch);

            Log(threadCount + " threads posted " + iterations + " events each in " + time + "ms");

            waitForEventCount(COUNT * 2, 5000);

            Assert.Equal("Hello", lastStringEvent);
            var expectedCount = threadCount * iterations;
            Assert.Equal(expectedCount, countStringEvent.intValue());
            Assert.Equal(expectedCount, countobjectEvent.intValue());
        }

        private void runThreadsMixedEventType(int threadCount)
        {
            runThreadsMixedEventType(COUNT, threadCount);
        }

        private void runThreadsMixedEventType(int count, int threadCount)
        {
            EventBus.Register(this);
            var eventTypeCount = 3;
            var iterations = count / threadCount / eventTypeCount;

            var latch = new CountdownEvent(eventTypeCount * threadCount + 1);
            var threadsString = startThreads(latch, threadCount, iterations, "Hello");
            var threadsInteger = startThreads(latch, threadCount, iterations, 42);
            var threadsIntTestEvent = startThreads(latch, threadCount, iterations, new IntTestEvent(7));

            var threads = new List<PosterThread>();
            threads.AddRange(threadsString);
            threads.AddRange(threadsInteger);
            threads.AddRange(threadsIntTestEvent);
            var time = triggerAndWaitForThreads(threads, latch);

            Log(threadCount * eventTypeCount + " mixed threads posted " + iterations + " events each in "
                + time + "ms");

            var expectedCountEach = threadCount * iterations;
            var expectedCountTotal = expectedCountEach * eventTypeCount * 2;
            waitForEventCount(expectedCountTotal, 5000);

            Assert.Equal("Hello", lastStringEvent);
            Assert.Equal(42, lastIntegerEvent.intValue());
            Assert.Equal(7, lastIntTestEvent.value);

            Assert.Equal(expectedCountEach, countStringEvent.intValue());
            Assert.Equal(expectedCountEach, countIntegerEvent.intValue());
            Assert.Equal(expectedCountEach, countIntTestEvent.intValue());

            Assert.Equal(expectedCountEach * eventTypeCount, countobjectEvent.intValue());
        }

        private long triggerAndWaitForThreads(List<PosterThread> threads, CountdownEvent latch)
        {
            while (latch.CurrentCount != 1)
                // Let all other threads prepare and ensure this one is the last 
                Thread.Sleep(1);

            var stopWatch = Stopwatch.StartNew();

            latch.AddCount(-1);
            foreach (var thread in threads) thread.join();
            return stopWatch.ElapsedMilliseconds;
        }

        private List<PosterThread> startThreads(CountdownEvent latch, int threadCount, int iterations,
            object eventToPost)
        {
            var threads = new List<PosterThread>(threadCount);
            for (var i = 0; i < threadCount; i++)
            {
                var thread = new PosterThread(latch, iterations, eventToPost);
                thread.start();
                threads.add(thread);
            }

            return threads;
        }

        [Subscribe(ThreadModeEnum.Background)]
        public void OnMessageBackgroundThread(string message)
        {
            lastStringEvent = message;
            countStringEvent.incrementAndGet();
            TrackMessage(message);
        }

        [Subscribe(ThreadModeEnum.Main)]
        public void OnMessageMainThread(int message)
        {
            lastIntegerEvent = message;
            countIntegerEvent.incrementAndGet();
            TrackMessage(message);
        }

        [Subscribe(ThreadModeEnum.Async)]
        public void OnMessageAsync(IntTestEvent message)
        {
            countIntTestEvent.incrementAndGet();
            lastIntTestEvent = message;
            TrackMessage(message);
        }

        [Subscribe]
        public void OnMessage(object message)
        {
            countobjectEvent.incrementAndGet();
            TrackMessage(message);
        }

        private class PosterThread : Thread
        {
            private readonly object eventToPost;
            private readonly int iterations;

            private readonly CountdownEvent startLatch;

            public PosterThread(CountdownEvent latch, int iterations, object eventToPost)
            {
                startLatch = latch;
                this.iterations = iterations;
                this.eventToPost = eventToPost;
            }

            Override

            public void run()
            {
                startLatch.AddCount(-1);
                try
                {
                    startLatch.Wait();
                }
                catch (InterruptedException e)
                {
                    Log("Unexpected interrupt", e);
                }

                for (var i = 0; i < iterations; i++) EventBus.Post(eventToPost);
            }
        }
    }
}