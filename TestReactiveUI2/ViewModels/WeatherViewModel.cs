
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace TestReactiveUI2.ViewModels
{
    public class WeatherViewModel : ReactiveObject
    {
        private string _temperature;
        private string _humidity;
        private string _windSpeed;

        public string Temperature
        {
            get => _temperature;
            set => this.RaiseAndSetIfChanged(ref _temperature, value);
        }

        public string Humidity
        {
            get => _humidity;
            set => this.RaiseAndSetIfChanged(ref _humidity, value);
        }

        public string WindSpeed
        {
            get => _windSpeed;
            set => this.RaiseAndSetIfChanged(ref _windSpeed, value);
        }

        private IObservable<string> _weatherUpdates;

        public WeatherViewModel()
        {
            // Simulate weather updates every 3 seconds
            _weatherUpdates = Observable
                .Interval(TimeSpan.FromSeconds(3))
                .Select(_ => GenerateWeatherData());

            // Subscribe to updates and automatically update the UI
            _weatherUpdates
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(UpdateWeatherData);
        }

        private string GenerateWeatherData()
        {
            var temp = Random.Shared.Next(-10, 35);
            var humidity = Random.Shared.Next(30, 90);
            var windSpeed = Random.Shared.Next(0, 30);
            return $"{temp}|{humidity}|{windSpeed}";
        }

        private void UpdateWeatherData(string data)
        {
            var parts = data.Split('|');
            Temperature = $"{parts[0]}°C";
            Humidity = $"{parts[1]}%";
            WindSpeed = $"{parts[2]} km/h";
        }
    }
}