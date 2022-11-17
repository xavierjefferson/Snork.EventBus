using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Snork.EventBus
{
    /// <summary>
    ///     Used internally by generated subscriber indexes.
    /// </summary>
    public class SubscriberMethod
    {
        public SubscriberMethod(MethodInfo methodInfo, Type eventType, ThreadModeEnum threadMode, int priority,
            bool sticky, int iteration)
        {
            MethodInfo = methodInfo;
            ThreadMode = threadMode;
            EventType = eventType;
            Priority = priority;
            Sticky = sticky;
            Iteration = iteration;
        }

        public Type EventType { get; }
        public MethodInfo MethodInfo { get; }
        public int Priority { get; }
        public bool Sticky { get; }
        public int Iteration { get; }
        public ThreadModeEnum ThreadMode { get; }

        public override bool Equals(object other)
        {
            if (other == this) return true;

            if (other is SubscriberMethod otherSubscriberMethod)
            {
                return otherSubscriberMethod.MethodInfo.Equals(this.MethodInfo);
            }

            return false;
        }


        public override int GetHashCode()
        {
            return MethodInfo.GetHashCode();
        }
    }
}