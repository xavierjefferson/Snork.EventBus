// Need to use upper class or Android test runner does not pick it up

using Snork.EventBus.Tests.Messages;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class InheritanceSubclassTest : InheritanceTest
    {
        public int CountMyMessageOverwritten { get; set; }

        [Subscribe]
        public override void OnMessage(MyInheritanceMessage message)
        {
            CountMyMessageOverwritten++;
        }

        public override void TestMessageClassHierarchy()
        {
            // TODO fix test in super, then remove this
        }

        public InheritanceSubclassTest(ITestOutputHelper output) : base(output)
        {
        }
    }
}