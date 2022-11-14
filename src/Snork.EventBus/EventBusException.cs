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

        public EventBusException(Exception exception) : base("", exception)
        {
        }

        public EventBusException(string detailMessage, Exception exception) : base(detailMessage, exception)
        {
        }
    }
}