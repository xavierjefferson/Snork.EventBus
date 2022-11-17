using System;

namespace Snork.EventBus
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class SubscribeAttribute : Attribute
    {
        public const int DefaultPriority = 5;
        public SubscribeAttribute(ThreadModeEnum threadMode = ThreadModeEnum.Posting, bool sticky = false, int priority = DefaultPriority)
        {
            ThreadMode = threadMode;
            Sticky = sticky;
            Priority = priority;
        }

        public ThreadModeEnum ThreadMode { get; }

        /// <summary>
        ///     If true, delivers the most recent sticky event (posted with <see cref="EventBus.PostSticky(object)" />  to this
        ///     subscriber (if event available).
        /// </summary>
        public bool Sticky { get; }

        /// <summary>
        ///     Subscriber priority to influence the order of event delivery.
        ///     Within the same delivery thread (<see cref="ThreadMode" />), higher priority subscribers will receive events before
        ///     others with a lower priority. The default priority is <see cref="DefaultPriority" />. The priority does not affect the order of
        ///     delivery among subscribers with different <see cref="ThreadMode" /> values.
        /// </summary>
        public int Priority { get; }
    }
}