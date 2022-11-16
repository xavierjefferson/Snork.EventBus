// Need to use upper class or Android test runner does not pick it up

using Snork.EventBus.Tests.Messages;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class InheritanceDisabledSubclassTest : InheritanceDisabledTest
    {
        public int CountMyMessageOverridden { get; set; }

        [Subscribe]
        public override void OnMessage(MyInheritanceMessage message)
        {
            CountMyMessageOverridden++;
        }

        public override void TestMessageClassHierarchy()
        {
            // TODO fix test in super, then remove this
        }

        public InheritanceDisabledSubclassTest(ITestOutputHelper output) : base(output)
        {
        }
    }
}