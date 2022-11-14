using System;

namespace Snork.EventBus.Meta
{
    public class SubscriberMethodInfo
    {
        public SubscriberMethodInfo(string methodName, Type messageType, ThreadModeEnum threadMode,
            int priority, bool sticky)
        {
            this.MethodName = methodName;
            this.ThreadMode = threadMode;
            this.MessageType = messageType;
            this.Priority = priority;
            this.Sticky = sticky;
        }

        public SubscriberMethodInfo(string methodName, Type messageType) :
            this(methodName, messageType, ThreadModeEnum.Posting, 0, false)
        {
        }

        public SubscriberMethodInfo(string methodName, Type messageType, ThreadModeEnum threadMode) : this(methodName,
            messageType, threadMode, 0, false)
        {
        }

        public Type MessageType { get; }
        public string MethodName { get; }
        public int Priority { get; }
        public bool Sticky { get; }
        public ThreadModeEnum ThreadMode { get; }
    }
}