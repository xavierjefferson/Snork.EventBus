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
using Snork.EventBus.Tests.Subscribers;
using Xunit;

namespace Snork.EventBus.Tests
{
    /**
 * @author Markus Junginger, greenrobot
 */
    public class SubscriberExceptionTest : TestBase
    {
        [Fact]
        public void TestSubscriberExceptionMessage()
        {
            EventBus = EventBus.Builder().WithLogSubscriberExceptions(false).Build();
            EventBus.Register(this);
            EventBus.Post("Foo");

            AssertMessageCount(1);
            Assert.Equal(typeof(SubscriberExceptionMessage), LastMessage.GetType());
            var exMessage = (SubscriberExceptionMessage)LastMessage;
            Assert.Equal("Foo", exMessage.OriginalMessage);
            Assert.Same(this, exMessage.OriginalSubscriber);
            Assert.Equal("Bar", exMessage.Exception.Message);
        }

        [Fact]
        public void TestBadExceptionSubscriber()
        {
            EventBus = EventBus.Builder().WithLogSubscriberExceptions(false).Build();
            EventBus.Register(this);
            EventBus.Register(new BadExceptionSubscriber());
            EventBus.Post("Foo");
            AssertMessageCount(1);
        }

        [Subscribe]
        public virtual void OnMessage(string message)
        {
            throw new Exception("Bar");
        }

        [Subscribe]
        public virtual void OnMessage(SubscriberExceptionMessage message)
        {
            TrackMessage(message);
        }
    }
}