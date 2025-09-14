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


**ImportanPoint**

I Added `NameTxt` as Reactive to check the concept, if i can change a Reactive prperty in `ViewModel` and in the 
same time listen to its change on `ViewModel` and trigger a pipeline or not, so the answer is `yes`,
I change `NameTxt` in 
```
  this.WhenAnyValue(x => x.IsChecked)
                  .Do(val =>
                  {
                      NameTxt = "pejman is change true checkbox" + val;
                  })
                  .Select(isChecked =>
                  {
                      var res = isChecked ? "Checkbox is checked" : "Checkbox is unchecked";
                      return res;
                  })
                  .ToPropertyEx(this, x => x.ShowCheckboxIsChecked);

```
and when its changed in above pipeline in ViewModel, the following will be triggered:
```
 this.WhenAnyValue(x => x.NameTxt)
                 .Do(x =>
                 {
                     Console.WriteLine(x);
                 })
                 .Skip(1)
                 .Subscribe(txt =>
                 {
                     Console.WriteLine(txt);
                     // do other side-effects here
                 });
```
