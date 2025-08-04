using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Xml.Linq;

namespace TestReactiveUI2.ViewModels
{
	public class MyViewModel2 : ReactiveObject, IDisposable
	{
		private readonly CompositeDisposable _disposables = new();



		// checking ObservableAsPropertyHelper<T> concept
		private readonly ObservableAsPropertyHelper<string> _status;
		public string Status => _status.Value;



		// checking RaiseAndSetIfChanged concept
		private string _status2;
		public string Status2
		{
			get => _status2;
			set => this.RaiseAndSetIfChanged(ref _status2, value);
		}


		// checking RaisePropertyChanged concept
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



		// checking BindTo(...) concept
		private string _status4;
		public string Status4
		{
			get => _status4;
			set => this.RaiseAndSetIfChanged(ref _status4, value);
		}


		public MyViewModel2()
		{
			var statusStream = Observable.Interval(TimeSpan.FromSeconds(30))
										 .Select(_ => $"Updated at {DateTime.Now}")
										 .Do(value =>
										 {
											 Console.WriteLine("statusStream");
										 });
			_status = statusStream
				.ToProperty(this, x => x.Status)
				.DisposeWith(_disposables);



			var statusStream2 = Observable.Interval(TimeSpan.FromSeconds(20))
									 .Select(_ => $"Updated at {DateTime.Now}");
			statusStream2
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(value =>
				{
					Console.WriteLine($"statusStream2 | {value}");
					Status2 = value;
				})
				.DisposeWith(_disposables);


			var statusStream3 = Observable.Interval(TimeSpan.FromSeconds(10))
								 .Select(_ => $"Updated at {DateTime.Now}");
			statusStream2
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(value =>
				{
					Console.WriteLine($"statusStream3 | {value}");
					Status3 = value;
				})
				.DisposeWith(_disposables);



			var statusStream4 = Observable.Interval(TimeSpan.FromSeconds(1))
							 .Select(_ => $"Updated at {DateTime.Now}");
			statusStream4
				.ObserveOn(RxApp.MainThreadScheduler)
				.Do(value => Console.WriteLine($"statusStream4 | {value}"))
				.BindTo(this, x => x.Status4)
				.DisposeWith(_disposables);
		}

		public void Dispose()
		{
			_disposables.Dispose();
		}
	}
}
