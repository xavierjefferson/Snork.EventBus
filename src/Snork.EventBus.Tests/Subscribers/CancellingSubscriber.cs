namespace Snork.EventBus.Tests.Subscribers
{
    public class CancellingSubscriber : OuterTestSubscriberBase
    {
        public CancellingSubscriber(TestBase outerTest) : base(outerTest)
        {
        }

        [Subscribe]
        public virtual void OnMessage(string message)
        {
            try
            {
                OuterTest.EventBus.CancelMessageDelivery(this);
            }
            catch (EventBusException e)
            {
                OuterTest.LastException = e;
            }
        }
    }
}