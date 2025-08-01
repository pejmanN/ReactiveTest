using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.Reactive.Linq;
using System.Xml.Linq;

namespace ReactiveIFody.ViewModels
{
    public class MyViewModel : ReactiveObject
    {
        public readonly ReactiveCommand<Unit, Unit> Clear;

        #region Text Change
        //private string _text;
        //public string Text
        //{
        //    get => _text;
        //    set => this.RaiseAndSetIfChanged(ref _text, value);
        //} 

        [Reactive]
        public string Text { get; set; }
        #endregion

        #region DisplayText Change
        //private readonly ObservableAsPropertyHelper<string> _displayText;
        //public string DisplayText => _displayText.Value;

        [ObservableAsProperty]
        public string DisplayText { get; set; }
        #endregion


        #region CanClear Change

        //private readonly ObservableAsPropertyHelper<bool> _canClear;
        //public bool CanClear => _canClear.Value;

        [ObservableAsProperty] 
        public bool CanClear { get; set; }
        #endregion

        public MyViewModel()
        {
            #region DisplayText Change

            //_displayText = this.WhenAnyValue(x => x.Text)
            //    //.Where(x =>
            //    //{
            //    //    var res = x != null;
            //    //    return res;
            //    //})
            //    .ToProperty(this, x => x.DisplayText); 

            this.WhenAnyValue(x => x.Text)
                .ToPropertyEx(this, x => x.DisplayText);

            // agar bekhaim ba BindTo() benevisim "DisplayText" dg niaz be [ObservableAsProperty] nadre:
            //public string DisplayText { get; set; }
            //this.WhenAnyValue(x => x.Text)
            //   .BindTo(this, x => x.DisplayText);
            #endregion



            #region CanClear Change
            //_canClear = this.WhenAnyValue(x => x.Text)

            //      .Select(s => CheckItCanBeClear(s))
            //      .ToProperty(this, x => x.CanClear);

            this.WhenAnyValue(x => x.Text)
                .Select(s => CheckItCanBeClear(s))
               .ToPropertyEx(this, x => x.CanClear);
            #endregion


            Clear = ReactiveCommand.CreateFromTask(ClearTest);
            //Clear = ReactiveCommand.Create(() => { Text = string.Empty; }); => in ravesh kar nakard

        }

        private async Task ClearTest()
        {
            await Task.CompletedTask;
            Text = string.Empty;

        }

        private bool CheckItCanBeClear(string x)
        {
            return !string.IsNullOrEmpty(x);
        }
    }
}
