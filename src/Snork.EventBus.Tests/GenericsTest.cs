using System;
using System.Collections.Generic;
using Snork.EventBus.Tests.Events;
using Snork.EventBus.Tests.Subscribers;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class GenericsTest : TestBase
    {
        public GenericsTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void TestGenericEventAndSubscriber()
        {
            var genericSubscriber = new GenericEventSubscriber<IntTestEvent>(this);
            EventBus.Register(genericSubscriber);
            EventBus.Post(new IntTestEvent(10));
            AssertEventCount(1);
        }

        [Fact]
        public void TestGenericEventAndSubscriber_TypeErasure()
        {
            var genericSubscriber = new FullGenericEventSubscriber<IntTestEvent>(this);
            EventBus.Register(genericSubscriber);
            EventBus.Post(new IntTestEvent(42));
            EventBus.Post("Type erasure!");
            AssertEventCount(1);
        }

        [Fact]
        public void TestGenericEventAndSubscriber_BaseType()
        {
            var genericSubscriber = new GenericEnumerableEventSubscriber<float>(this);
            EventBus.Register(genericSubscriber);
            EventBus.Post(new[] { Convert.ToSingle(42) });
            EventBus.Post(new List<float> { 23f });
            AssertEventCount(2);
            EventBus.Post("Not the same base type");
            AssertEventCount(2);
        }

        [Fact]
        public void TestGenericEventAndSubscriber_Subclass()
        {
            var genericSubscriber = new GenericFloatEventSubscriber(this);
            EventBus.Register(genericSubscriber);
            EventBus.Post(new[] { Convert.ToSingle(42) });
            EventBus.Post(new List<float> { 23f });
            AssertEventCount(2);
            EventBus.Post("Not the same base type");
            AssertEventCount(2);
        }
    }
}