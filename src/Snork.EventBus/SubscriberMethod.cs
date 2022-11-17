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

        /// <summary>
        ///     Used for efficient comparison
        /// </summary>
        public string? MethodString { get; private set; }

        public override bool Equals(object other)
        {
            if (other == this) return true;

            if (other is SubscriberMethod otherSubscriberMethod)
            {
                GenerateMethodString();
                otherSubscriberMethod.GenerateMethodString();
                // Don't use method.equals because of http://code.google.com/p/android/issues/detail?id=7811#c6
                return MethodString.Equals(otherSubscriberMethod.MethodString);
            }

            return false;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        private void GenerateMethodString()
        {
            if (MethodString == null)
            {
                // MethodInfo.toString has more overhead, just take relevant parts of the method
                var builder = new StringBuilder(64);
                builder.Append(MethodInfo.DeclaringType.FullName);
                builder.Append('#').Append(MethodInfo.Name);
                builder.Append('(').Append(EventType.Name);
                MethodString = builder.ToString();
            }
        }

        public override int GetHashCode()
        {
            return MethodInfo.GetHashCode();
        }
    }
}