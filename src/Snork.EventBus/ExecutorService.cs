using System.Threading;

namespace Snork.EventBus
{
    public class ExecutorService : IExecutor
    {
        public void Execute(IRunnable runnable)
        {
            ThreadPool.QueueUserWorkItem(_=> runnable.Run());
        }
    }
}