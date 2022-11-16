using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Snork.EventBus
{
    /// <summary>
    ///     Used internally by EventBus and generated subscriber indexes.
    /// </summary>
    public class SubscriberMethod
    {
        public SubscriberMethod(MethodInfo method, Type messageType, ThreadModeEnum threadMode, int priority,
            bool sticky, int iteration)
        {
            Method = method;
            ThreadMode = threadMode;
            EventType = messageType;
            Priority = priority;
            Sticky = sticky;
            Iteration = iteration;
        }

        public Type EventType { get; }
        public MethodInfo Method { get; }
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

            if (other is SubscriberMethod)
            {
                CheckMethodString();
                var otherSubscriberMethod = (SubscriberMethod)other;
                otherSubscriberMethod.CheckMethodString();
                // Don't use method.equals because of http://code.google.com/p/android/issues/detail?id=7811#c6
                return MethodString.Equals(otherSubscriberMethod.MethodString);
            }

            return false;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        private void CheckMethodString()
        {
            if (MethodString == null)
            {
                // MethodInfo.toString has more overhead, just take relevant parts of the method
                var builder = new StringBuilder(64);
                builder.Append(Method.DeclaringType.FullName);
                builder.Append('#').Append(Method.Name);
                builder.Append('(').Append(EventType.Name);
                MethodString = builder.ToString();
            }
        }

        public override int GetHashCode()
        {
            return Method.GetHashCode();
        }
    }
}