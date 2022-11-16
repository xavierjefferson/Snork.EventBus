using System.Collections.Generic;

namespace Snork.EventBus
{
    public sealed class PendingPost
    {
        private static readonly Queue<PendingPost> PendingPostPool = new Queue<PendingPost>();

        private PendingPost(object message, Subscription subscription)
        {
            Message = message;
            Subscription = subscription;
        }

        internal PendingPost? Next { get; set; }

        public object? Message { get; private set; }
        public Subscription? Subscription { get; private set; }


        public static PendingPost ObtainPendingPost(Subscription subscription, object message)
        {
            lock (PendingPostPool)
            {
                var size = PendingPostPool.Count;
                if (size > 0)
                {
                    var pendingPost = PendingPostPool.Dequeue();
                    pendingPost.Message = message;
                    pendingPost.Subscription = subscription;
                    pendingPost.Next = null;
                    return pendingPost;
                }

                return new PendingPost(message, subscription);
            }
        }

        public static void ReleasePendingPost(PendingPost pendingPost)
        {
            lock (PendingPostPool)
            {
                pendingPost.Message = null;
                pendingPost.Subscription = null;
                pendingPost.Next = null;

                PendingPostPool.Enqueue(pendingPost);
            }
        }
    }
}