using Snork.EventBus.Tests.Events;
using Snork.EventBus.Tests.Subscribers;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class InheritanceDisabledTest : InheritanceTestBase
    {
        public InheritanceDisabledTest(ITestOutputHelper output) : base(output)
        {
        }


        protected override void Setup()
        {
            EventBus = EventBus.Builder().WithLogger(Logger).WithEventInheritance(false).Build();
        }

        [Fact]
        public virtual void TestEventClassHierarchy()
        {
            EventBus.Register(this);

            EventBus.Post("Hello");
            Assert.Equal(0, CountObjectEvent);

            EventBus.Post(new MyInheritanceEvent());
            Assert.Equal(0, CountObjectEvent);
            Assert.Equal(1, CountMyEvent);

            EventBus.Post(new MyInheritanceEventExtended());
            Assert.Equal(0, CountObjectEvent);
            Assert.Equal(1, CountMyEvent);
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
            Assert.Equal(1, CountMyEvent);
            Assert.Equal(0, CountObjectEvent);
        }

        [Fact]
        public void TestEventInterfaceHierarchy()
        {
            EventBus.Register(this);

            EventBus.Post(new MyInheritanceEvent());
            Assert.Equal(0, CountMyEventInterface);

            EventBus.Post(new MyInheritanceEventExtended());
            Assert.Equal(0, CountMyEventInterface);
            Assert.Equal(0, CountMyEventInterfaceExtended);
        }


        [Fact]
        public void TestSubscriberClassHierarchy()
        {
            var subscriber = new InheritanceDisabledSubclassTest(Output);
            EventBus.Register(subscriber);

            EventBus.Post("Hello");
            Assert.Equal(0, subscriber.CountObjectEvent);

            EventBus.Post(new MyInheritanceEvent());
            Assert.Equal(0, subscriber.CountObjectEvent);
            Assert.Equal(0, subscriber.CountMyEvent);
            Assert.Equal(1, subscriber.CountMyEventOverridden);

            EventBus.Post(new MyInheritanceEventExtended());
            Assert.Equal(0, subscriber.CountObjectEvent);
            Assert.Equal(0, subscriber.CountMyEvent);
            Assert.Equal(1, subscriber.CountMyEventExtended);
            Assert.Equal(1, subscriber.CountMyEventOverridden);
        }

        [Fact]
        public void TestSubscriberClassHierarchyWithoutNewSubscriberMethod()
        {
            var subscriber = new InheritanceDisabledSubclassNoMethodTest(Output);
            EventBus.Register(subscriber);

            EventBus.Post("Hello");
            Assert.Equal(0, subscriber.CountObjectEvent);

            EventBus.Post(new MyInheritanceEvent());
            Assert.Equal(0, subscriber.CountObjectEvent);
            Assert.Equal(1, subscriber.CountMyEvent);

            EventBus.Post(new MyInheritanceEventExtended());
            Assert.Equal(0, subscriber.CountObjectEvent);
            Assert.Equal(1, subscriber.CountMyEvent);
            Assert.Equal(1, subscriber.CountMyEventExtended);
        }
    }
}