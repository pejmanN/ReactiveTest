
The `BehaviorSubject` its used in `BasketService` is initialized with a default value "Pejman comes first". This means:
- It will always have a value
- New subscribers will immediately receive the latest value
- It maintains the current value and emits it to any new subscriber

`Subject` its used in `BasketService2`: 
- It doesn't require an initial value
- New subscribers will only receive values that are emitted after they subscribe
- It doesn't maintain any current value


### Example
Example with Subject
```
// Example with Subject
void SubjectExample()
{
    var subject = new Subject<string>();
    
    // First emit a value
    subject.OnNext("Apple");
    
    // First subscriber (LATE) - won't receive "Apple"
    subject.Subscribe(x => Console.WriteLine($"Subscriber 1: {x}"));
    
    // Emit more values
    subject.OnNext("Banana");
    subject.OnNext("Orange");
}
```
The output would be:
```
Subscriber 1: Banana
Subscriber 1: Orange
```

Example with BehaviorSubject:
```
// Example with BehaviorSubject
void BehaviorSubjectExample()
{
    var behaviorSubject = new BehaviorSubject<string>("Initial");
    
    // First emit a value
    behaviorSubject.OnNext("Apple");
    
    // First subscriber (LATE) - WILL receive "Apple" immediately
    behaviorSubject.Subscribe(x => Console.WriteLine($"Subscriber 1: {x}"));
    
    // Emit more values
    behaviorSubject.OnNext("Banana");
    behaviorSubject.OnNext("Orange");
}
```
result:
```
Subscriber 1: Apple    // Gets the last value immediately upon subscribing
Subscriber 1: Banana
Subscriber 1: Orange
```


####
In the `BasketService` we have:
Let me explain the key differences between these three approaches:
1. **Without Subscribing**:
``` csharp
productAddedSubject
    .Where(x => x != null)
    .Select(x => x + " suffix added");
```
- This code creates a chain of operations but **nothing happens**
- It's like defining a pipeline but never turning on the water
- No values will be processed because there's no subscriber
- This is completely inactive/dormant code
- Without `.Subscribe()`:
    - ❌ No timer is created
    - ❌ No intervals are generated
    - ❌ No thread scheduling occurs
    - ❌ Nothing happens at all


1. **Empty Subscribe**:
``` csharp
productAddedSubject
    .Where(x => x != null)
    .Select(x => x + " suffix added")
    .Subscribe();
```
- This activates the chain of operations
- Values will be processed through the Where and Select
- But we don't do anything with the results
- Like turning on the water but letting it go down the drain
- Useful when you just want side effects to occur but don't care about the values

1. **onNext Subscribe with handlers**:
``` csharp
productAddedSubject
    .Where(x => x != null)
    .Select(x => x + " suffix added")
    .Subscribe(
        onNext: val => Console.WriteLine(val),
        onError: ex => Console.WriteLine($"Error: {ex.Message}"),
        onCompleted: () => Console.WriteLine("Sequence completed")
    );
```
- This is the most complete form
- Activates the chain AND handles all possible events:
    - : Handles each value (prints it) `onNext`
    - `onError`: Handles any errors that occur
    - `onCompleted`: Handles the completion of the sequence

- Like turning on the water and actually using it for something


