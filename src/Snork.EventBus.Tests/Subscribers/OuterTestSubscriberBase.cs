namespace Snork.EventBus.Tests.Subscribers
{
    public abstract class OuterTestSubscriberBase
    {
        protected OuterTestSubscriberBase(TestBase outerTest)
        {
            OuterTest = outerTest;
        }

        public TestBase OuterTest { get; }
    }
}