using System.Collections.Generic;

namespace Snork.EventBus.Tests.Subscribers
{
    public class IEnumerableSubscriber
    {
        [Subscribe]
        public virtual void OnEvent(IEnumerable<object> @event)
        {
        }
    }
}