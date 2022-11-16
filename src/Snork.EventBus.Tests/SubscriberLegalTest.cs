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

using Snork.EventBus.Tests.Subscribers;
using Xunit;

namespace Snork.EventBus.Tests
{
    /**
 * @author Markus Junginger, greenrobot
 */
    public class SubscriberLegalTest : TestBase
    {
        [Fact]
        public void TestSubscriberLegal()
        {
            EventBus.Register(this);
            EventBus.Post("42");
            EventBus.Unregister(this);
            Assert.Equal(1, MessageCount);
        }

        // With Build time verification, some of these tests are obsolete (and cause problems during Build)
        //    public void TestSubscriberNotPublic() {
        //        try {
        //            eventBus.Register(new NotPublic());
        //            Assert.True(false, "Registration of ilegal subscriber successful");
        //        } catch (EventBusException e) {
        //            // Expected
        //        }
        //    }

        //    public void TestSubscriberStatic() {
        //        try {
        //            eventBus.Register(new Static());
        //            Assert.True(false, "Registration of ilegal subscriber successful");
        //        } catch (EventBusException e) {
        //            // Expected
        //        }
        //    }

        public void TestSubscriberLegalAbstract()
        {
            EventBus.Register(new AbstractImpl(this));

            EventBus.Post("42");
            Assert.Equal(1, MessageCount);
        }

        [Subscribe]
        public virtual void OnMessage(string message)
        {
            TrackMessage(message);
        }

        //    public class NotPublic {
        //        @Subscribe
        //        void OnMessage(string message) {
        //        }
        //    }

        public abstract class Abstract : OuterTestSubscriberBase
        {
            protected Abstract(TestBase outerTest) : base(outerTest)
            {
            }

            [Subscribe]
            public abstract void OnMessage(string message);
        }

        public class AbstractImpl : Abstract
        {
            public AbstractImpl(TestBase outerTest) : base(outerTest)
            {
            }


            [Subscribe]
            public override void OnMessage(string message)
            {
                OuterTest.TrackMessage(message);
            }
        }

        //    public class Static {
        //        @Subscribe
        //        public static void OnMessage(string message) {
        //        }
        //    }
    }
}