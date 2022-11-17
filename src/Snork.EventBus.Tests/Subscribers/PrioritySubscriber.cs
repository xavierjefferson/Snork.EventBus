using Snork.EventBus.Tests.Events;

namespace Snork.EventBus.Tests.Subscribers
{
    public class PrioritySubscriber : EventOrderedPriorityTestSubscriberBase
    {
        public PrioritySubscriber(TestBase text) : base(text)
        {
        }

        [Subscribe(priority: 1)]
        public virtual void OnEventP1(string @event)
        {
            HandleEvent(1, @event);
        }

        [Subscribe(priority: -1)]
        public virtual void OnEventM1(string @event)
        {
            HandleEvent(-1, @event);
        }

        [Subscribe(priority: 0)]
        public virtual void OnEventP0(string @event)
        {
            HandleEvent(0, @event);
        }

        [Subscribe(priority: 10)]
        public virtual void OnEventP10(string @event)
        {
            HandleEvent(10, @event);
        }

        [Subscribe(priority: -100)]
        public virtual void OnEventM100(string @event)
        {
            HandleEvent(-100, @event);
        }


        [Subscribe(ThreadModeEnum.Main, priority: -1)]
        public virtual void OnEventMainThreadM1(IntTestEvent @event)
        {
            HandleEvent(-1, @event);
        }

        [Subscribe(ThreadModeEnum.Main)]
        public virtual void OnEventMainThreadP0(IntTestEvent @event)
        {
            HandleEvent(SubscribeAttribute.DefaultPriority, @event);
        }

        [Subscribe(ThreadModeEnum.Main, priority: 1)]
        public virtual void OnEventMainThreadP1(IntTestEvent @event)
        {
            HandleEvent(1, @event);
        }

        [Subscribe(ThreadModeEnum.Background, priority: 1)]
        public virtual void OnEventBackgroundThreadP1(int @event)
        {
            HandleEvent(1, @event);
        }

        [Subscribe(ThreadModeEnum.Background)]
        public virtual void OnEventBackgroundThreadP0(int @event)
        {
            HandleEvent(SubscribeAttribute.DefaultPriority, @event);
        }

        [Subscribe(ThreadModeEnum.Background, priority: -1)]
        public virtual void OnEventBackgroundThreadM1(int @event)
        {
            HandleEvent(-1, @event);
        }
    }
}