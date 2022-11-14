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

using System;
using Xunit;

namespace Snork.EventBus.Tests
{
    public class GenericsTest : TestBase
    {
        [Fact]
        public void TestGenericEventAndSubscriber()
        {
            var genericSubscriber = new GenericEventSubscriber<int>(this);
            EventBus.Register(genericSubscriber);
            EventBus.Post(new GenericEvent<int>());
            assertEventCount(1);
        }

        [Fact]
        public void TestGenericEventAndSubscriber_TypeErasure()
        {
            var genericSubscriber = new FullGenericEventSubscriber<IntTestEvent>(this);
            EventBus.Register(genericSubscriber);
            EventBus.Post(new IntTestEvent(42));
            EventBus.Post("Type erasure!");
            assertEventCount(2);
        }

        [Fact]
        public void TestGenericEventAndSubscriber_BaseType()
        {
            var genericSubscriber = new GenericNumberEventSubscriber<float>(this);
            EventBus.Register(genericSubscriber);
            EventBus.Post(Convert.ToSingle(42));
            EventBus.Post(Convert.ToDouble(23));
            assertEventCount(2);
            EventBus.Post("Not the same base type");
            assertEventCount(2);
        }

        [Fact]
        public void TestGenericEventAndSubscriber_Subclass()
        {
            var genericSubscriber = new GenericFloatEventSubscriber(this);
            EventBus.Register(genericSubscriber);
            EventBus.Post(Convert.ToSingle(42));
            EventBus.Post(Convert.ToDouble(77));
            assertEventCount(2);
            EventBus.Post("Not the same base type");
            assertEventCount(2);
        }

        public class GenericEvent<T>
        {
            private T value;
        }

        public class GenericEventSubscriber<T> : OuterTestHandlerBase
        {
            public GenericEventSubscriber(TestBase outerTest) : base(outerTest)
            {
            }

            [Subscribe]
            public void onGenericEvent(GenericEvent<T> message)
            {
                OuterTest.TrackMessage(message);
            }
        }

        public class FullGenericEventSubscriber<T> : OuterTestHandlerBase
        {
            public FullGenericEventSubscriber(TestBase outerTest) : base(outerTest)
            {
            }

            [Subscribe]
            public void onGenericEvent(T message)
            {
                OuterTest.TrackMessage(message);
            }
        }

        public class GenericNumberEventSubscriber<T> : OuterTestHandlerBase //where T : IntTestEvent
        {
            public GenericNumberEventSubscriber(TestBase outerTest) : base(outerTest)
            {
            }

            [Subscribe]
            public void onGenericEvent(T message)
            {
                OuterTest.TrackMessage(message);
            }
        }

        public class GenericFloatEventSubscriber : GenericNumberEventSubscriber<float>
        {
            public GenericFloatEventSubscriber(TestBase outerTest) : base(outerTest)
            {
            }
        }
    }
}