//using ReactiveUI;
//using ReactiveUI.Fody.Helpers;
//using System;
//using System.Reactive;
//using System.Reactive.Linq;
//using System.Runtime.ConstrainedExecution;
//using System.Threading.Tasks;

//public class CounterViewModel : ReactiveObject
//{
//    private int _count;
//    private string _searchText;
//    private ObservableAsPropertyHelper<bool> _canClear;


//    [Reactive]
//    public int Count
//    {
//        get => _count;
//        set
//        {
//            _count = value;
//            System.Diagnostics.Debug.WriteLine($"Count property changed. New value: {value}");
//        }
//    }
//    public string SearchText
//    {
//        get => _searchText;
//        set => this.RaiseAndSetIfChanged(ref _searchText, value);
//    }

//    [ObservableAsProperty]
//    public string Message { get; }

//    public bool CanClear => _canClear.Value;



//    public ReactiveCommand<Unit, Unit> Clear { get; set; }

//    public ReactiveCommand<Unit, Unit> IncrementCommand { get; }




//    public CounterViewModel()
//    {
//        IncrementCommand = ReactiveCommand.CreateFromTask(IncrementAsync);

//        this.WhenAnyValue(x => x.Count)
//            .Where(x => x > 0)
//            .Select(count =>
//            {
//                var message = $"Current count is {count}";
//                System.Diagnostics.Debug.WriteLine($"Message property updated. New value: {message}");
//                return message;
//            })
//        .ToPropertyEx(this, x => x.Message);


//        Clear = ReactiveCommand.Create(() => { }, this.WhenAnyValue(x => x.SearchText).Select(x => !string.IsNullOrEmpty(x)));

//        Clear
//            .Subscribe(_ => SearchText = string.Empty);

//        Clear.CanExecute
//            .ToProperty(this, nameof(CanClear), out _canClear);
//    }

//    private async Task IncrementAsync()
//    {
//        // Add any asynchronous logic here
//        await Task.Delay(10); // Example: Simulate an async operation

//        Count++;
//    }
//}
