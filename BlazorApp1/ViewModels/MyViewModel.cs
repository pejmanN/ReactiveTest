using ReactiveUI.Fody.Helpers;
using ReactiveUI;

namespace BlazorApp1.ViewModels
{
    //show to-way binging
    public class MyViewModel : ReactiveObject
    {
        private string _reactiveProperty;
        private string _nonReactiveProperty;
        [Reactive]
        public string ReactiveProperty
        {
            get
            {
                return _reactiveProperty;
            }
            set
            {
                _reactiveProperty = value;
                //this.RaiseAndSetIfChanged(ref _count, value);
                System.Diagnostics.Debug.WriteLine($"Reactive Property changed. New value: {value}");
            }
        }

        public string NonReactiveProperty {
            get
            {
                return _nonReactiveProperty;
            }
            set
            {
                _nonReactiveProperty = value;
                //this.RaiseAndSetIfChanged(ref _count, value);
                System.Diagnostics.Debug.WriteLine($"NonReactiveProperty property changed. New value: {value}");
            }
        }
    }
}
