using Xunit;

namespace Snork.EventBus.Tests.Subscribers
{
    public class RepostInteger
    {
        private readonly EventBus _eventBus;

        public RepostInteger(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public int CountEvent { get; set; }
        public int LastEvent { get; set; }

        [Subscribe]
        public virtual void OnEvent(int @event)
        {
            LastEvent = @event;
            CountEvent++;
            Assert.Equal(CountEvent, @event);

            if (@event < 10)
            {
                var countIntEventBefore = CountEvent;
                _eventBus.Post(@event + 1);
                // All our Post calls will just enqueue the event, so check count is unchanged
                Assert.Equal(countIntEventBefore, countIntEventBefore);
            }
        }
    }
}