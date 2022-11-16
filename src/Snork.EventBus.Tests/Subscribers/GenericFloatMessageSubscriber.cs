namespace Snork.EventBus.Tests.Subscribers
{
    public class GenericFloatMessageSubscriber : GenericNumberMessageSubscriber<float>
    {
        public GenericFloatMessageSubscriber(TestBase outerTest) : base(outerTest)
        {
        }
    }
}