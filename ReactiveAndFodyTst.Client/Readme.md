# Combined Example
Try to implement example combine differnt technique


## AdminProductViewModel
The stroy start from this component.

***NOTE =>***  In the Add New Product process i was going to check if the validation is passd, then let 
the use Add new product, so we write validator using FluentValidation, then write `canAdd` observable to prevent
`AddCommand` to be executed if the requirement were not meet (which is passed as `canExecute` to AddCommand):
```
 var canAdd = this.WhenAnyValue(
               x => x.Name,  // Subscribe to Name changes
               x => x.Price  // Subscribe to Price changes
             )
            .Do(tuple => Console.WriteLine($"Properties changed: Name={tuple.Item1}, Price={tuple.Item2}"))
            .Select(tuple =>
                   {
                       var (name, price) = tuple;
                       var product = new Product { Name = name, Price = price };
                       var result = _validator.Validate(product);
                       return result.IsValid;
                   })
            .Do(isValid => Console.WriteLine($"Validation result: {isValid}"));


            AddCommand = ReactiveCommand.Create(ExecuteAdd, canAdd);
```
As u know at the first time that the compoent is loaded the `AddCommand` will subsctribe to `canAdd`, so as u load the component 
u can see in `output` following logs:
```
Properties changed: Name=, Price=0
Validation result: False
```
which are related the log we put in `.Do` for `canAdd`.


The issue here is the way i called this `AddCoomand` from `razor` does not respect the canAdd :
```
    <button class="btn btn-primary" 
        @onclick="@(() => ViewModel.AddCommand.Execute().Subscribe())">Add New Product</button>

```
, so we have to disable the button using:
```
<button class="btn btn-primary"
        disabled="@(!ViewModel.AddCommand.CanExecute.FirstAsync().Wait())"
        @onclick="@(() => ViewModel.AddCommand.Execute().Subscribe())">
    Add
</button>
```
or change `onclick` like following:
```
<button class="btn btn-primary"
        @onclick="@OnAddClicked"
        disabled="@(!ViewModel.AddCommand.CanExecute.FirstAsync().Wait())">
    Add
</button>


```

and in the `@code`:
```
private async Task OnAddClicked()
{
    var canExecute = await ViewModel.AddCommand.CanExecute.FirstAsync();
    if (canExecute)
    {
        await ViewModel.AddCommand.Execute();
    }
}
```


but we did the `canExecute`  for `command` in the `SearchViewModel` in `TestReactiveUI2` project,
and it works properly, since we use `.InvokeCommand(SearchCommand);` since in its impelmtatin , it will 
check if `canExecute` is `True`.

> So Its Really Important That Check If Different Method in a Framework or Differnt Frameworks to EachOther
They Respect About Their Feature or Events or Not.


#### ObservableCollection
ObservableCollection in a nutshell 
- It’s a collection type from System.Collections.ObjectModel designed for data binding.
- It implements:
    - INotifyCollectionChanged: raises CollectionChanged when items are added, removed, moved, replaced, or when the list is reset.
    - INotifyPropertyChanged: raises PropertyChanged for Count and the indexer when the collection changes.

- UIs (WPF/WinUI/etc.) listen to those events to automatically refresh the view when the collection changes.
- It is not thread-safe; you must modify it on the UI thread.

Why use it here
- Local, UI-friendly cache: _products holds the current products in memory. Because it’s an ObservableCollection, any structural change can be observed reactively.
- Integration with DynamicData: _products.ToObservableChangeSet(p => p.Id) converts collection change notifications into a dynamic “change set” stream keyed by Id. From there:
    - ToCollection() materializes snapshots (IReadOnlyCollection ) whenever the collection changes. 
    - Those snapshots are combined with your ProjectionFunction to produce Filtered.

- Safe threading: The code calls ObserveOn(RxApp.MainThreadScheduler) before clearing/adding items to ensure all mutations happen on the UI thread (required by ObservableCollection).
- Batched updates: AddRange(list) (extension from DynamicData.Binding) batches additions so the UI receives fewer, larger notifications instead of one per item, improving performance.

