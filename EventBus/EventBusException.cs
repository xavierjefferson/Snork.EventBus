using System;

namespace Snork.EventBus
{
    /// <summary>
    ///     An <see cref="InvalidOperationException"/> thrown in cases something went wrong inside EventBus.
    ///     @author Markus
    /// </summary>
    public class EventBusException : Exception
    {

        public EventBusException(string detailMessage) : base(detailMessage)
        {
        }

        public EventBusException(Exception throwable) : base("", throwable)
        {
        }

        public EventBusException(string detailMessage, Exception throwable) : base(detailMessage, throwable)
        {
        }
    }
}