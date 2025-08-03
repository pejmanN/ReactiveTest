using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Xml.Linq;

namespace TestReactiveUI2.ViewModels
{
	public class MyViewModel2 : ReactiveObject
	{
		private readonly ObservableAsPropertyHelper<string> _status;
		public string Status => _status.Value;


		private string _status2;
		public string Status2
		{
			get => _status2;
			set => this.RaiseAndSetIfChanged(ref _status2, value);
		}

		public MyViewModel2()
		{
			var statusStream = Observable.Interval(TimeSpan.FromSeconds(10))
										 .Select(_ => $"Updated at {DateTime.Now}")
										 .Do(value =>
										 {
											 Console.WriteLine("statusStream");
										 });
			_status = statusStream
				.ToProperty(this, x => x.Status);



			var statusStream2 = Observable.Interval(TimeSpan.FromSeconds(1))
									 .Select(_ => $"Updated at {DateTime.Now}");
			statusStream2
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(value =>
				{
					Console.WriteLine($"statusStream2 | {value}");
					Status2 = value;
				});
		}
	}
}
