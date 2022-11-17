using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Snork.EventBus
{
    internal sealed class PendingPostQueue
    {

        private readonly ConcurrentQueue<PendingPost> _queue = new ConcurrentQueue<PendingPost>();

        public void Enqueue(PendingPost pendingPost)
        {
            if (pendingPost == null) throw new ArgumentNullException(nameof(pendingPost));
            _queue.Enqueue(pendingPost);
        }

        public PendingPost? Poll()
        {
            return _queue.TryDequeue(out var pendingPost) ? pendingPost : null;
        }

        public PendingPost? Poll(TimeSpan duration)
        {
            if (!_queue.Any()) Thread.Sleep(duration);
            return Poll();
        }
    }
}