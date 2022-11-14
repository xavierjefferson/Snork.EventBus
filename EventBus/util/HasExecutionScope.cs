namespace Snork.EventBus.util
{
    public interface HasExecutionScope
    {
        object GetExecutionScope();

        void SetExecutionScope(object executionScope);
    }
}