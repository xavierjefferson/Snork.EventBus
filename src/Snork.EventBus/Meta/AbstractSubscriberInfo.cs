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

        public abstract List<SubscriberMethod> GetSubscriberMethods();

        public ISubscriberInfo? GetSuperSubscriberInfo()
        {
            if (SuperSubscriberInfoType == null) return null;
            return Activator.CreateInstance(SuperSubscriberInfoType) as ISubscriberInfo;
        }


        protected SubscriberMethod CreateSubscriberMethod(string methodName, Type messageType)
        {
            return CreateSubscriberMethod(methodName, messageType, ThreadModeEnum.Posting, 0, false);
        }

        protected SubscriberMethod CreateSubscriberMethod(string methodName, Type messageType, ThreadModeEnum threadMode)
        {
            return CreateSubscriberMethod(methodName, messageType, threadMode, 0, false);
        }

        protected SubscriberMethod CreateSubscriberMethod(string methodName, Type messageType, ThreadModeEnum threadMode,
            int priority, bool sticky)
        {
            try
            {
                var method = SubscriberType.GetMethod(methodName, new[] { messageType });
                if (method == null)
                    throw new EventBusException("Could not find subscriber method in " + SubscriberType +
                                                ". Maybe a missing ProGuard rule?");
                return new SubscriberMethod(method, messageType, threadMode, priority, sticky);
            }
            catch (Exception e)
            {
                throw new EventBusException("Could not find subscriber method in " + SubscriberType +
                                            ". Maybe a missing ProGuard rule?", e);
            }
        }
    }
}