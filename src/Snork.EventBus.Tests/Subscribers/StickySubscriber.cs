using Snork.EventBus.Tests.Events;

namespace Snork.EventBus.Tests.Subscribers
{
    public class StickySubscriber
    {
        private readonly InheritanceTestBase _outer;

        public StickySubscriber(InheritanceTestBase outer)
        {
            _outer = outer;
        }

        [Subscribe(sticky: true)]
        public virtual void OnEvent(object @event)
        {
            _outer.CountObjectEvent++;
        }

        [Subscribe(sticky: true)]
        public virtual void OnEvent(MyInheritanceEvent @event)
        {
            _outer.CountMyEvent++;
        }

        [Subscribe(sticky: true)]
        public virtual void OnEvent(MyInheritanceEventExtended @event)
        {
            _outer.CountMyEventExtended++;
        }

        [Subscribe(sticky: true)]
        public virtual void OnEvent(MyInheritanceEventInterface @event)
        {
            _outer.CountMyEventInterface++;
        }

        [Subscribe(sticky: true)]
        public virtual void OnEvent(MyInheritanceEventInterfaceExtended @event)
        {
            _outer.CountMyEventInterfaceExtended++;
        }
    }
}