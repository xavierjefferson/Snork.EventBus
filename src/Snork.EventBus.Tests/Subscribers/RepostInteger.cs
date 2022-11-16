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

        public int CountMessage { get; set; }
        public int LastMessage { get; set; }

        [Subscribe]
        public virtual void OnMessage(int message)
        {
            LastMessage = message;
            CountMessage++;
            Assert.Equal(CountMessage, message);

            if (message < 10)
            {
                var countIntMessageBefore = CountMessage;
                _eventBus.Post(message + 1);
                // All our Post calls will just enqueue the @message, so check count is unchanged
                Assert.Equal(countIntMessageBefore, countIntMessageBefore);
            }
        }
    }
}