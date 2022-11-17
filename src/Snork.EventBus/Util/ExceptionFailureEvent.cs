using System;

namespace Snork.EventBus.Util
{
    /// <summary>
    ///     A generic failure event, which can be used by apps to propagate thrown exceptions.
    ///     Used as default failure event by <see cref="AsyncExecutor" />.
    /// </summary>
    public class ExceptionFailureEvent : IExecutionScopeContainer
    {
        public bool SuppressErrorUi { get; }

        public ExceptionFailureEvent(Exception exception)
        {
            Exception = exception;
            SuppressErrorUi = false;
        }

        /// <summary>
        ///     @param suppressErrorUi
        ///     true indicates to the receiver that no error UI (e.g. dialog) should now displayed.
        /// </summary>
        public ExceptionFailureEvent(Exception exception, bool suppressErrorUi)
        {
            Exception = exception;
            this.SuppressErrorUi = suppressErrorUi;
        }

        public Exception Exception { get; }
        public object? ExecutionScope { get; set; }
    }
}