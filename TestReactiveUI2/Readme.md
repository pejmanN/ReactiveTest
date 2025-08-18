 
### Nuget Packages in Reactive Programming

**System.Reactive**: This is the fundamental reactive programming library for .NET that provides the core functionality for working with observable sequences and LINQ-style query operators [[5]](https://stackoverflow.com/questions/34727584/reactiveui-rxui-vs-reactive-extensions).

**ReactiveUI.Blazor**: This is a specialized UI framework built on top of System.Reactive, specifically designed for building reactive user interfaces in Blazor applications1. 
    - [[3]](https://medium.com/@anderson.buenogod/optimizing-iot-dashboards-with-blazor-reactiveui-and-advanced-net-practices-a2dd72dd5693).
 
 Features:
 
        1- MVVM pattern implementation

        2- Automatic UI updates based on observable changes

        3- Memory optimizations for UI scenarios

        4- Component composition tools [[6]](https://github.com/reactiveui/ReactiveUI)




***NOTE*** => *** Its very important to know every viewModel on Razor can be injected with 2 approaches:
1) Just initilizing in 
```
protected override void OnInitialized()
    {
         ViewModel = new MyViewModel2();
     }
```
2)other one is Inject it on razor then pass to ViewModel on OnInitialized :
```
@inject MyViewModel2 _ViewModel
 ViewModel = _ViewModel;

```
and then Register it on Program.cs
```
builder.Services.AddSingleton<MyViewModel2>();
```

***NOTE*** => *** U have dispose any subscriber, either in Razor or ViewModel, otherwise when u even change the page
the subscribe still remain and listen to stream, and it is not performant

# Weather Component
```
 public string Temperature
        {
            get => _temperature;
            set => this.RaiseAndSetIfChanged(ref _temperature, value);
        }
```
1. **`RaiseAndSetIfChanged`**:
    - Part of ReactiveUI's notification system
    - Does three important things:
        1. Checks if the new value is different from the current value
        2. If different, updates the backing field
        3. Notifies subscribers that the property has changed

Here's an example of how it works behind the scine:
``` csharp
private string _temperature; // Stores the actual value

public string Temperature
{
    get => _temperature;  // Returns stored value
    set
    {
        if (_temperature != value)  // Check if value actually changed
        {
            _temperature = value;   // Update the stored value
            // Notify UI and other subscribers that Temperature changed
            NotifyPropertyChanged("Temperature");
        }
    }
}
```
Thepiplie is like:
```
User Action → Set Temperature → RaiseAndSetIfChanged → Update _temperature → Notify UI → UI Updates

```

and in the weather.razor related file we use`ReactiveComponentBase<T>`, 
`ReactiveComponentBase<T>` is a special base class provided by ReactiveUI.Blazor that automatically handles
the connection between ReactiveUI's property changes and Blazor's UI updates


in the weather component if u do:
```
  public string Temperature
        {
            get => _temperature;
            set => _temperature = value;
            //set => this.RaiseAndSetIfChanged(ref _temperature, value);
        }
```
seince there are two others properties that are using `this.RaiseAndSetIfChanged(ref ... , value)`
so the UI related to Temperatur also will be updated, since they are triggered `RaiseAndSetIfChanged` and also in
this senario at the same time values for all of them will be changes.

**Best Parctice is using `RaiseAndSetIfChanged` seperately for everything that needs to reflect the changes on UI


Let me explain the `ObserveOn(RxApp.MainThreadScheduler)` and its importance:
1. **What it does**:
    - Ensures that the subscription (UpdateWeatherData) runs on the main/UI thread
    - Acts like a traffic controller for your observable sequence
    - Switches execution context to the main thread

2. **Why it's important**:
``` csharp
   _weatherUpdates
       .ObserveOn(RxApp.MainThreadScheduler)  // Ensures UI updates happen on main thread
       .Subscribe(UpdateWeatherData);          // Updates UI components
```
1. **If you remove it**:
    - UI updates might happen on background threads
    - Could get errors like:
        - "Cannot access UI elements from a background thread"
        - "Invalid cross-thread operation"

    - UI might freeze or crash
    - Updates might be missed

Here's a practical example:
``` csharp
// BAD - Could crash
_weatherUpdates
    .Subscribe(UpdateWeatherData);  // Might run on background thread

// GOOD - Safe for UI
_weatherUpdates
    .ObserveOn(RxApp.MainThreadScheduler)  // Ensures UI thread
    .Subscribe(UpdateWeatherData);
```
Think of it like this:
``` 
Without ObserveOn:
Background Thread → Direct UI Update ❌ (Crash!)

With ObserveOn:
Background Thread → MainThreadScheduler → UI Update ✅ (Safe!)
```
Common scenarios where `ObserveOn` is crucial:
1. Updating UI elements
2. Modifying observable collections
3. Working with UI-bound properties
4. Handling user interface events

Best practices:
- Always use `ObserveOn(RxApp.MainThreadScheduler)` before UI updates
- Keep it close to the Subscribe call
- Use it when working with UI elements or bound properties


In the following:
```
 // Simulate weather updates every 3 seconds
            _weatherUpdates = Observable
                .Interval(TimeSpan.FromSeconds(3))
                .Select(_ => GenerateWeatherData());

// Subscribe to updates and automatically update the UI
_weatherUpdates
    .ObserveOn(RxApp.MainThreadScheduler)
    .Subscribe(UpdateWeatherData);

```
If u dont want to refrence `_weatherUpdates` in other code,  or expose it , u can rewrite it:
```
 Observable
    .Interval(TimeSpan.FromSeconds(3))
    .Select(_ => GenerateWeatherData())
    .ObserveOn(RxApp.MainThreadScheduler)
    .Subscribe(UpdateWeatherData);
```
***Final Point =>*** when value of a property like `Temperature` is changes, it will raise `RaiseAndSetIfChanged` , since our `razor`
is inherited from `ReactiveComponentBase<T>` and `ReactiveComponentBase<T>` wires itself to the `INotifyPropertyChanged` of the 
ViewModel and, when it sees a change, it schedules a re-render by calling Blazor’s render pipeline
(effectively InvokeAsync(StateHasChanged)). That’s how the markup using @ViewModel.Temperature gets refreshed.

# Search Component

in the Rezor:
- ✅ `@bind-value`: Blazor framework feature
- ✅ `@bind-value:event`: Blazor framework feature
- ❌ Not from System.Reactive
- ❌ Not from ReactiveUI.Blazor


 1. **Angular vs Blazor Binding**:
``` csharp
   // Blazor
   <input @bind-value="ViewModel.SearchTerm" />

   // Angular
   <input [(ngModel)]="searchTerm" />
```
 Both `@bind-value` and `[(ngModel)]` are two-way binding mechanisms in their respective frameworks.

2. **Change Detection and Execution** the exact behavior:
``` csharp
// In your ViewModel
this.WhenAnyValue(x => x.SearchTerm)
    .Throttle(TimeSpan.FromMilliseconds(400))
    .Where(term => !string.IsNullOrWhiteSpace(term))
    .InvokeCommand(SearchCommand);
```
For these two scenarios:

**Scenario 1: Default binding**
``` razor
// Scenario 1: Default binding
<input @bind-value="ViewModel.SearchTerm" />
```
- Updates `SearchTerm ` only when:
    - Input loses focus
    - User presses Enter
    - User tabs out

- since `WhenAnyValue` will be executed when `SearchTerm` is changed, so it will only trigger at above moments


**Scenario 2: oninput binding**
``` razor
// Scenario 2: oninput binding
<input @bind-value="ViewModel.SearchTerm" 
       @bind-value:event="oninput" />
```
- Updates SearchTerm on every keystroke
- `WhenAnyValue` triggers for each character change
- But! The `Throttle(400ms)` means:
    - It waits 400ms after last keystroke
    - If no new keystrokes, then executes
    - If new keystroke comes before 400ms, timer resets

Here's a visualization:
``` 
Scenario 1 (default):
User types "hello" → [no updates] → User tabs out → SearchTerm updates once → WhenAnyValue triggers once

Scenario 2 (oninput):
User types "h" → update → WhenAnyValue triggers
         "e" → update → WhenAnyValue triggers
         "l" → update → WhenAnyValue triggers
         "l" → update → WhenAnyValue triggers
         "o" → update → WhenAnyValue triggers

But with Throttle(400ms):
User types "h" → waits 400ms → executes
         "he" (quickly, it means user after `h` quickly press on `e`) → resets timer
         "hel" (quickly) → resets timer
         "hell" (quickly) → resets timer
         "hello" → waits 400ms → executes once
```
The key differences from Angular's `ngOnChanges`:
1. ReactiveUI's approach is more flexible and powerful
2. You can chain operations (, `Where`, etc.) `Throttle`
3. You set up the change handling in the ViewModel, not the component
4. It's more reactive-programming focused

#### ReactiveCommand
 `ReactiveCommand` is both an Observable (IObservable ) and a Command (ICommand) pattern implementation in ReactiveUI. However, it's not an Observer (it doesn't implement IObserver  ).

 1. **Command Creation**:
