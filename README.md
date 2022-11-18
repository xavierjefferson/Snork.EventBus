
#Snork.EventBus
========
Snork.EventBus is a publish/subscribe event bus for .NET.
<img src="EventBus-Publish-Subscribe.png" width="500" height="187"/>

It is a port of the EventBus project for Android/Java.


EventBus...

 * simplifies the communication between components
    * decouples event senders and receivers
    * performs well with background threads
    * avoids complex and error-prone dependencies and life cycle issues
 * makes your code simpler
 * is fast
 * is tiny
 * has advanced features like delivery threads, subscriber priorities, etc.

Snork.EventBus In Three Steps
-------------------
1. Define events:

    ```C#  
    public class MessageType { /* Additional fields if needed */ }
    ```

2. Prepare subscribers:
    Declare and annotate your subscribing method, optionally specify a [thread mode](documentation/DeliveryThreadsThreadmode.md):  

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

Read the full [getting started guide](documentation/HowToGetStarted.md).

There are also some [examples](Examples.md).

**Note:** we highly recommend the [EventBus annotation processor with its subscriber index](documentation/SubscriberIndex.md).
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
Links
------------------------------
Here are some documents that you may find useful:

[Features](Features.md)

[Documentation](documentation/Index.md)

[FAQ](documentation/FAQ.md)

License
-------
Snork.EventBus binaries and source code can be used according to the [Apache License, Version 2.0](LICENSE).

