using Snork.EventBus.Tests.Messages;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public abstract class InheritanceTestBase : LoggingTestBase
    {
        public int CountMyMessageExtended { get; set; }
        public int CountMyMessage { get; set; }
        public int CountObjectMessage { get; set; }
        public int CountMyMessageInterface { get; set; }
        public int CountMyMessageInterfaceExtended { get; set; }
        protected EventBus EventBus { get; set; }

        [Subscribe]
        public void OnMessage(object message)
        {
            CountObjectMessage++;
        }

        [Subscribe]
        public virtual void OnMessage(MyInheritanceMessage message)
        {
            CountMyMessage++;
        }

        [Subscribe]
        public void OnMessage(MyInheritanceMessageExtended message)
        {
            CountMyMessageExtended++;
        }

        [Subscribe]
        public void OnMessage(MyInheritanceMessageInterface message)
        {
            CountMyMessageInterface++;
        }

        [Subscribe]
        public void OnMessage(MyInheritanceMessageInterfaceExtended message)
        {
            CountMyMessageInterfaceExtended++;
        }

        protected InheritanceTestBase(ITestOutputHelper output) : base(output)
        {
        }
    }
}