namespace Snork.EventBus.Tests.Subscribers
{
    public class GenericFloatMessageSubscriber : GenericEnumerableMessageSubscriber<float>
    {
        public GenericFloatMessageSubscriber(TestBase outerTest) : base(outerTest)
        {
        }
    }
}