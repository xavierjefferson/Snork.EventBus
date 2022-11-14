namespace Snork.EventBus.Tests
{
    public abstract class OuterTestHandlerBase
    {
        protected OuterTestHandlerBase(TestBase outerTest)
        {
            OuterTest = outerTest;
        }

        public TestBase OuterTest { get; }

        protected void HandleEvent(int priority, object message)
        {
            if (priority > OuterTest.LastPriority)
                OuterTest.Fail = $"Called priority {priority} after {OuterTest.LastPriority}";
            OuterTest.LastPriority = priority;

            OuterTest.Log($"Subscriber {priority} got: {message}");
            OuterTest.TrackMessage(message);
        }
    }
}