``` csharp
ReactiveCommand.CreateFromTask<TParam, TResult>( ... )
                                  ↑        ↑
                               Input     Output
```
2. **Triggering and Execution**:
It can be executed directly using:
```
await SearchCommand.Execute("search term");

```
Or, depend on a condition :
```
this.WhenAnyValue(x => x.SearchTerm)
    .Throttle(TimeSpan.FromMilliseconds(400))
    .Where(term => !string.IsNullOrWhiteSpace(term))
    .InvokeCommand(SearchCommand);  // This executes the command

```

3. **Subscription**:
it can  multiple times  be subscribe:
```
  // Multiple ways to handle results
        SearchCommand.Subscribe(results => SearchResults = results);
        
        // You can have multiple subscribers
        SearchCommand.Subscribe(results => 
            Debug.WriteLine($"Found {results.Count()} results"));

```
4. **key Properties**:
 
can have execute method, and condition to check if satisfied so it can be triggerd like `execute` and `canExecute` in following:
```
SearchCommand = ReactiveCommand.CreateFromTask<string, IEnumerable<SearchResult>>(
                execute: async term =>
                {
                    IsSearching = true;
                    try
                    {
                        return await PerformSearch(term);
                    }
                    finally
                    {
                        IsSearching = false;
                    }
                },
                canExecute: this.WhenAnyValue(x => x.IsSearchEnabled)
            );
```
You can observe different aspects:
```
SearchCommand.IsExecuting
    .Subscribe(isExecuting => IsLoading = isExecuting);

SearchCommand.ThrownExceptions
    .Subscribe(ex => HandleError(ex));

SearchCommand.CanExecute
    .Subscribe(canExecute => CanSearch = canExecute);

```

