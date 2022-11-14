using System.Threading;

namespace Snork.EventBus
{
    public class ExecutorService
    {
        public void Execute(IRunnable runnable)
        {
            ThreadPool.QueueUserWorkItem(_ => runnable.Run(), new object(), true);
        }
    }
}