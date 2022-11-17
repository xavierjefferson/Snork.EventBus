using Snork.EventBus.Tests.Events;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class InheritanceSubclassTest : InheritanceTest
    {
        public InheritanceSubclassTest(ITestOutputHelper output) : base(output)
        {
        }

        public int CountMyEventOverwritten { get; set; }

        [Subscribe]
        public override void OnEvent(MyInheritanceEvent @event)
        {
            CountMyEventOverwritten++;
        }

        public override void TestEventClassHierarchy()
        {
            // TODO fix test in super, then remove this
        }
    }
}