using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Linq;

namespace ReactiveIFody.ViewModels
{
    public class OrderViewModel : ReactiveObject
    {


        [ObservableAsProperty]
        public string ShowCheckboxIsChecked { get; set; }
        [Reactive]
        public bool IsChecked { get; set; }


        /// ////////////////////////////


        [Reactive]
        public decimal SubTotal { get; set; }


        private decimal _taxRate;
        public decimal TaxRate
        {
            get => _taxRate;
            set => this.RaiseAndSetIfChanged(ref _taxRate, value);
        }

        private readonly ObservableAsPropertyHelper<decimal> _total;
        public decimal Total => _total.Value;


        public OrderViewModel()
        {
            _total = this.WhenAnyValue(x => x.SubTotal, x => x.TaxRate)
                         .Select(tuple =>
                         {
                             var (subtotal, rate) = tuple;
                             var res = subtotal * rate;
                             return res;
                         })
                         .Do(x =>
                         {
                             Console.WriteLine(x);
                         })
                          .ToProperty(this, x => x.Total);



            this.WhenAnyValue(x => x.IsChecked)
                  .Select(isChecked =>
                  {
                      var res = isChecked ? "Checkbox is checked" : "Checkbox is unchecked";
                      return res;
                  })
                  .ToPropertyEx(this, x => x.ShowCheckboxIsChecked);


        }

    }
}
