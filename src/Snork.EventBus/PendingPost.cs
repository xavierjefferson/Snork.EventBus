using System.Collections.Concurrent;

namespace Snork.EventBus
{
    public sealed class PendingPost
    {
        private static readonly ConcurrentQueue<PendingPost> PendingPostPool = new ConcurrentQueue<PendingPost>();

        private PendingPost(object @event, Subscription subscription)
        {
            Event = @event;
            Subscription = subscription;
        }

        public object? Event { get; private set; }
        public Subscription? Subscription { get; private set; }


        public static PendingPost ObtainPendingPost(Subscription subscription, object @event)
        {
            if (PendingPostPool.TryDequeue(out var pendingPost))
            {
                pendingPost.Event = @event;
                pendingPost.Subscription = subscription;
                return pendingPost;
            }

            return new PendingPost(@event, subscription);
        }

        public static void ReleasePendingPost(PendingPost pendingPost)
        {
            pendingPost.Event = null;
            pendingPost.Subscription = null;
            PendingPostPool.Enqueue(pendingPost);

        }
    }
}