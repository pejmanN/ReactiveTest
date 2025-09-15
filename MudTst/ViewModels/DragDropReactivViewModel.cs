using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using static MudTst.Pages.DragDropTable;
using System.Collections.ObjectModel;

namespace MudTst.ViewModels
{
    public class DragDropReactivViewModel : ReactiveObject
    {
        public ObservableCollection<Person> People { get; } = new();

        public Person? Dragged { get; set; }

        public DragDropReactivViewModel()
        {
            // seed data
            for (int i = 1; i <= 12; i++)
                People.Add(new Person(i, $"P {i}", i));
        }

        public void DragStart(Person person) => Dragged = person;
        public void DragEnd() => Dragged = null;

        public void DragEnter(Person person)
        {
            if (!ReferenceEquals(person, Dragged))
                person.IsDragOver = true;
        }

        public void DragLeave(Person person) => person.IsDragOver = false;

        public void DropOn(Person target)
        {
            if (Dragged is null) return;

            var from = People.IndexOf(Dragged);
            var to = People.IndexOf(target);
            if (from < 0 || to < 0 || from == to)
            {
                target.IsDragOver = false;
                return;
            }

            People.RemoveAt(from);

            int insertIndex = from > to ? to : to - 1;
            if (insertIndex > People.Count) insertIndex = People.Count;
            People.Insert(insertIndex, Dragged);

            for (int i = 0; i < People.Count; i++)
                People[i].PositionInList = i + 1;

            foreach (var p in People) p.IsDragOver = false;

            Dragged = null;
        }
    }
}
