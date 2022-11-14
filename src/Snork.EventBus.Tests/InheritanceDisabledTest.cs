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


using Xunit;

namespace Snork.EventBus.Tests
{
    /**
 * @author Markus Junginger, greenrobot
 */
    public class InheritanceDisabledTest
    {
        protected EventBus eventBus;

        public InheritanceDisabledTest()
        {
            setUp();
        }

        protected int countMyEventExtended { get; set; }
        protected int countMyEvent { get; set; }
        protected int countobjectEvent { get; set; }
        private int countMyEventInterface { get; set; }
        private int countMyEventInterfaceExtended { get; set; }

        public void setUp()
        {
            eventBus = EventBus.Builder().WithMessageInheritance(false).Build();
        }

        [Fact]
        public virtual void TestEventClassHierarchy()
        {
            eventBus.Register(this);

            eventBus.Post("Hello");
            Assert.Equal(0, countobjectEvent);

            eventBus.Post(new MyEvent());
            Assert.Equal(0, countobjectEvent);
            Assert.Equal(1, countMyEvent);

            eventBus.Post(new MyEventExtended());
            Assert.Equal(0, countobjectEvent);
            Assert.Equal(1, countMyEvent);
            Assert.Equal(1, countMyEventExtended);
        }

        [Fact]
        public void TestEventClassHierarchySticky()
        {
            eventBus.PostSticky("Hello");
            eventBus.PostSticky(new MyEvent());
            eventBus.PostSticky(new MyEventExtended());
            eventBus.Register(new StickySubscriber(this));
            Assert.Equal(1, countMyEventExtended);
            Assert.Equal(1, countMyEvent);
            Assert.Equal(0, countobjectEvent);
        }

        [Fact]
        public void TestEventInterfaceHierarchy()
        {
            eventBus.Register(this);

            eventBus.Post(new MyEvent());
            Assert.Equal(0, countMyEventInterface);

            eventBus.Post(new MyEventExtended());
            Assert.Equal(0, countMyEventInterface);
            Assert.Equal(0, countMyEventInterfaceExtended);
        }


        [Fact]
        public void TestSubscriberClassHierarchy()
        {
            var
                subscriber = new InheritanceDisabledSubclassTest();
            eventBus.Register(subscriber);

            eventBus.Post("Hello");
            Assert.Equal(0, subscriber.countobjectEvent);

            eventBus.Post(new MyEvent());
            Assert.Equal(0, subscriber.countobjectEvent);
            Assert.Equal(0, subscriber.countMyEvent);
            Assert.Equal(1, subscriber.countMyEventOverwritten);

            eventBus.Post(new MyEventExtended());
            Assert.Equal(0, subscriber.countobjectEvent);
            Assert.Equal(0, subscriber.countMyEvent);
            Assert.Equal(1, subscriber.countMyEventExtended);
            Assert.Equal(1, subscriber.countMyEventOverwritten);
        }

        [Fact]
        public void TestSubscriberClassHierarchyWithoutNewSubscriberMethod()
        {
            var
                subscriber = new InheritanceDisabledSubclassNoMethodTest();
            eventBus.Register(subscriber);

            eventBus.Post("Hello");
            Assert.Equal(0, subscriber.countobjectEvent);

            eventBus.Post(new MyEvent());
            Assert.Equal(0, subscriber.countobjectEvent);
            Assert.Equal(1, subscriber.countMyEvent);

            eventBus.Post(new MyEventExtended());
            Assert.Equal(0, subscriber.countobjectEvent);
            Assert.Equal(1, subscriber.countMyEvent);
            Assert.Equal(1, subscriber.countMyEventExtended);
        }

        [Subscribe]
        public virtual void OnMessage(object message)
        {
            countobjectEvent++;
        }

        [Subscribe]
        public virtual void OnMessage(MyEvent message)
        {
            countMyEvent++;
        }

        [Subscribe]
        public virtual void OnMessage(MyEventExtended message)
        {
            countMyEventExtended++;
        }

        [Subscribe]
        public virtual void OnMessage(MyEventInterface message)
        {
            countMyEventInterface++;
        }

        [Subscribe]
        public virtual void OnMessage(MyEventInterfaceExtended message)
        {
            countMyEventInterfaceExtended++;
        }

        public interface MyEventInterface
        {
        }

        public class MyEvent : MyEventInterface
        {
        }

        public interface MyEventInterfaceExtended : MyEventInterface
        {
        }

        public class MyEventExtended : MyEvent, MyEventInterfaceExtended
        {
        }

        public class StickySubscriber
        {
            private readonly InheritanceDisabledTest _outer;

            public StickySubscriber(InheritanceDisabledTest outer)
            {
                _outer = outer;
            }

            [Subscribe(sticky: true)]
            public virtual void OnMessage(object message)
            {
                _outer.countobjectEvent++;
            }

            [Subscribe(sticky: true)]
            public virtual void OnMessage(MyEvent message)
            {
                _outer.countMyEvent++;
            }

            [Subscribe(sticky: true)]
            public virtual void OnMessage(MyEventExtended message)
            {
                _outer.countMyEventExtended++;
            }

            [Subscribe(sticky: true)]
            public virtual void OnMessage(MyEventInterface message)
            {
                _outer.countMyEventInterface++;
            }

            [Subscribe(sticky: true)]
            public virtual void OnMessage(MyEventInterfaceExtended message)
            {
                _outer.countMyEventInterfaceExtended++;
            }
        }
    }
}