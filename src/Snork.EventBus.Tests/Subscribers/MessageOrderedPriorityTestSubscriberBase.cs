namespace Snork.EventBus.Tests.Subscribers
{
    public abstract class MessageOrderedPriorityTestSubscriberBase : OuterTestSubscriberBase
    {
        protected MessageOrderedPriorityTestSubscriberBase(TestBase outerTest) : base(outerTest)
        {
        }

        protected void HandleMessage(int priority, object message)
        {
            if (priority < OuterTest.LastPriority)
                OuterTest.Fail = $"Called priority {priority} after {OuterTest.LastPriority}";
            OuterTest.LastPriority = priority;

            OuterTest.Log($"Subscriber {priority} got: {message}");
            OuterTest.TrackMessage(message);
        }
    }
}