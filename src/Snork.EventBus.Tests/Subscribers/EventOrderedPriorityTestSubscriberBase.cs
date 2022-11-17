namespace Snork.EventBus.Tests.Subscribers
{
    public abstract class EventOrderedPriorityTestSubscriberBase : OuterTestSubscriberBase
    {
        protected EventOrderedPriorityTestSubscriberBase(TestBase outerTest) : base(outerTest)
        {
        }

        protected void HandleEvent(int priority, object @event)
        {
            if (priority < OuterTest.LastPriority)
                OuterTest.Fail = $"Called priority {priority} after {OuterTest.LastPriority}";
            OuterTest.LastPriority = priority;

            OuterTest.Log($"Subscriber {priority} got: {@event}");
            OuterTest.TrackEvent(@event);
        }
    }
}