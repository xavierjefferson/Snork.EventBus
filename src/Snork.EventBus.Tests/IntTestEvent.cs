

/**
 * Simple message storing an int value. More efficient than int because of the its flat hierarchy. 
 */

namespace Snork.EventBus.Tests
{
    public class IntTestEvent
    {
        public readonly int value;

        public IntTestEvent(int value)
        {
            this.value = value;
        }
    }
}