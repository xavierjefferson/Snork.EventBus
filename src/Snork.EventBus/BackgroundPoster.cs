using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Snork.EventBus
{
    /// <summary>
    ///     Posts messages in background.
    /// </summary>
    internal sealed class BackgroundPoster : IRunnable, IPoster
    {
        private readonly EventBus _eventBus;

        private readonly PendingPostQueue _queue;

        private readonly object _mutex = new object();

        public BackgroundPoster(EventBus eventBus)
        {
            _eventBus = eventBus;
            _queue = new PendingPostQueue();
        }

        public void Enqueue(Subscription subscription, object message)
        {
            var pendingPost = PendingPost.ObtainPendingPost(subscription, message);
            lock (this)
            {
                _queue.Enqueue(pendingPost);
                if (Monitor.TryEnter(_mutex))
                    try
                    {
                        _eventBus.Executor.Execute(this);
                    }
                    finally
                    {
                        Monitor.Exit(_mutex);
                    }
            }
        }

        public void Run()
        {
            Monitor.Enter(_mutex);
            try
            {
                while (true)
                    try
                    {
                        var pendingPost = _queue.Poll(1000);
                        if (pendingPost == null) return;

                        var result = _eventBus.InvokeSubscriber(pendingPost);
                    }
                    catch (OperationCanceledException exception)
                    {
                        _eventBus.Logger.LogWarning(exception, Thread.CurrentThread.Name + " was canceled");
                    }
            }
            finally
            {
                Monitor.Exit(_mutex);
            }

        }
    }
}