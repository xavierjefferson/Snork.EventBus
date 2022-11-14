using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Snork.EventBus
{
    /// <summary>
    ///     Posts events in background.
    /// </summary>
    internal sealed class BackgroundPoster : IRunnable, IPoster
    {
        private readonly EventBus _eventBus;

        private readonly PendingPostQueue _queue;

        private volatile bool _executorRunning;

        public BackgroundPoster(EventBus eventBus)
        {
            this._eventBus = eventBus;
            _queue = new PendingPostQueue();
        }

        public void Enqueue(Subscription subscription, object message)
        {
            var pendingPost = PendingPost.ObtainPendingPost(subscription, message);
            lock (this)
            {
                _queue.Enqueue(pendingPost);
                if (!_executorRunning)
                {
                    _executorRunning = true;
                    _eventBus.GetExecutorService().Execute(this);
                }
            }
        }


        public void Run()
        {
            try
            {
                try
                {
                    while (true)
                    {
                        var pendingPost = _queue.Poll(1000);
                        if (pendingPost == null)
                            lock (this)
                            {
                                // Check again, this time in synchronized
                                pendingPost = _queue.Poll();
                                if (pendingPost == null)
                                {
                                    _executorRunning = false;
                                    return;
                                }
                            }

                        var result = _eventBus.InvokeSubscriber(pendingPost);
                        if (result != null)
                        {
                            _eventBus.Post(result);
                        }
                    }
                }
                catch (OperationCanceledException e)
                {
                    _eventBus.Logger.log(LogLevel.Warning, Thread.CurrentThread.Name + " was canceled", e);
                }
            }
            finally
            {
                _executorRunning = false;
            }
        }
    }
}