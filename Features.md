
# EventBus Features

What makes EventBus unique are its features:

-   **Simple yet powerful:** EventBus is a tiny library with an API that is super easy to learn. Nevertheless, your software architecture may great benefit by decoupling components: Subscribers do not have know about senders, when using events.
-   **High Performance:**  Performance matters. EventBus was profiled and optimized a lot; probably making it the fastest solution of its kind.
-   **Convenient Annotation based API** (without sacrificing performance)**:**  Simply apply the [Subscribe()] attribute to your subscriber methods.
-   **Selective thread delivery:**  EventBus is extensible to deliver events in the thread of your choosing, regardless how an event was posted.
-   **Background thread delivery:**  If your subscriber does long running tasks, EventBus can also use background threads to avoid UI blocking.
-   **Event and Subscriber inheritance:**  In EventBus, the object oriented paradigm apply to event and subscriber classes. Let’s say event class A is the superclass of B. Posted events of type B will also be posted to subscribers interested in A. Similarly the inheritance of subscriber classes are considered.
-   **Zero configuration:** You can get started immediately using a default EventBus instance available from anywhere in your code.
-   **Configurable:** To tweak EventBus to your requirements, you can adjust its behavior using the builder pattern.