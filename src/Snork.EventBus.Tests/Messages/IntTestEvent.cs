namespace Snork.EventBus.Tests.Events
{
    /// <summary>
    /// Simple event storing an int value. More efficient than int because of the its flat hierarchy. 
    /// </summary>
    public class IntTestEvent
    {
        public readonly int value;

        public IntTestEvent(int value)
        {
            this.value = value;
        }
    }
}