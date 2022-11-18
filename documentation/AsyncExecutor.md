# AsyncExecutor

AsyncExecutor is like a thread pool, but with failure (exception) handling. Failures are thrown exceptions and AsyncExecutor will wrap those exceptions inside an event, which is posted automatically.

_Disclaimer: AsyncExecutor is a non-core utility class. It might save you some code with error handling in background threads, but it’s not a core EventBus class._

Usually, you call AsyncExecutor.Create()  to create an instance and keep it in the application scope. Then to execute something, implement the  IRunnable  interface and pass it to the execute method of AsyncExecutor.

If the IRunnable  implementation throws an exception, it will be caught and wrapped into a  [ThrowableFailureEvent](../util/ThrowableFailureEvent.md), which will be posted.

Example for execution:

    AsyncExecutor.create().execute(
        new AsyncExecutor.RunnableEx() {
            @Override
            public void run() throws LoginException {
                // No need to catch any Exception (here: LoginException)
                remote.login();
                EventBus.getDefault().postSticky(new LoggedInEvent());
            }
        }
    );

Example for the receiving part:

    [Subscribe(ThreadMode = ThreadModeEnum.Main)
    public void HandleLoginEvent(LoggedInEvent @event) {
        // do something
    }
 
    [Subscribe(ThreadMode = ThreadModeEnum.Main)]
    public void HandleFailureEvent(ThrowableFailureEvent event) {
        // do something
    }

## AsyncExecutor Builder

If you want to customize your AsyncExecutor instance, call the static method  AsyncExecutor.Builder(). It will return a builder which lets you  **customize the EventBus instance, the thread pool, and the class of the failure event**.

Another customization option is the  **execution scope, which gives failure events context information**. For example, a failure event may be relevant only to a specific Activity instance or class.

If your custom failure event class implements the  [HasExecutionScope](../util/HasExecutionScope.md)  interface, AsyncExecutor will set the execution scope automatically. Like this, your subscriber can query the failure event for its execution scope and react depending on it.