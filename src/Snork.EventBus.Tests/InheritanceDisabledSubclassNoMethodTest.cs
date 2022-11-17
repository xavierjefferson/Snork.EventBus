using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class InheritanceDisabledSubclassNoMethodTest : InheritanceDisabledTest
    {
        public InheritanceDisabledSubclassNoMethodTest(ITestOutputHelper output) : base(output)
        {
        }
    }
}