How `canExecute` is working:
```
canExecute: this.WhenAnyValue(x => x.IsSearchEnabled)

```
he ReactiveCommand infrastructure:

1. Subscribes to this observable

2. Takes each value emitted (true/false)

3. Uses that boolean value to determine if the command can execute

its like u do like above, u pass `this.WhenAnyValue(x => x.IsSearchEnabled)` in to command, and command will
`Subscribe` on it, and check if bool is `True` or `False`, if True it execute it let command to executed, otherwise it wont

also following:
```
   canExecute: this.WhenAnyValue(x => x.IsSearchEnabled)
                                    .Do(isEnabled => Debug.WriteLine($"CanExecute changed to: {isEnabled}"))
```
 `.Do()` operator:
    - Is a side-effect operator
    - Doesn't modify the stream value
    - Just "peeks" at the value passing through
    - Executes the debug write
    - Passes the same value downstream


To make this more clear, let's see what happens when checkbox is clicked:
``` csharp
// When checkbox is checked (IsSearchEnabled = true):
IsSearchEnabled = true
  → WhenAnyValue emits true
  → Do receives true
  → Debug.WriteLine prints "CanExecute changed to: true"
  → true value continues to ReactiveCommand
  → Command becomes executable

// When checkbox is unchecked (IsSearchEnabled = false):
IsSearchEnabled = false
  → WhenAnyValue emits false
  → Do receives false
  → Debug.WriteLine prints "CanExecute changed to: false"
  → false value continues to ReactiveCommand
  → Command becomes non-executable
```


can be rewrite to:
```
canExecute : CheckIfSearchEnabled

private IObservable<bool> CheckIfSearchEnabled()
{
    return this.WhenAnyValue(x => x.IsSearchEnabled)
        .Select(isEnabled =>
        {
            // Add breakpoint here
            Debug.WriteLine($"Search enabled status changed to: {isEnabled}");
            
            if (!isEnabled)
            {
                Debug.WriteLine("Search is disabled - command won't execute");
            }
            else
            {
                Debug.WriteLine("Search is enabled - command can execute");
            }

            // You could add more complex conditions here
            // var finalResult = isEnabled && someOtherCondition;
            
            return isEnabled;
        });
}
```

