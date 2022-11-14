using System;

namespace Snork.EventBus.Meta
{
    /// <summary>
    ///     Interface for generated indexes.
    /// </summary>
    public interface ISubscriberInfoIndex
    {
        ISubscriberInfo? GetSubscriberInfo(Type? subscriberType);
    }
}