What it does and doesn’t notify
- It notifies about structural changes (add/remove/clear/replace/reset).
- It does not automatically notify when a property on an item changes. For that:
    - Each Product should implement INotifyPropertyChanged so bindings to Product properties update.
    - If you need the filtering pipeline to react to item property changes, you’d either re-emit from the service or use DynamicData operators like AutoRefresh on the change set.

Contrast with other collections
- List : No change notifications; the UI won’t update automatically. 
- ReadOnlyObservableCollection : Read-only wrapper suitable for exposing to the UI while mutating an underlying observable collection. 
- DynamicData sources (SourceList/SourceCache): Better for high-frequency/large datasets; produce change sets directly and can be bound to a ReadOnlyObservableCollection for the UI. They’re often preferred for more complex scenarios.

Tiny example of notifications
``` csharp
// C#
var coll = new ObservableCollection<string>();
coll.CollectionChanged += (s, e) =>
{
    // e.Action tells you Add/Remove/Reset/etc.
    // e.NewItems / e.OldItems provide the changed items
};
coll.Add("A");   // raises CollectionChanged(Add), PropertyChanged(Count)
coll.Clear();    // raises CollectionChanged(Reset), PropertyChanged(Count)
```
Bottom line
- ObservableCollection is used here as a UI-friendly, observable local cache. 
- DynamicData listens to it, converts changes to a change set, materializes snapshots, and those snapshots are then filtered by the latest ProjectionFunction to produce the final Filtered result.
product
product





## RoomViewModel
1) If u have a normal property (without any decoration and attributes) in ur viewmodel like:
```
  public int RoomCount { get; set; }
```
and in razor:
```
<input type="text" class="form-control" @bind-value="ViewModel.RoomCount" @bind-value:event="oninput" />
```
***IF*** u change this RoomCount from razor, the RoomCount in viewmodel will be updated, it means if u put number
in html input, or in `@code {}` in the razor u do something like `ViewModel.RoomCount = 10`, for test create a button
and call `OnInitialized` for ` @onclick="@OnAddClicked"` event.
```
@code {
    protected override void OnInitialized()
    {
        ViewModel ??= _viewModel;
    }

    private async Task OnAddClicked()
    {
        ViewModel.RoomCount = 10;
        // ViewModel.AddRoom();
    }

}
```

***IF*** u change this RoomCount from Viewmodel, the razor did not get updated. for test u can create a button like
above but in the `@code` u have to call method from Viewmodel
```

@code {
    protected override void OnInitialized()
    {
        ViewModel ??= _viewModel;
    }

    private async Task OnAddClicked()
    {
         ViewModel.AddRoom();
    }
}
```
and in viewmodel:
```
  public void AddRoom()
        {
            var t = RoomCount;
            RoomCount = 3;
        }
```

***NOTE =>*** There are 2 approached to detecting ChildrentCount changes, 
1) we used `oninput` like:
```
 <input type="text" class="form-control" @bind-value="room.ChildrenCount"
                           @oninput="@(e => ViewModel.OnChildrenCountChanged(room, e))" />
```
which call the `OnChildrenCountChanged` from ViewModel, its working fine.

2) if u are going to use ReactiveFunction, u have to something like following:

in view model
```
  this.WhenAnyValue(x => x.RoomCount)
           .Subscribe(value =>
           {
               RoomList.Clear();
               for (int i = 0; i < this.RoomCount; i++)
               {
                   var room = new Room();

                   // Subscribe to ChildrenCount changes for each room
                   room.WhenAnyValue(x => x.ChildrenCount)
                       .Subscribe(childrenCount =>
                       {
                           room.ChildrenList.Clear();
                           for (int j = 0; j < childrenCount; j++)
                           {
                               room.ChildrenList.Add(new Children());
                           }
                       });

                   RoomList.Add(room);
               }
           });
```
and razor:
```
  <label>ChildrenCount:</label>
  <input type="number" class="form-control" @bind-value="room.ChildrenCount" @bind-value:event="oninput" />
```
and define `Room` as `ReactiveObject` like following:
```
 public class Room : ReactiveObject
    {
        [Reactive] public int AdultCount { get; set; }
        [Reactive] public int ChildrenCount { get; set; }
        [Reactive] public List<Children> ChildrenList { get; set; } = new();
    }
    public class Children
    {
        public int Age { get; set; }
    }
```

