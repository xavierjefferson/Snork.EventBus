using System;

namespace Snork.EventBus.Tests.Subscribers
{
    public class BadExceptionSubscriber
    {
        [Subscribe]
        public virtual void OnEvent(SubscriberExceptionEvent @event)
        {
            throw new Exception("Bad");
        }
    }
}