# Fody

ReactiveUI.Fody is an optional add-on for ReactiveUI that uses Fody, a .NET IL-weaving tool,
to automatically inject boilerplate code — especially around `RaiseAndSetIfChanged`.

🔧 What does it do?
It automatically generates RaiseAndSetIfChanged for you behind the scenes, so you don’t need to 
write repetitive code like this:
```
private string _name;
public string Name
{
    get => _name;
    set => this.RaiseAndSetIfChanged(ref _name, value);
}

```
to following:
```
[Reactive]
public string Name { get; set; }

```

# OrderComponent
In this component, i tried to prove above assumption about `[Reactive]` attribute, i tried to use both
`RaiseAndSetIfChanged` and `[Reactive]`, i showed that the functionality are the same.


***NOTE =>*** One important thing, the `RaiseAndSetIfChanged` as u know , it update the UI and also it
raise `INotifyPropertyChanged` which `ReactiveUI` use it in order to trigger `WhenAnyValue`,
so if we change for exmaple `SubTotal` to a normal `Auto-Property`, when do change on it on UI ,since it does not fire that event, 
the following pipleine is not fired `_total = this.WhenAnyValue(x => x.SubTotal, x => x.TaxRate)...` 



#### [ObservableAsProperty] Attribute
this attribute is doing like `ObservableAsPropertyHelper<T>`, but it removes boilder plate codes,
i used that attribute for `ShowCheckboxIsChecked` to show the status of checkbox, if u pay attention in the
```

this.WhenAnyValue(x => x.IsChecked)
        .Select(isChecked =>
        {
            var res = isChecked ? "Checkbox is checked" : "Checkbox is unchecked";
            return res;
        })
        .ToPropertyEx(this, x => x.ShowCheckboxIsChecked);
```
contrary to `Total` i used `ToPropertyEx` instead of `ToProperty`, `ToPropertyEx` is Fody Helper, which help us 
to update the ui without to write `ViewModel.WhenAnyValue` on `razor`, which we have to do for `Total` to 
update the UI `ViewModel.WhenAnyValue(x => x.Total) ....` in Razor.
