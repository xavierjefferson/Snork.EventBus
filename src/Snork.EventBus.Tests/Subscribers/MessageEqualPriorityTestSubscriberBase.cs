namespace Snork.EventBus.Tests.Subscribers
{
    public abstract class MessageEqualPriorityTestSubscriberBase : OuterTestSubscriberBase
    {
        protected MessageEqualPriorityTestSubscriberBase(TestBase outerTest) : base(outerTest)
        {
        }

        protected bool Cancel { get; set; }
        protected int Priority { get; set; }

        protected void HandleMessage(string message, int priority)
        {
            if (Priority == priority)
            {
                OuterTest.TrackMessage(message);
                if (Cancel) OuterTest.EventBus.CancelMessageDelivery(message);
            }
        }
    }
}