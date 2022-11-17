using System.Threading;
using Snork.EventBus.Interfaces;

namespace Snork.EventBus
{
    public class ExecutorService : IExecutor
    {
        public void Execute(IRunnable runnable)
        {
            ThreadPool.QueueUserWorkItem(_=> runnable.Run(), new object(), true);
        }
    }
}