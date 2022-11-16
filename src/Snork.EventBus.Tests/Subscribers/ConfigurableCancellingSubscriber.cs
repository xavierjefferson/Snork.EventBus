namespace Snork.EventBus.Tests.Subscribers
{
    public class ConfigurableCancellingSubscriber : MessageEqualPriorityTestSubscriberBase
    {
        public ConfigurableCancellingSubscriber(CancelMessageDeliveryTest outerTest) : base(outerTest)
        {
        }

        public ConfigurableCancellingSubscriber(int priority, bool cancel, CancelMessageDeliveryTest outerTest) : this(outerTest)
        {
            Priority = priority;
            Cancel = cancel;
        }

        [Subscribe]
        public virtual void OnMessage(string message)
        {
            HandleMessage(message, 0);
        }

        [Subscribe(priority: 1)]
        public virtual void OnMessage1(string message)
        {
            HandleMessage(message, 1);
        }

        [Subscribe(priority: 2)]
        public virtual void OnMessage2(string message)
        {
            HandleMessage(message, 2);
        }

        [Subscribe(priority: 3)]
        public virtual void OnMessage3(string message)
        {
            HandleMessage(message, 3);
        }
    }
}