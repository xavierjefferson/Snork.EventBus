
using System;
using System.Collections.Generic;
using Snork.EventBus.Tests.Messages;
using Snork.EventBus.Tests.Subscribers;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class GenericsTest : TestBase
    {
        [Fact]
        public void TestGenericMessageAndSubscriber()
        {
            var genericSubscriber = new GenericMessageSubscriber<IntTestMessage>(this);
            EventBus.Register(genericSubscriber);
            EventBus.Post(new IntTestMessage(10));
            AssertMessageCount(1);
        }

        [Fact]
        public void TestGenericMessageAndSubscriber_TypeErasure()
        {
            var genericSubscriber = new FullGenericMessageSubscriber<IntTestMessage>(this);
            EventBus.Register(genericSubscriber);
            EventBus.Post(new IntTestMessage(42));
            EventBus.Post("Type erasure!");
            AssertMessageCount(1);
        }

        [Fact]
        public void TestGenericMessageAndSubscriber_BaseType()
        {
            var genericSubscriber = new GenericEnumerableMessageSubscriber<float>(this);
            EventBus.Register(genericSubscriber);
            EventBus.Post(new[] { Convert.ToSingle(42) });
            EventBus.Post(new List<float> { 23f });
            AssertMessageCount(2);
            EventBus.Post("Not the same base type");
            AssertMessageCount(2);
        }

        [Fact]
        public void TestGenericMessageAndSubscriber_Subclass()
        {
            var genericSubscriber = new GenericFloatMessageSubscriber(this);
            EventBus.Register(genericSubscriber);
            EventBus.Post(new[] { Convert.ToSingle(42) });
            EventBus.Post(new List<float> { 23f });
            AssertMessageCount(2);
            EventBus.Post("Not the same base type");
            AssertMessageCount(2);
        }

        public GenericsTest(ITestOutputHelper output) : base(output)
        {
        }
    }
}