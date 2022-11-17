using System;

namespace Snork.EventBus.Tests.Subscribers
{
    public class ThrowingSubscriber
    {
        [Subscribe]
        public virtual void OnEvent(object @event)
        {
            throw new Exception();
        }
    }
}