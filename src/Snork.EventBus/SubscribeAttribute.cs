using System;

namespace Snork.EventBus
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class SubscribeAttribute : Attribute
    {
        public SubscribeAttribute(ThreadModeEnum threadMode = ThreadModeEnum.Posting, bool sticky = false, int priority = 5)
        {
            ThreadMode = threadMode;
            Sticky = sticky;
            Priority = priority;
        }

        public ThreadModeEnum ThreadMode { get; }

        /// <summary>
        ///     If true, delivers the most recent sticky message (posted with <see cref="EventBus.PostSticky(object)" />  to this
        ///     subscriber (if message available).
        /// </summary>
        public bool Sticky { get; }

        /// <summary>
        ///     Subscriber priority to influence the order of message delivery.
        ///     Within the same delivery thread (<see cref="ThreadMode" />), higher priority subscribers will receive messages before
        ///     others with a lower priority. The default priority is 0. Note: the priority does *NOT* affect the order of
        ///     delivery among subscribers with different <see cref="ThreadMode" />s!
        /// </summary>
        public int Priority { get; }
    }
}