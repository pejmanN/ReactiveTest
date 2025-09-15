using ReactiveUI.Fody.Helpers;
using ReactiveUI;

namespace MudTst.ViewModels
{
    public class Person : ReactiveObject
    {
        public int Id { get; }                    // immutable id
        public string Name { get; set; }
        public int PositionInList { get; set; }
        public bool IsDragOver { get; set; }

        public Person(int id, string name, int position)
        {
            Id = id;
            Name = name;
            PositionInList = position;
        }
    }
}
