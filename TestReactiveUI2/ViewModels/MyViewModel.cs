using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Xml.Linq;

namespace TestReactiveUI2.ViewModels
{
    public class MyViewModel : ReactiveObject
    {
        public readonly ReactiveCommand<Unit, Unit> Clear;

        private string _text;
        public string Text
        {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }

        private readonly ObservableAsPropertyHelper<string> _displayText;
        public string DisplayText => _displayText.Value;



        private readonly ObservableAsPropertyHelper<bool> _canClear;
        public bool CanClear => _canClear.Value;

        public MyViewModel()
        {

            #region 2 rahe hal dare tavajo kon
            _displayText = this.WhenAnyValue(x => x.Text)
                 
                    .ToProperty(this, x => x.DisplayText);

            //be jaye in mavarede bala in hame harkataye ajib gharib, be jash bia 1 property ma'mooli va chanta khat code mesle pain benivis hamin kare balaro anjam mide
            //public string DisplayText { get; set; }
            //this.WhenAnyValue(x => x.Text)
            // .BindTo(this, x => x.DisplayText);

            #endregion

            _canClear = this.WhenAnyValue(x => x.Text)
                // .Where(x => !string.IsNullOrEmpty(x))
                .Select(s => CheckItCanBeClear(s))
                .ToProperty(this, x => x.CanClear);


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
