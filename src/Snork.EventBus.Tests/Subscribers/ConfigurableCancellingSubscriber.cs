namespace Snork.EventBus.Tests.Subscribers
{
    public class ConfigurableCancellingSubscriber : EventEqualPriorityTestSubscriberBase
    {
        public ConfigurableCancellingSubscriber(CancelEventDeliveryTest outerTest) : base(outerTest)
        {
        }

        public ConfigurableCancellingSubscriber(int priority, bool cancel, CancelEventDeliveryTest outerTest) :
            this(outerTest)
        {
            Priority = priority;
            Cancel = cancel;
        }

        [Subscribe]
        public virtual void OnEvent(string @event)
        {
            HandleEvent(@event, 0);
        }

        [Subscribe(priority: 1)]
        public virtual void OnEvent1(string @event)
        {
            HandleEvent(@event, 1);
        }

        [Subscribe(priority: 2)]
        public virtual void OnEvent2(string @event)
        {
            HandleEvent(@event, 2);
        }

        [Subscribe(priority: 3)]
        public virtual void OnEvent3(string @event)
        {
            HandleEvent(@event, 3);
        }
    }
}