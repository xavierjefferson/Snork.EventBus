namespace Snork.EventBus.Util
{
    public interface IExecutionScopeContainer
    {
        object? ExecutionScope { get; set; }
    }
}