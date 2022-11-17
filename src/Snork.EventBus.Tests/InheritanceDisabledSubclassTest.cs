using Snork.EventBus.Tests.Events;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class InheritanceDisabledSubclassTest : InheritanceDisabledTest
    {
        public InheritanceDisabledSubclassTest(ITestOutputHelper output) : base(output)
        {
        }

        public int CountMyEventOverridden { get; set; }

        [Subscribe]
        public override void OnEvent(MyInheritanceEvent @event)
        {
            CountMyEventOverridden++;
        }

        public override void TestEventClassHierarchy()
        {
            // TODO fix test in super, then remove this
        }
    }
}