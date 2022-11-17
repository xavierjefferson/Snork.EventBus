namespace Snork.EventBus.Tests.Subscribers
{
    public class CancellingSubscriber : OuterTestSubscriberBase
    {
        public CancellingSubscriber(TestBase outerTest) : base(outerTest)
        {
        }

        [Subscribe]
        public virtual void OnEvent(string @event)
        {
            try
            {
                OuterTest.EventBus.CancelEventDelivery(this);
            }
            catch (EventBusException e)
            {
                OuterTest.LastException = e;
            }
        }
    }
}