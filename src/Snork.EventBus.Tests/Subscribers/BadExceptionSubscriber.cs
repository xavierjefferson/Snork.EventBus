using System;

namespace Snork.EventBus.Tests.Subscribers
{
    public class BadExceptionSubscriber
    {
        [Subscribe]
        public virtual void OnMessage(SubscriberExceptionMessage message)
        {
            throw new Exception("Bad");
        }
    }
}