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
As u know at the first the compoent is loaded the `AddCommand` will subsctribe to `canAdd`, so as u load the component 
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