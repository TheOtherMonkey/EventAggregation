# EventAggregation
A centralised in-memory event aggregator written using .NET Standard 2.0.

For this component to be used successfully, there needs to be one and only one instance of the EventAggregator in existence during the lifetime of the application where the EventAggregator is being used. The easiest way to enforce the singleton requirement
for this class is to utilise a Dependency Injection (DI) framework and declare the lifetime of the EventAggregator as a singleton.

In order to get the most out of this component, the DI framework should identify objects that implement the IListenFor<T> interface and automatically register the newly constructed object with the EventAggregator by making a call to the AddListener method
prior to returning the new instance.

Of course, this automagic registration of objects constructed by a DI framework is not a requirement for the use of the EventAggregator but it does make using it a lot more convenient. If no DI framework is being used (or the DI framework does not support custom type interception) the AddListener method needs to be called in order to register an object as a listener.
