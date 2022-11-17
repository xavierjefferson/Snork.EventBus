using System;
using System.Collections.Generic;

namespace Snork.EventBus.Meta
{
    /// <summary>
    ///     Base class for generated subscriber meta info classes created by annotation processing.
    /// </summary>
    public abstract class AbstractSubscriberInfo : ISubscriberInfo
    {
        protected AbstractSubscriberInfo(Type subscriberType, Type? superSubscriberInfoType,
            bool shouldCheckSuperclass)
        {
            SubscriberType = subscriberType;
            SuperSubscriberInfoType = superSubscriberInfoType;
            ShouldCheckSuperclass = shouldCheckSuperclass;
        }

        public Type? SuperSubscriberInfoType { get; }
        public bool ShouldCheckSuperclass { get; }
        public Type? SubscriberType { get; }

        public abstract List<SubscriberMethod> GetSubscriberMethods(int iteration);

        public ISubscriberInfo? GetSuperSubscriberInfo()
        {
            if (SuperSubscriberInfoType == null) return null;
            return Activator.CreateInstance(SuperSubscriberInfoType) as ISubscriberInfo;
        }


        protected SubscriberMethod CreateSubscriberMethod(string methodName, Type eventType)
        {
            return CreateSubscriberMethod(methodName, eventType, ThreadModeEnum.Posting, 0, false, 0);
        }

        protected SubscriberMethod CreateSubscriberMethod(string methodName, Type eventType, ThreadModeEnum threadMode)
        {
            return CreateSubscriberMethod(methodName, eventType, threadMode, 0, false, 0);
        }

        protected SubscriberMethod CreateSubscriberMethod(string methodName, Type eventType,
            ThreadModeEnum threadMode,
            int priority, bool sticky, int iteration)
        {
            try
            {
                var method = SubscriberType.GetMethod(methodName, new[] { eventType });
                if (method == null)
                    throw new EventBusException($"Could not find subscriber method in {SubscriberType.FullName}");
                return new SubscriberMethod(method, eventType, threadMode, priority, sticky, iteration);
            }
            catch (Exception e)
            {
                throw new EventBusException($"Could not find subscriber method in {SubscriberType.FullName}", e);
            }
        }
    }
}