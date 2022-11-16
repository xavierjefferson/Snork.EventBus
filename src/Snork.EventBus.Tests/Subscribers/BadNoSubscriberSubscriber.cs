using System;

namespace Snork.EventBus.Tests.Subscribers
{
    public class BadNoSubscriberSubscriber
    {
        [Subscribe]
        public virtual void OnMessage(NoSubscriberMessage message)
        {
            throw new Exception("I'm bad");
        }
    }
}