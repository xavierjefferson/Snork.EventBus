// Need to use upper class or Android test runner does not pick it up

namespace Snork.EventBus.Tests
{
    public class InheritanceDisabledSubclassTest : InheritanceDisabledTest
    {
        public int countMyEventOverwritten { get; set; }

        [Subscribe]
        public override void OnMessage(MyEvent message)
        {
            countMyEventOverwritten++;
        }

        public override void TestEventClassHierarchy()
        {
            // TODO fix test in super, then remove this
        }
    }
}