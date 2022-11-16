
#Snork.EventBus
========
Snork.EventBus is a publish/subscribe event bus for C#.
<img src="EventBus-Publish-Subscribe.png" width="500" height="187"/>

It is a port of the EventBus project for Android/Java


EventBus...

 * simplifies the communication between components
    * decouples event senders and receivers
    * performs well with background threads
    * avoids complex and error-prone dependencies and life cycle issues
 * makes your code simpler
 * is fast
 * is tiny (~45k dll)
 * has advanced features like delivery threads, subscriber priorities, etc.

Snork.EventBus in 3 steps
-------------------
1. Define events:

    ```C#  
    public class MessageType { /* Additional fields if needed */ }
    ```

2. Prepare subscribers:
    Declare and annotate your subscribing method, optionally specify a [thread mode](documentation/delivery-threads-threadmode.md):  

    ```C#
    [Subscribe(ThreadMode: ThreadModeEnum.Main)]  
    public void OnReceived(MessageType message) {
        // Do something
    }
    ```
    Register and unregister your subscriber.

   ```C#
    public void OnStart() {
        EventBus.Default.Register(this);
    }
 
    public void OnStop() {
        EventBus.Default.Unregister(this);
    }
    ```

3. Post events:

   ```C#
    EventBus.Default.Post(new MessageType());
    ```

Read the full [getting started guide](documentation/how-to-get-started.md).

There are also some [examples](https://github.com/greenrobot-team/greenrobot-examples).

**Note:** we highly recommend the [EventBus annotation processor with its subscriber index](https://greenrobot.org/eventbus/documentation/subscriber-index/).
This will avoid some reflection related problems seen in the wild.  

Add EventBus to your project
----------------------------
<a href="https://search.maven.org/search?q=g:org.greenrobot%20AND%20a:eventbus"><img src="https://img.shields.io/maven-central/v/org.greenrobot/eventbus.svg"></a>

Available on <a href="https://search.maven.org/search?q=g:org.greenrobot%20AND%20a:eventbus">Maven Central</a>.

Android projects:
```groovy
implementation("org.greenrobot:eventbus:3.3.1")
```

Java projects:
```groovy
implementation("org.greenrobot:eventbus-java:3.3.1")
```
```xml
<dependency>
    <groupId>org.greenrobot</groupId>
    <artifactId>eventbus-java</artifactId>
    <version>3.3.1</version>
</dependency>
```
Homepage, Documentation, Links
------------------------------
For more details please check the [EventBus website](https://greenrobot.org/eventbus). Here are some direct links you may find useful:

[Features](https://greenrobot.org/eventbus/features/)

[Documentation](https://greenrobot.org/eventbus/documentation/)

[Changelog](https://github.com/greenrobot/EventBus/releases)

[FAQ](https://greenrobot.org/eventbus/documentation/faq/)

License
-------
Snork.EventBus binaries and source code can be used according to the [Apache License, Version 2.0](LICENSE).

