/**
 * Simple message storing an int value. More efficient than int because of the its flat hierarchy. 
 */

namespace Snork.EventBus.Tests.Messages
{
    public class IntTestMessage
    {
        public readonly int value;

        public IntTestMessage(int value)
        {
            this.value = value;
        }
    }
}