**IMPORTANT NOTE =>** only looks at the final boolean value that comes out of the observable stream. `canExecute`
Let me demonstrate this with different examples:
``` csharp
// Example 1: Direct boolean value
canExecute: this.WhenAnyValue(x => x.IsSearchEnabled)
// ReactiveCommand sees: true/false directly from IsSearchEnabled

// Example 2: With transformation
canExecute: this.WhenAnyValue(x => x.IsSearchEnabled)
    .Select(isEnabled => !isEnabled) // Inverts the boolean
// ReactiveCommand sees: opposite of IsSearchEnabled

// Example 3: Multiple properties
canExecute: this.WhenAnyValue(
    x => x.IsSearchEnabled,
    x => x.HasSearchText,
    (isEnabled, hasText) => isEnabled && hasText
)
// ReactiveCommand sees: combined boolean result

// Example 4: Complex logic
canExecute: this.WhenAnyValue(x => x.IsSearchEnabled)
    .Select(isEnabled => 
    {
        Debug.WriteLine($"Checking enabled: {isEnabled}");
        var someOtherCondition = DateTime.Now.Hour < 17;
        return isEnabled && someOtherCondition;
    })
// ReactiveCommand sees: final boolean after all calculations
```
The important points:
1. ReactiveCommand **only cares about the final boolean value**
2. It doesn't matter how complex the logic is before
3. The stream must ultimately produce boolean values
4. Each time a new boolean arrives, ReactiveCommand updates its executable state


***NOTE =>*** In this expample we triggered Command according to an value change event, i mean:
```
  this.WhenAnyValue(x => x.SearchTerm)
            .Do(x => Console.WriteLine($"1. Value changed to: {x}"))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Do(x => Console.WriteLine($"2. Non-empty value: {x}"))
            .Throttle(TimeSpan.FromMilliseconds(4000))
            .Do(x => Console.WriteLine($"3. After throttle: {x}"))
            .InvokeCommand(SearchCommand); // Triggering the Command
```
so also it can be triggerd by other events like a buttonClick on UI, and also we use `Subscription` to
use the result of command:
```
    SearchCommand.Subscribe(results => SearchResults = results);

```
It gives us more flexibility to do differnt thing with the result of command, otherwise it could easily set
`SearchResults` in the command, so it does not need to do subscription.

U can check example in `ReactiveAndFodyTst.Client`

#### WhenAnyValue
It checkes if the given Poperty's value is changed or not, if change so it run the given pipleline.
1) when u susbcribe if the value is chnage it will run, so if it gets the same value it does not run the pipline
2) the first time it will be executed without doing any change, with the default value of that control, for example with null

consider if we have 
```
public class SearchViewModel : ReactiveObject
{
    private string _searchTerm;
    public string SearchTerm
    {
        get => _searchTerm;
        set => this.RaiseAndSetIfChanged(ref _searchTerm, value);
    }

    public SearchViewModel()
    {
        this.WhenAnyValue(x => x.SearchTerm)
            .Do(x => Debug.WriteLine($"1. Value changed to: {x}"))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Do(x => Debug.WriteLine($"2. Non-empty value: {x}"))
            .Throttle(TimeSpan.FromMilliseconds(400))
            .Do(x => Debug.WriteLine($"3. After throttle: {x}"))
            .Subscribe(DoSearch);

        // Now let's change the property multiple times
        SearchTerm = "";           // Change 1
        SearchTerm = "h";          // Change 2
        SearchTerm = "he";         // Change 3
        SearchTerm = "hello";      // Change 4
        SearchTerm = "hello";      // Change 5 (same value)
    }
}
```
the result
```

0. Value changed to:  ,(the firs time that compoenent is loaded on UI, the WhenAnyValue will be trigered with null value)
1. Value changed to: 
2. Value changed to: h
2. Non-empty value: h (if user waits to throttle time to pass it will go for `3. After throttle`, if not and press a key immidetly the timer will be reset and the piple will be started from begining)
2. Value changed to: he
2. Non-empty value: he
2. Value changed to: hello
2. Non-empty value: hello
// After 400ms:
3. After throttle: hello
DoSearch called with: hello
// Note: Change 5 doesn't trigger because value didn't change

```
NOTE=> the `Where` condition, is not meet, it does not let the rest of pipeline to be executed, if u are going contol something in middle
of piple to be executed or not, without interfering other steps , u can change as following:
```
this.WhenAnyValue(x => x.SearchTerm)
    .Do(x => Console.WriteLine($"1. Value changed to: {x}"))
    // Remove the Where clause that was blocking everything
    .Select(term => new { 
        Term = term, 
        IsNotEmpty = !string.IsNullOrEmpty(term?.Trim()) 
    })
    .Do(x => {
        if (x.IsNotEmpty)
        {
            Console.WriteLine($"2. Non-empty value: {x.Term}");
        }
    })
    .Select(x => x.Term) // Return back to just the term
    .Throttle(TimeSpan.FromMilliseconds(4000))
    .Do(x => Console.WriteLine($"3. After throttle: {x}"))
    .InvokeCommand(SearchCommand);

```

