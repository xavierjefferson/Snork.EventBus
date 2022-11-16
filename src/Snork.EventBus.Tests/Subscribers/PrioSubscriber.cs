using Snork.EventBus.Tests.Messages;

namespace Snork.EventBus.Tests.Subscribers
{
    public class PrioSubscriber : MessageOrderedPriorityTestSubscriberBase
    {
        public PrioSubscriber(TestBase text) : base(text)
        {
        }

        [Subscribe(priority: 1)]
        public virtual void OnMessageP1(string message)
        {
            HandleMessage(1, message);
        }

        [Subscribe(priority: -1)]
        public virtual void OnMessageM1(string message)
        {
            HandleMessage(-1, message);
        }

        [Subscribe(priority: 0)]
        public virtual void OnMessageP0(string message)
        {
            HandleMessage(0, message);
        }

        [Subscribe(priority: 10)]
        public virtual void OnMessageP10(string message)
        {
            HandleMessage(10, message);
        }

        [Subscribe(priority: -100)]
        public virtual void OnMessageM100(string message)
        {
            HandleMessage(-100, message);
        }


        [Subscribe(ThreadModeEnum.Main, priority: -1)]
        public virtual void OnMessageMainThreadM1(IntTestMessage message)
        {
            HandleMessage(-1, message);
        }

        [Subscribe(ThreadModeEnum.Main)]
        public virtual void OnMessageMainThreadP0(IntTestMessage message)
        {
            HandleMessage(0, message);
        }

        [Subscribe(ThreadModeEnum.Main, priority: 1)]
        public virtual void OnMessageMainThreadP1(IntTestMessage message)
        {
            HandleMessage(1, message);
        }

        [Subscribe(ThreadModeEnum.Background, priority: 1)]
        public virtual void OnMessageBackgroundThreadP1(int message)
        {
            HandleMessage(1, message);
        }

        [Subscribe(ThreadModeEnum.Background)]
        public virtual void OnMessageBackgroundThreadP0(int message)
        {
            HandleMessage(0, message);
        }

        [Subscribe(ThreadModeEnum.Background, priority: -1)]
        public virtual void OnMessageBackgroundThreadM1(int message)
        {
            HandleMessage(-1, message);
        }
    }
}