// Need to use upper class or Android test runner does not pick it up

namespace Snork.EventBus.Tests
{
    public class InheritanceSubclassTest : InheritanceTest
    {
        public int countMyEventOverwritten { get; set; }

        [Subscribe]
        public virtual void OnMessage(MyEvent message)
        {
            countMyEventOverwritten++;
        }

        public override void TestEventClassHierarchy()
        {
            // TODO fix test in super, then remove this
        }
    }
}