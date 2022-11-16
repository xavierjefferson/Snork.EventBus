// Need to use upper class or Android test runner does not pick it up

using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public class InheritanceSubclassNoMethodTest : InheritanceTest
    {
        public InheritanceSubclassNoMethodTest(ITestOutputHelper output) : base(output)
        {
        }
    }
}