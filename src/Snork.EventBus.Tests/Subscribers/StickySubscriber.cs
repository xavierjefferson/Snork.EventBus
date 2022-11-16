using Snork.EventBus.Tests.Messages;

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
        public virtual void OnMessage(object message)
        {
            _outer.CountObjectMessage++;
        }

        [Subscribe(sticky: true)]
        public virtual void OnMessage(MyInheritanceMessage message)
        {
            _outer.CountMyMessage++;
        }

        [Subscribe(sticky: true)]
        public virtual void OnMessage(MyInheritanceMessageExtended message)
        {
            _outer.CountMyMessageExtended++;
        }

        [Subscribe(sticky: true)]
        public virtual void OnMessage(MyInheritanceMessageInterface message)
        {
            _outer.CountMyMessageInterface++;
        }

        [Subscribe(sticky: true)]
        public virtual void OnMessage(MyInheritanceMessageInterfaceExtended message)
        {
            _outer.CountMyMessageInterfaceExtended++;
        }
    }
}