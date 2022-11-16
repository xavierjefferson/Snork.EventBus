using System;

namespace Snork.EventBus.Tests.Subscribers
{
    public class ThrowingSubscriber
    {
        [Subscribe]
        public virtual void OnMessage(object message)
        {
            throw new Exception();
        }
    }
}