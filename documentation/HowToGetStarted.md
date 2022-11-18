
# How To Get Started With EventBus In Three Steps

Contents

 1. [Define Events](#DefineEvents)
 2. [Prepare Subscribers](#PrepareSubscribers)
 3. [Post Events](#PostEvents)
 4. [Learn more](#LearnMore)

The EventBus API is as easy as 1-2-3.

**Before we get started  [make sure to add EventBus as a dependency to your project](#AddEventBusToYourProject).**

<a name=DefineEvents></a>
### Step 1: Define Events

Events are POCO (plain old C# object) without any specific requirements.

    public class MessageEvent  {    
        public readonly string Message { get; set; }
        
        public MessageEvent(string message) {
	        this.message = message;
	    }
	}

<a name=PrepareSubscribers></a>
### Step 2: Prepare Subscribers

Subscribers implement event handling methods (also called “subscriber methods”) that will be called when an event is posted. These are defined with the  **[Subscribe()]**  attribute.

    // This method will be called when a MessageEvent is posted
    [Subscribe(ThreadMode = ThreadModeEnum.Main)]
    public void OnMessageEvent(MessageEvent @event)  {
        System.Diagnostics.Debug.Print(@event.Message);
    }
    
    // This method will be called when a SomeOtherEvent is posted
    [Subscribe()]
    public void HandleSomethingElse(SomeOtherEvent @event) {
		DoSomethingWith(@event);
	}

Subscribers also need to  **register**  themselves to  **and unregister**  from the bus.  Subscribers will receive events only while they are registered.

@Override

public  void  onStart()  {

super.onStart();

EventBus.getDefault().register(this);

}

@Override

public  void  onStop()  {

EventBus.getDefault().unregister(this);

super.onStop();

}
<a name=PostEvents></a>
### Step 3: Post events

Post an event from any part of your code. All currently registered subscribers matching the event type will receive it.

    EventBus.Default.Post(new MessageEvent("Hello everyone!"));

<a name=LearnMore></a>
### Learn more

Have a look at  [the full documentation](Index.md)  to learn about all features of EventBus.