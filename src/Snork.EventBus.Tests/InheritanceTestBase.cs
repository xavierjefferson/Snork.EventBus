namespace Snork.EventBus.Tests
{
    public abstract class InheritanceTestBase
    {
        public int CountMyMessageExtended { get; set; }
        public int CountMyMessage { get; set; }
        public int CountObjectMessage { get; set; }
        public int CountMyMessageInterface { get; set; }
        public int CountMyMessageInterfaceExtended { get; set; }
        protected EventBus EventBus { get; set; }
    }
}