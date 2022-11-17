namespace Snork.EventBus.Interfaces
{
    public interface IExecutor
    {
        void Execute(IRunnable runnable);
    }
}