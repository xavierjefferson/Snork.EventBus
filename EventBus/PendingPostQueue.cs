using System;
using System.Threading;

namespace Snork.EventBus
{
    internal sealed class PendingPostQueue
    {
        private readonly object _mutex = new object();
        private PendingPost? _head;
        private PendingPost? _tail;

        public void Enqueue(PendingPost pendingPost)
        {
            lock (_mutex)
            {
                if (pendingPost == null) throw new ArgumentNullException(nameof(pendingPost));

                if (_tail != null)
                {
                    _tail.Next = pendingPost;
                    _tail = pendingPost;
                }
                else if (_head == null)
                {
                    _head = _tail = pendingPost;
                }
                else
                {
                    throw new InvalidOperationException("Head present, but no tail");
                }

                //notifyAll();
            }
        }

        public PendingPost? Poll()
        {
            lock (_mutex)
            {
                var pendingPost = _head;
                if (_head != null)
                {
                    _head = _head.Next;
                    if (_head == null) _tail = null;
                }

                return pendingPost;
            }
        }

        public PendingPost? Poll(int maxMillisToWait)
        {
            lock (_mutex)
            {
                if (_head == null) Thread.Sleep(maxMillisToWait);

                return Poll();
            }
        }
    }
}