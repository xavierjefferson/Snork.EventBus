
# Delivery Threads (ThreadMode)

Contents

-   [1  ThreadMode: Posting](https://greenrobot.org/eventbus/documentation/delivery-threads-threadmode/#ThreadMode_POSTING)
-   [2  ThreadMode: Main](https://greenrobot.org/eventbus/documentation/delivery-threads-threadmode/#ThreadMode_MAIN)
-   [3  ThreadMode: MainOrdered](https://greenrobot.org/eventbus/documentation/delivery-threads-threadmode/#ThreadMode_MAIN_ORDERED)
-   [4  ThreadMode: Background](https://greenrobot.org/eventbus/documentation/delivery-threads-threadmode/#ThreadMode_BACKGROUND)
-   [5  ThreadMode: Async](https://greenrobot.org/eventbus/documentation/delivery-threads-threadmode/#ThreadMode_ASYNC)

EventBus can handle threading for you: events can be posted in threads different from the posting thread. A common use case is dealing with UI changes. In WinForms and WPF, UI changes must be done in the UI thread. On the other hand, networking, or any time consuming task, must not run on the main thread. EventBus helps you to deal with those tasks and synchronize with the UI thread without having to delve into thread transitions.

In EventBus, you may define the thread that will call the event handling method by using one of the four ThreadModes.

### ThreadModeEnum.Posting

**This is the default.** Subscribers will be called in the same thread posting the event. Event delivery is done synchronously and all subscribers will have been called once the posting is done. This ThreadMode implies the least overhead because it avoids thread switching completely. Thus this is the recommended mode for simple tasks that are known to complete is a very short time without requiring the main thread. Event handlers using this mode should return quickly to avoid blocking the posting thread, which may be the main thread. Example:

    // Called in the same thread (default)
    // ThreadMode is optional here
    @Subscribe(threadMode  =  ThreadMode.POSTING)
    public  void  onMessage(MessageEvent event)  {
         log(event.message);
    }

### ThreadModeEnum.Main

    Subscribers will be called in Android’s main thread (sometimes referred to as UI thread). If the posting thread is the main thread, event handler methods will be called directly (synchronously like described for ThreadMode.POSTING). Event handlers using this mode must return quickly to avoid blocking the main thread. Example:

    // Called in Android UI's main thread
    @Subscribe(threadMode  =  ThreadMode.MAIN)
    public  void  onMessage(MessageEvent event)  {
        textField.setText(event.message);
    }

If not on Android behaves the same as ThreadMode.POSTING.

### ThreadModeEnum.MainOrdered

Subscribers will be called in Android’s main thread. The event is always enqueued for later delivery to subscribers, so the call to post will return immediately. This gives event processing a stricter and more consistent order (thus the name MAIN_ORDERED). For example if you post another event in an event handler with MAIN thread mode, the second event handler will finish before the first one (because it is called synchronously – compare it to a method call). With MAIN_ORDERED, the first event handler will finish, and then the second one will be invoked at a later point in time (as soon as the main thread has capacity).

Event handlers using this mode must return quickly to avoid blocking the main thread. Example:

    // Called in Android UI's main thread
    @Subscribe(threadMode  =  ThreadMode.MAIN_ORDERED)
    public  void  onMessage(MessageEvent event)  {
        textField.setText(event.message);
    }

If not on Android behaves the same as ThreadModeEnum.Posting

### ThreadModeEnum.Background

Subscribers will be called in a background thread. On Android, if the posting thread is not the main thread, event handler methods will be called directly in the posting thread. If the posting thread is the main thread, EventBus uses a single background thread that will deliver all its events sequentially. Event handlers using this mode should try to return quickly to avoid blocking the background thread.

    // Called in the background thread
    @Subscribe(threadMode  =  ThreadMode.BACKGROUND)
    public  void  onMessage(MessageEvent event){
        saveToDisk(event.message);
    }

If not on Android, always uses a background thread.

### ThreadModeEnum.Async

Event handler methods are called in a separate thread. This is always independent from the posting thread and the main thread. Posting events never wait for event handler methods using this mode. Event handler methods should use this mode if their execution might take some time, e.g. for network access. Avoid triggering a large number of long running asynchronous handler methods at the same time to limit the number of concurrent threads. EventBus uses a thread pool to efficiently reuse threads from completed asynchronous event handler notifications.

    // Called in a separate thread
    [Subscribe(ThreadMode = ThreadModeEnum.Async)]
    public void OnMessage(MessageEvent Event){
        backend.Send(@event.Message);
    }