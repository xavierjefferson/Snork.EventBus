using System;

namespace Snork.EventBus.Tests.Subscribers
{
    public class BadNoSubscriberSubscriber
    {
        [Subscribe]
        public virtual void OnEvent(NoSubscriberEvent @event)
        {
            throw new Exception("I'm bad");
        }
    }
}