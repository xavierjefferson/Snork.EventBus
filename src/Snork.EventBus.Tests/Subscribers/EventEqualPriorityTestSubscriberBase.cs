namespace Snork.EventBus.Tests.Subscribers
{
    public abstract class EventEqualPriorityTestSubscriberBase : OuterTestSubscriberBase
    {
        protected EventEqualPriorityTestSubscriberBase(TestBase outerTest) : base(outerTest)
        {
        }

        protected bool Cancel { get; set; }
        protected int Priority { get; set; }

        protected void HandleEvent(string @event, int priority)
        {
            if (Priority == priority)
            {
                OuterTest.TrackEvent(@event);
                if (Cancel) OuterTest.EventBus.CancelEventDelivery(@event);
            }
        }
    }
}