# MyComponent2

***NOTE=>*** In this Component, it handle proper way to implement `Disposable` pattern for Subsribers.

`ObservableAsPropertyHelper` is related to`ReactiveUI.Blazor`

I define Status ObservableAsPropertyHelper and Status2 with RaiseAndSetIfChanged,the diffence:

in the `RaiseAndSetIfChanged`:
  1-You  want to manually set property. good fro Setting values from service callbacks, etc.
  2-ReactiveUI only handles notifying the UI, so u dont need to call `StateHasChanged` directly
  3- Mutable property that you can set from anywhere so its good for Two-way binding capable (can be both read and written to)
  4- Ui automatically  get updated so u dont have to use `ViewModel.WhenAnyValue(...)` and call `StateHasChanged`



in the `ObservableAsPropertyHelper`:
  1-The property is derived from another observable source.You don’t want to manually set it..
  2-You want to  keep the UI in sync with a stream
  3- Read-only property so its good for One-way binding (can only be read)
  4- Ui automatically does not get updated so u have to use `ViewModel.WhenAnyValue(...)` and call `StateHasChanged`




***NOTE=>*** In the case of using `ObservableAsPropertyHelper` u can handle `StateHasChanged` (to update UI) in following ways :
```
ViewModel.WhenAnyValue(x => x.Status)
    .Subscribe(_ => InvokeAsync(StateHasChanged)); 

```
to
```
 ViewModel.WhenAnyValue(x => x.Status)
            .Subscribe(_ => StateHasChanged());
```
but ` InvokeAsync(StateHasChanged)` is making sure that the change will be done by UI Thread.
-  So ❌ Don't call `StateHasChanged` directly from subscription


and also its better to dispose it using:
```
private readonly CompositeDisposable _disposables = new();

ViewModel.WhenAnyValue(x => x.Status)
           .Subscribe(
               // Next
               _ => InvokeAsync(StateHasChanged),
               // Error
               ex => Console.WriteLine($"Error: {ex.Message}"),
               // Completed
               () => Console.WriteLine("Completed")
           )
           .DisposeWith(_disposables);
```

***NOTE=>*** We have two differnts notify approach `RaiseAndSetIfChanged` and `RaisePropertyChanged`, that can be impelemnted like:
```
private string _status2;
		public string Status2
		{
			get => _status2;
			set => this.RaiseAndSetIfChanged(ref _status2, value);
		}


		private string _status3;
		public string Status3
		{
			get => _status3;
			set
			{
				_status3 = value;
				this.RaisePropertyChanged(nameof(Status3));
			}
		}
```
But they are working slightly differnt:

`RaiseAndSetIfChanged` is a special ReactiveUI method that's specifically designed to work with `ReactiveComponentBase` and ReactiveUI's property change tracking system. It does several things in one call:
1. Sets the value
2. Raises the property changed notification in a way that ReactiveUI components can track
3. Ensures proper integration with ReactiveUI's reactive property system

`RaisePropertyChanged` is just raising a standard `INotifyPropertyChanged` event, which isn't automatically 
hooked into ReactiveUI's reactive property system. so we need to `WhenAnyValue` to update the UI.



***NOTE=>*** we have `.BaindTo(...)` method :

    - One-way binding only (source → target)
    - Used primarily between view model properties
    - Does NOT handle two-way binding between Razor view and ViewModel


if u pay attention i have Status2 whch use `Subscribe` and Status4 which use `BinadTo`, they are almost the same
but with some differnces:

Does exactly the ***same thing***, but:

>1) In `Subscribe` You must manually manage the subscription, but its not like this in `Bindto(..)`

>2) If you forget to dispose `Subscribe` (especially in long-running components), you'll have a memory leak or background updates even when the component is inactive.

>3) `Subscribe` is more Impretive  so u have more control to do some customize things.and `BindTo` is not.
 
>4) `BindTo` automatically observes on `RxApp.MainThreadScheduler` so the assignment is UI-safe, but `Subscribe` you mustdo it by urself.
 
>5)









