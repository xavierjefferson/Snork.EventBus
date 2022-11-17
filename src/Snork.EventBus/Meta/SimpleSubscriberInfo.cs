using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Snork.EventBus.Meta
{
    /// <summary>
    ///     Uses <see cref="SubscriberMethodInfo"/> instances to create <see cref="SubscriberMethod"/> instances on
    ///     demand.
    /// </summary>
    public class SimpleSubscriberInfo : AbstractSubscriberInfo
    {
        private readonly List<SubscriberMethodInfo> _methodInfos;

        public SimpleSubscriberInfo(Type subscriberType, bool shouldCheckSuperclass,
            List<SubscriberMethodInfo> methodInfos) : base(subscriberType, null, shouldCheckSuperclass)
        {
            this._methodInfos = methodInfos;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override List<SubscriberMethod> GetSubscriberMethods(int generation)
        {
            var methods = new List<SubscriberMethod>();
            foreach (var info in _methodInfos)
                methods.Add(CreateSubscriberMethod(info.MethodName, info.EventType, info.ThreadMode,
                    info.Priority, info.Sticky, generation));

            return methods;
        }
    }
}