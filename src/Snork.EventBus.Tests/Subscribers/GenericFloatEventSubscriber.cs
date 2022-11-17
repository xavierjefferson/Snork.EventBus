namespace Snork.EventBus.Tests.Subscribers
{
    public class GenericFloatEventSubscriber : GenericEnumerableEventSubscriber<float>
    {
        public GenericFloatEventSubscriber(TestBase outerTest) : base(outerTest)
        {
        }
    }
}