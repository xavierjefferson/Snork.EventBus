using Snork.EventBus.Tests.Events;
using Snork.EventBus.Tests.Subscribers;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class InheritanceTest : InheritanceTestBase
    {
        public InheritanceTest(ITestOutputHelper output) : base(output)
        {
        }

        protected override void Setup()
        {
            EventBus = new EventBusBuilder().WithLogger(Logger).Build();
        }

        [Fact]
        public virtual void TestEventClassHierarchy()
        {
            EventBus.Register(this);

            EventBus.Post("Hello");
            Assert.Equal(1, CountObjectEvent);

            EventBus.Post(new MyInheritanceEvent());
            Assert.Equal(2, CountObjectEvent);
            Assert.Equal(1, CountMyEvent);

            EventBus.Post(new MyInheritanceEventExtended());
            Assert.Equal(3, CountObjectEvent);
            Assert.Equal(2, CountMyEvent);
            Assert.Equal(1, CountMyEventExtended);
        }

        [Fact]
        public void TestEventClassHierarchySticky()
        {
            EventBus.PostSticky("Hello");
            EventBus.PostSticky(new MyInheritanceEvent());
            EventBus.PostSticky(new MyInheritanceEventExtended());
            EventBus.Register(new StickySubscriber(this));
            Assert.Equal(1, CountMyEventExtended);
            Assert.Equal(2, CountMyEvent);
            Assert.Equal(3, CountObjectEvent);
        }

        [Fact]
        public void TestEventInterfaceHierarchy()
        {
            EventBus.Register(this);

            EventBus.Post(new MyInheritanceEvent());
            Assert.Equal(1, CountMyEventInterface);

            EventBus.Post(new MyInheritanceEventExtended());
            Assert.Equal(2, CountMyEventInterface);
            Assert.Equal(1, CountMyEventInterfaceExtended);
        }


        [Fact]
        public void TestSubscriberClassHierarchy()
        {
            var subscriber = new InheritanceSubclassTest(Output);
            EventBus.Register(subscriber);

            EventBus.Post("Hello");
            Assert.Equal(1, subscriber.CountObjectEvent);

            EventBus.Post(new MyInheritanceEvent());
            Assert.Equal(2, subscriber.CountObjectEvent);
            Assert.Equal(0, subscriber.CountMyEvent);
            Assert.Equal(1, subscriber.CountMyEventOverwritten);

            EventBus.Post(new MyInheritanceEventExtended());
            Assert.Equal(3, subscriber.CountObjectEvent);
            Assert.Equal(0, subscriber.CountMyEvent);
            Assert.Equal(1, subscriber.CountMyEventExtended);
            Assert.Equal(2, subscriber.CountMyEventOverwritten);
        }

        [Fact]
        public void TestSubscriberClassHierarchyWithoutNewSubscriberMethod()
        {
            var
                subscriber = new InheritanceSubclassNoMethodTest(Output);
            EventBus.Register(subscriber);

            EventBus.Post("Hello");
            Assert.Equal(1, subscriber.CountObjectEvent);

            EventBus.Post(new MyInheritanceEvent());
            Assert.Equal(2, subscriber.CountObjectEvent);
            Assert.Equal(1, subscriber.CountMyEvent);

            EventBus.Post(new MyInheritanceEventExtended());
            Assert.Equal(3, subscriber.CountObjectEvent);
            Assert.Equal(2, subscriber.CountMyEvent);
            Assert.Equal(1, subscriber.CountMyEventExtended);
        }
    }
}