namespace Snork.EventBus
{
    public interface IExecutor
    {
        void Execute(IRunnable runnable);
    }
}