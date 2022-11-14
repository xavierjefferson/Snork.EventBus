using System;

namespace Snork.EventBus.util
{
    /// <summary>
    ///     A generic failure message, which can be used by apps to propagate thrown exceptions.
    ///     Used as default failure message by <see cref="AsyncExecutor"/>.
    /// </summary>
    public class ThrowableFailureEvent : HasExecutionScope
    {
        protected readonly bool suppressErrorUi;
        public Exception Exception { get; }
        private object executionContext;

        public ThrowableFailureEvent(Exception throwable)
        {
            this.Exception = throwable;
            suppressErrorUi = false;
        }

        /// <summary>
        ///     @param suppressErrorUi
        ///     true indicates to the receiver that no error UI (e.g. dialog) should now displayed.
        /// </summary>
        public ThrowableFailureEvent(Exception throwable, bool suppressErrorUi)
        {
            this.Exception = throwable;
            this.suppressErrorUi = suppressErrorUi;
        }

        public object GetExecutionScope()
        {
            return executionContext;
        }

        public void SetExecutionScope(object executionContext)
        {
            this.executionContext = executionContext;
        }

    

        public bool IsSuppressErrorUi()
        {
            return suppressErrorUi;
        }
    }
}