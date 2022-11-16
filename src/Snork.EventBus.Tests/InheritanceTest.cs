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

using Snork.EventBus.Tests.Messages;
using Snork.EventBus.Tests.Subscribers;
using Xunit;

namespace Snork.EventBus.Tests
{
    /**
 * @author Markus Junginger, greenrobot
 */
    public class InheritanceTest : InheritanceTestBase
    {
        public InheritanceTest()
        {
            setUp();
        }

        public void setUp()
        {
            EventBus = new EventBus();
        }

        [Fact]
        public virtual void TestMessageClassHierarchy()
        {
            EventBus.Register(this);

            EventBus.Post("Hello");
            Assert.Equal(1, CountObjectMessage);

            EventBus.Post(new MyInheritanceMessage());
            Assert.Equal(2, CountObjectMessage);
            Assert.Equal(1, CountMyMessage);

            EventBus.Post(new MyInheritanceMessageExtended());
            Assert.Equal(3, CountObjectMessage);
            Assert.Equal(2, CountMyMessage);
            Assert.Equal(1, CountMyMessageExtended);
        }

        [Fact]
        public void TestMessageClassHierarchySticky()
        {
            EventBus.PostSticky("Hello");
            EventBus.PostSticky(new MyInheritanceMessage());
            EventBus.PostSticky(new MyInheritanceMessageExtended());
            EventBus.Register(new StickySubscriber(this));
            Assert.Equal(1, CountMyMessageExtended);
            Assert.Equal(2, CountMyMessage);
            Assert.Equal(3, CountObjectMessage);
        }

        [Fact]
        public void TestMessageInterfaceHierarchy()
        {
            EventBus.Register(this);

            EventBus.Post(new MyInheritanceMessage());
            Assert.Equal(1, CountMyMessageInterface);

            EventBus.Post(new MyInheritanceMessageExtended());
            Assert.Equal(2, CountMyMessageInterface);
            Assert.Equal(1, CountMyMessageInterfaceExtended);
        }


        [Fact]
        public void TestSubscriberClassHierarchy()
        {
            var subscriber = new InheritanceSubclassTest();
            EventBus.Register(subscriber);

            EventBus.Post("Hello");
            Assert.Equal(1, subscriber.CountObjectMessage);

            EventBus.Post(new MyInheritanceMessage());
            Assert.Equal(2, subscriber.CountObjectMessage);
            Assert.Equal(0, subscriber.CountMyMessage);
            Assert.Equal(1, subscriber.CountMyMessageOverwritten);

            EventBus.Post(new MyInheritanceMessageExtended());
            Assert.Equal(3, subscriber.CountObjectMessage);
            Assert.Equal(0, subscriber.CountMyMessage);
            Assert.Equal(1, subscriber.CountMyMessageExtended);
            Assert.Equal(2, subscriber.CountMyMessageOverwritten);
        }

        [Fact]
        public void TestSubscriberClassHierarchyWithoutNewSubscriberMethod()
        {
            var
                subscriber = new InheritanceSubclassNoMethodTest();
            EventBus.Register(subscriber);

            EventBus.Post("Hello");
            Assert.Equal(1, subscriber.CountObjectMessage);

            EventBus.Post(new MyInheritanceMessage());
            Assert.Equal(2, subscriber.CountObjectMessage);
            Assert.Equal(1, subscriber.CountMyMessage);

            EventBus.Post(new MyInheritanceMessageExtended());
            Assert.Equal(3, subscriber.CountObjectMessage);
            Assert.Equal(2, subscriber.CountMyMessage);
            Assert.Equal(1, subscriber.CountMyMessageExtended);
        }

        [Subscribe]
        public virtual void OnMessage(object message)
        {
            CountObjectMessage++;
        }

        [Subscribe]
        public virtual void OnMessage(MyInheritanceMessage message)
        {
            CountMyMessage++;
        }

        [Subscribe]
        public virtual void OnMessage(MyInheritanceMessageExtended message)
        {
            CountMyMessageExtended++;
        }

        [Subscribe]
        public virtual void OnMessage(MyInheritanceMessageInterface message)
        {
            CountMyMessageInterface++;
        }

        [Subscribe]
        public virtual void OnMessage(MyInheritanceMessageInterfaceExtended message)
        {
            CountMyMessageInterfaceExtended++;
        }
    }
}