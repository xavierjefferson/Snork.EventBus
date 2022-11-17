using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Snork.EventBus.Interfaces;

namespace Snork.EventBus
{
    /// <summary>
    ///     Posts events in background.
    /// </summary>
    internal sealed class BackgroundPoster : IRunnable, IPoster
    {
        private static readonly TimeSpan PollDuration = TimeSpan.FromSeconds(1);
        private readonly EventBus _eventBus;

        private readonly object _mutex = new object();

        private readonly PendingPostQueue _queue;

        public BackgroundPoster(EventBus eventBus)
        {
            _eventBus = eventBus;
            _queue = new PendingPostQueue();
        }

        public void Enqueue(Subscription subscription, object @event)
        {
            var pendingPost = PendingPost.ObtainPendingPost(subscription, @event);

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

        public void Run()
        {
            Monitor.Enter(_mutex);
            try
            {
                while (true)
                    try
                    {
                        var pendingPost = _queue.Poll(PollDuration);
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