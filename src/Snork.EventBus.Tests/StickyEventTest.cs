/*
 * Copyright (C) 2012-2016 Markus Junginger, greenrobot (http://greenrobot.org)
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

using System.Threading;
using Xunit;

namespace Snork.EventBus.Tests
{
    /**
 * @author Markus Junginger, greenrobot
 */
    public class StickyEventTest : TestBase
    {
        [Fact]
        public void TestPostSticky()
        {
            EventBus.PostSticky("Sticky");
            EventBus.Register(this);
            Assert.Equal("Sticky", lastEvent);
            Assert.Equal(Thread.CurrentThread, lastThread);
        }

        [Fact]
        public void TestPostStickyTwoEvents()
        {
            EventBus.PostSticky("Sticky");
            EventBus.PostSticky(new IntTestEvent(7));
            EventBus.Register(this);
            Assert.Equal(2, eventCount);
        }

        [Fact]
        public void TestPostStickyTwoSubscribers()
        {
            EventBus.PostSticky("Sticky");
            EventBus.PostSticky(new IntTestEvent(7));
            EventBus.Register(this);
            var subscriber2 = new StickyIntTestSubscriber(this);
            EventBus.Register(subscriber2);
            Assert.Equal(3, eventCount);

            EventBus.PostSticky("Sticky");
            Assert.Equal(4, eventCount);

            EventBus.PostSticky(new IntTestEvent(8));
            Assert.Equal(6, eventCount);
        }

        [Fact]
        public void TestPostStickyRegisterNonSticky()
        {
            EventBus.PostSticky("Sticky");
            EventBus.Register(new NonStickySubscriber(this));
            Assert.Null(lastEvent);
            Assert.Equal(0, eventCount);
        }

        [Fact]
        public void TestPostNonStickyRegisterSticky()
        {
            EventBus.Post("NonSticky");
            EventBus.Register(this);
            Assert.Null(lastEvent);
            Assert.Equal(0, eventCount);
        }

        [Fact]
        public void TestPostStickyTwice()
        {
            EventBus.PostSticky("Sticky");
            EventBus.PostSticky("NewSticky");
            EventBus.Register(this);
            Assert.Equal("NewSticky", lastEvent);
        }

        [Fact]
        public void TestPostStickyThenPostNormal()
        {
            EventBus.PostSticky("Sticky");
            EventBus.Post("NonSticky");
            EventBus.Register(this);
            Assert.Equal("Sticky", lastEvent);
        }

        [Fact]
        public void TestPostStickyWithRegisterAndUnregister()
        {
            EventBus.Register(this);
            EventBus.PostSticky("Sticky");
            Assert.Equal("Sticky", lastEvent);

            EventBus.Unregister(this);
            EventBus.Register(this);
            Assert.Equal("Sticky", lastEvent);
            Assert.Equal(2, eventCount);

            EventBus.PostSticky("NewSticky");
            Assert.Equal(3, eventCount);
            Assert.Equal("NewSticky", lastEvent);

            EventBus.Unregister(this);
            EventBus.Register(this);
            Assert.Equal(4, eventCount);
            Assert.Equal("NewSticky", lastEvent);
        }

        [Fact]
        public void TestPostStickyAndGet()
        {
            EventBus.PostSticky("Sticky");
            Assert.Equal("Sticky", EventBus.GetStickyMessage(typeof(string)));
        }

        [Fact]
        public void TestPostStickyRemoveClass()
        {
            EventBus.PostSticky("Sticky");
            EventBus.RemoveStickyMessage(typeof(string));
            Assert.Null(EventBus.GetStickyMessage(typeof(string)));
            EventBus.Register(this);
            Assert.Null(lastEvent);
            Assert.Equal(0, eventCount);
        }

        [Fact]
        public void TestPostStickyRemoveEvent()
        {
            EventBus.PostSticky("Sticky");
            Assert.True(EventBus.RemoveStickyMessage("Sticky"));
            Assert.Null(EventBus.GetStickyMessage(typeof(string)));
            EventBus.Register(this);
            Assert.Null(lastEvent);
            Assert.Equal(0, eventCount);
        }

        [Fact]
        public void TestPostStickyRemoveAll()
        {
            EventBus.PostSticky("Sticky");
            EventBus.PostSticky(new IntTestEvent(77));
            EventBus.RemoveAllStickyMessages();
            Assert.Null(EventBus.GetStickyMessage(typeof(string)));
            Assert.Null(EventBus.GetStickyMessage(typeof(IntTestEvent)));
            EventBus.Register(this);
            Assert.Null(lastEvent);
            Assert.Equal(0, eventCount);
        }

        [Fact]
        public void TestRemoveStickyEventInSubscriber()
        {
            EventBus.Register(new RemoveStickySubscriber(this));
            EventBus.PostSticky("Sticky");
            EventBus.Register(this);
            Assert.Null(lastEvent);
            Assert.Equal(0, eventCount);
            Assert.Null(EventBus.GetStickyMessage(typeof(string)));
        }

        [Subscribe(sticky: true)]
        public virtual void OnMessage(string message)
        {
            TrackMessage(message);
        }

        [Subscribe(sticky: true)]
        public virtual void OnMessage(IntTestEvent message)
        {
            TrackMessage(message);
        }

        public class RemoveStickySubscriber : OuterTestHandlerBase
        {
            public RemoveStickySubscriber(TestBase outerTest) : base(outerTest)
            {
            }

            [Subscribe(sticky: true)]
            public virtual void OnMessage(string message)
            {
                OuterTest.EventBus.RemoveStickyMessage(message);
            }
        }

        public class NonStickySubscriber : OuterTestHandlerBase
        {
            public NonStickySubscriber(TestBase outerTest) : base(outerTest)
            {
            }

            [Subscribe]
            public virtual void OnMessage(string message)
            {
                OuterTest.TrackMessage(message);
            }

            [Subscribe]
            public virtual void OnMessage(IntTestEvent message)
            {
                OuterTest.TrackMessage(message);
            }
        }

        public class StickyIntTestSubscriber : OuterTestHandlerBase
        {
            public StickyIntTestSubscriber(TestBase outerTest) : base(outerTest)
            {
            }

            [Subscribe(sticky: true)]
            public virtual void OnMessage(IntTestEvent message)
            {
                OuterTest.TrackMessage(message);
            }
        }
    }
}