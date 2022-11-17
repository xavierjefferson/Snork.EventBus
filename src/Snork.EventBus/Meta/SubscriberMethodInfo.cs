using System;

namespace Snork.EventBus.Meta
{
    public class SubscriberMethodInfo
    {
        public SubscriberMethodInfo(string methodName, Type eventType, ThreadModeEnum threadMode,
            int priority, bool sticky)
        {
            this.MethodName = methodName;
            this.ThreadMode = threadMode;
            this.EventType = eventType;
            this.Priority = priority;
            this.Sticky = sticky;
        }

        public SubscriberMethodInfo(string methodName, Type eventType) :
            this(methodName, eventType, ThreadModeEnum.Posting, 0, false)
        {
        }

        public SubscriberMethodInfo(string methodName, Type eventType, ThreadModeEnum threadMode) : this(methodName,
            eventType, threadMode, 0, false)
        {
        }

        public Type EventType { get; }
        public string MethodName { get; }
        public int Priority { get; }
        public bool Sticky { get; }
        public ThreadModeEnum ThreadMode { get; }
    }
}