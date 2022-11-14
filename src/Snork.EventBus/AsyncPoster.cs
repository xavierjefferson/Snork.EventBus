using System;

namespace Snork.EventBus
{
    /// <summary>
    ///     Posts messages in background.
    /// </summary>
    internal class AsyncPoster : IRunnable, IPoster
    {
        private readonly EventBus _eventBus;

        private readonly PendingPostQueue _queue;

        public AsyncPoster(EventBus eventBus)
        {
            _eventBus = eventBus;
            _queue = new PendingPostQueue();
        }

        public void Enqueue(Subscription subscription, object message)
        {
            var pendingPost = PendingPost.ObtainPendingPost(subscription, message);
            _queue.Enqueue(pendingPost);
            _eventBus.Executor.Execute(this);
        }


        public void Run()
        {
            var pendingPost = _queue.Poll();
            if (pendingPost == null) throw new InvalidOperationException("No pending post available");

            _eventBus.InvokeSubscriber(pendingPost);
        }
    }
}