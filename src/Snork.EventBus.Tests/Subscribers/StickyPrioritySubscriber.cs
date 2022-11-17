using Snork.EventBus.Tests.Events;

namespace Snork.EventBus.Tests.Subscribers
{
    public class StickyPrioritySubscriber : EventOrderedPriorityTestSubscriberBase
    {
        public StickyPrioritySubscriber(OrderedSubscriptionsTest outerTest) : base(outerTest)
        {
        }

        [Subscribe(priority: 1, sticky: true)]
        public virtual void OnEventP1(string @event)
        {
            HandleEvent(1, @event);
        }


        [Subscribe(priority: -1, sticky: true)]
        public virtual void OnEventM1(string @event)
        {
            HandleEvent(-1, @event);
        }

        [Subscribe(priority: 0, sticky: true)]
        public virtual void OnEventP0(string @event)
        {
            HandleEvent(0, @event);
        }

        [Subscribe(priority: 10, sticky: true)]
        public virtual void OnEventP10(string @event)
        {
            HandleEvent(10, @event);
        }

        [Subscribe(priority: -100, sticky: true)]
        public virtual void OnEventM100(string @event)
        {
            HandleEvent(-100, @event);
        }

        [Subscribe(ThreadModeEnum.Main, priority: -1, sticky: true)]
        public virtual void OnEventMainThreadM1(IntTestEvent @event)
        {
            HandleEvent(-1, @event);
        }

        [Subscribe(ThreadModeEnum.Main, true)]
        public virtual void OnEventMainThreadP0(IntTestEvent @event)
        {
            HandleEvent(SubscribeAttribute.DefaultPriority, @event);
        }

        [Subscribe(ThreadModeEnum.Main, priority: 1, sticky: true)]
        public virtual void OnEventMainThreadP1(IntTestEvent @event)
        {
            HandleEvent(1, @event);
        }

        [Subscribe(ThreadModeEnum.Background, priority: 1, sticky: true)]
        public virtual void OnEventBackgroundThreadP1(int @event)
        {
            HandleEvent(1, @event);
        }

        [Subscribe(ThreadModeEnum.Background, true)]
        public virtual void OnEventBackgroundThreadP0(int @event)
        {
            HandleEvent(SubscribeAttribute.DefaultPriority, @event);
        }

        [Subscribe(ThreadModeEnum.Background, priority: -1, sticky: true)]
        public virtual void OnEventBackgroundThreadM1(int @event)
        {
            HandleEvent(-1, @event);
        }
    }
}