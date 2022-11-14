using System;
using System.Collections.Generic;

namespace Snork.EventBus.Meta
{
    /// <summary> Base class for generated index classes created by annotation processing. </summary>
    public interface ISubscriberInfo
    {
        bool ShouldCheckSuperclass { get; }
        Type? SubscriberType { get; }

        List<SubscriberMethod> GetSubscriberMethods();

        ISubscriberInfo? GetSuperSubscriberInfo();
    }
}