using Snork.EventBus.Tests.Events;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public abstract class InheritanceTestBase : LoggingTestBase
    {
        protected InheritanceTestBase(ITestOutputHelper output) : base(output)
        {
        }

        public int CountMyEventExtended { get; set; }
        public int CountMyEvent { get; set; }
        public int CountObjectEvent { get; set; }
        public int CountMyEventInterface { get; set; }
        public int CountMyEventInterfaceExtended { get; set; }

        [Subscribe]
        public void OnEvent(object @event)
        {
            CountObjectEvent++;
        }

        [Subscribe]
        public virtual void OnEvent(MyInheritanceEvent @event)
        {
            CountMyEvent++;
        }

        [Subscribe]
        public void OnEvent(MyInheritanceEventExtended @event)
        {
            CountMyEventExtended++;
        }

        [Subscribe]
        public void OnEvent(MyInheritanceEventInterface @event)
        {
            CountMyEventInterface++;
        }

        [Subscribe]
        public void OnEvent(MyInheritanceEventInterfaceExtended @event)
        {
            CountMyEventInterfaceExtended++;
        }
    }
}