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
so if we change foremaple `TaxRate` to a normal Auto-Property since it does not fire this event, so 
`_total = this.WhenAnyValue(x => x.SubTotal, x => x.TaxRate)...` wont work.