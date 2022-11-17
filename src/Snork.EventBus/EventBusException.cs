using System;

namespace Snork.EventBus
{
    /// <summary>
    ///     An <see cref="InvalidOperationException"/> thrown in cases something went wrong inside EventBus.
    /// </summary>
    public class EventBusException : InvalidOperationException
    {

        public EventBusException(string detailEvent) : base(detailEvent)
        {
        }

        public EventBusException(Exception exception) : base("", exception)
        {
        }

        public EventBusException(string detailEvent, Exception exception) : base(detailEvent, exception)
        {
        }
    }
}