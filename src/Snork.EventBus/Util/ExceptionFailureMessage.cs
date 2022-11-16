using System;

namespace Snork.EventBus.Util
{
    /// <summary>
    ///     A generic failure message, which can be used by apps to propagate thrown exceptions.
    ///     Used as default failure message by <see cref="AsyncExecutor" />.
    /// </summary>
    public class ExceptionFailureMessage : IExecutionScopeContainer
    {
        public bool SuppressErrorUi { get; }

        public ExceptionFailureMessage(Exception exception)
        {
            Exception = exception;
            SuppressErrorUi = false;
        }

        /// <summary>
        ///     @param suppressErrorUi
        ///     true indicates to the receiver that no error UI (e.g. dialog) should now displayed.
        /// </summary>
        public ExceptionFailureMessage(Exception exception, bool suppressErrorUi)
        {
            Exception = exception;
            this.SuppressErrorUi = suppressErrorUi;
        }

        public Exception Exception { get; }
        public object? ExecutionScope { get; set; }
    }
}