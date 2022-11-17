using Snork.EventBus.Tests.Subscribers;
using Xunit;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class SubscriberLegalTest : TestBase
    {
        //    public class Static {
        //        @Subscribe
        //        public static void OnEvent(string @event) {
        //        }
        //    }
        public SubscriberLegalTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void TestSubscriberLegal()
        {
            EventBus.Register(this);
            EventBus.Post("42");
            EventBus.Unregister(this);
            Assert.Equal(1, EventCount);
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
            Assert.Equal(1, EventCount);
        }

        [Subscribe]
        public virtual void OnEvent(string @event)
        {
            TrackEvent(@event);
        }

        //    public class NotPublic {
        //        @Subscribe
        //        void OnEvent(string @event) {
        //        }
        //    }

        public abstract class Abstract : OuterTestSubscriberBase
        {
            protected Abstract(TestBase outerTest) : base(outerTest)
            {
            }

            [Subscribe]
            public abstract void OnEvent(string @event);
        }

        public class AbstractImpl : Abstract
        {
            public AbstractImpl(TestBase outerTest) : base(outerTest)
            {
            }


            [Subscribe]
            public override void OnEvent(string @event)
            {
                OuterTest.TrackEvent(@event);
            }
        }
    }
}