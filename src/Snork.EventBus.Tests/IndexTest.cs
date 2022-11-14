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

using Snork.EventBus.Meta;
using Xunit;

namespace Snork.EventBus.Tests
{
    public class IndexTest
    {
        private string value;

        /**
         * Ensures the index is actually used and no reflection fall-back kicks in.
         */
        [Fact]
        public void TestManualIndexWithoutAnnotation()
        {
            SubscriberInfoIndex index = new SubscriberInfoIndex
            {
                Override
                public SubscriberInfo getSubscriberInfo(Class<?> subscriberClass) {
                Assert.Equal(typeof(IndexTest), subscriberClass);
                SubscriberMethodInfo[] methodInfos = {
                new SubscriberMethodInfo("someMethodWithoutAnnotation", typeof(string))
            };
            return new SimpleSubscriberInfo(typeof(IndexTest), false, methodInfos);
            }
            };

            EventBus eventBus = EventBus.Builder().addIndex(index).Build();
            eventBus.Register(this);
            eventBus.Post("Yepp");
            eventBus.Unregister(this);
            Assert.Equal("Yepp", value);
        }

        public void someMethodWithoutAnnotation(string value)
        {
            this.value = value;
        }
    }
}