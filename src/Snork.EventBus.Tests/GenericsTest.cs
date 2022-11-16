
using System;
using Snork.EventBus.Tests.Messages;
using Snork.EventBus.Tests.Subscribers;
using Xunit;

namespace Snork.EventBus.Tests
{
    public class GenericsTest : TestBase
    {
        [Fact]
        public void TestGenericMessageAndSubscriber()
        {
            var genericSubscriber = new GenericMessageSubscriber<int>(this);
            EventBus.Register(genericSubscriber);
            EventBus.Post(new GenericMessage<int>());
            AssertMessageCount(1);
        }

        [Fact]
        public void TestGenericMessageAndSubscriber_TypeErasure()
        {
            var genericSubscriber = new FullGenericMessageSubscriber<IntTestMessage>(this);
            EventBus.Register(genericSubscriber);
            EventBus.Post(new IntTestMessage(42));
            EventBus.Post("Type erasure!");
            AssertMessageCount(2);
        }

        [Fact]
        public void TestGenericMessageAndSubscriber_BaseType()
        {
            var genericSubscriber = new GenericNumberMessageSubscriber<float>(this);
            EventBus.Register(genericSubscriber);
            EventBus.Post(Convert.ToSingle(42));
            EventBus.Post(Convert.ToDouble(23));
            AssertMessageCount(2);
            EventBus.Post("Not the same base type");
            AssertMessageCount(2);
        }

        [Fact]
        public void TestGenericMessageAndSubscriber_Subclass()
        {
            var genericSubscriber = new GenericFloatMessageSubscriber(this);
            EventBus.Register(genericSubscriber);
            EventBus.Post(Convert.ToSingle(42));
            EventBus.Post(Convert.ToDouble(77));
            AssertMessageCount(2);
            EventBus.Post("Not the same base type");
            AssertMessageCount(2);
        }
    }

    public class GenericMessage<T>
    {
        public T Value { get; }
    }
}