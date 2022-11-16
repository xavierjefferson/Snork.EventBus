


using Snork.EventBus.Tests.Messages;
using Snork.EventBus.Tests.Subscribers;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{

    public class InheritanceDisabledTest : InheritanceTestBase
    {
        public InheritanceDisabledTest(ITestOutputHelper output) : base(output)
        {
            setUp();
        }


        public void setUp()
        {
            EventBus = EventBus.Builder().WithLogger(Logger).WithMessageInheritance(false).Build();
        }

        [Fact]
        public virtual void TestMessageClassHierarchy()
        {
            EventBus.Register(this);

            EventBus.Post("Hello");
            Assert.Equal(0, CountObjectMessage);

            EventBus.Post(new MyInheritanceMessage());
            Assert.Equal(0, CountObjectMessage);
            Assert.Equal(1, CountMyMessage);

            EventBus.Post(new MyInheritanceMessageExtended());
            Assert.Equal(0, CountObjectMessage);
            Assert.Equal(1, CountMyMessage);
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
            Assert.Equal(1, CountMyMessage);
            Assert.Equal(0, CountObjectMessage);
        }

        [Fact]
        public void TestMessageInterfaceHierarchy()
        {
            EventBus.Register(this);

            EventBus.Post(new MyInheritanceMessage());
            Assert.Equal(0, CountMyMessageInterface);

            EventBus.Post(new MyInheritanceMessageExtended());
            Assert.Equal(0, CountMyMessageInterface);
            Assert.Equal(0, CountMyMessageInterfaceExtended);
        }


        [Fact]
        public void TestSubscriberClassHierarchy()
        {
            var subscriber = new InheritanceDisabledSubclassTest(Output);
            EventBus.Register(subscriber);

            EventBus.Post("Hello");
            Assert.Equal(0, subscriber.CountObjectMessage);

            EventBus.Post(new MyInheritanceMessage());
            Assert.Equal(0, subscriber.CountObjectMessage);
            Assert.Equal(0, subscriber.CountMyMessage);
            Assert.Equal(1, subscriber.CountMyMessageOverridden);

            EventBus.Post(new MyInheritanceMessageExtended());
            Assert.Equal(0, subscriber.CountObjectMessage);
            Assert.Equal(0, subscriber.CountMyMessage);
            Assert.Equal(1, subscriber.CountMyMessageExtended);
            Assert.Equal(1, subscriber.CountMyMessageOverridden);
        }

        [Fact]
        public void TestSubscriberClassHierarchyWithoutNewSubscriberMethod()
        {
            var subscriber = new InheritanceDisabledSubclassNoMethodTest(Output);
            EventBus.Register(subscriber);

            EventBus.Post("Hello");
            Assert.Equal(0, subscriber.CountObjectMessage);

            EventBus.Post(new MyInheritanceMessage());
            Assert.Equal(0, subscriber.CountObjectMessage);
            Assert.Equal(1, subscriber.CountMyMessage);

            EventBus.Post(new MyInheritanceMessageExtended());
            Assert.Equal(0, subscriber.CountObjectMessage);
            Assert.Equal(1, subscriber.CountMyMessage);
            Assert.Equal(1, subscriber.CountMyMessageExtended);
        }
    }
}