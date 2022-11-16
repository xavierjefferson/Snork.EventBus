// Need to use upper class or Android test runner does not pick it up

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