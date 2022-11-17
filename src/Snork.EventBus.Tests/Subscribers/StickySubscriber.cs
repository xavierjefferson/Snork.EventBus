using Snork.EventBus.Tests.Events;

namespace Snork.EventBus.Tests.Subscribers
{
    public class StickySubscriber
    {
        private readonly InheritanceTestBase _outerTest;

        public StickySubscriber(InheritanceTestBase outerTest)
        {
            _outerTest = outerTest;
        }

        [Subscribe(sticky: true)]
        public virtual void OnEvent(object @event)
        {
            _outerTest.CountObjectEvent++;
        }

        [Subscribe(sticky: true)]
        public virtual void OnEvent(MyInheritanceEvent @event)
        {
            _outerTest.CountMyEvent++;
        }

        [Subscribe(sticky: true)]
        public virtual void OnEvent(MyInheritanceEventExtended @event)
        {
            _outerTest.CountMyEventExtended++;
        }

        [Subscribe(sticky: true)]
        public virtual void OnEvent(MyInheritanceEventInterface @event)
        {
            _outerTest.CountMyEventInterface++;
        }

        [Subscribe(sticky: true)]
        public virtual void OnEvent(MyInheritanceEventInterfaceExtended @event)
        {
            _outerTest.CountMyEventInterfaceExtended++;
        }
    }
}