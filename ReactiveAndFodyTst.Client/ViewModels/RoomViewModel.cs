using Microsoft.AspNetCore.Components;
using ReactiveAndFodyTst.Client.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ReactiveAndFodyTst.Client.ViewModels
{
    public class RoomViewModel : ReactiveObject
    {
        public int ChangeFromViewModel { get; set; }

        public List<Room> RoomList { get; private set; } = [];
        [Reactive] public int RoomCount { get; set; }


        public RoomViewModel()
        {
            this.WhenAnyValue(x => x.RoomCount)
                .Subscribe(value =>
                {
                    ChangeFromViewModel = 4;

                    RoomList.Clear();
                    for (int i = 0; i < this.RoomCount; i++)
                    {
                        RoomList.Add(new Room());
                    }

                });

        }

        public void OnChildrenCountChanged(Room room, ChangeEventArgs e)
        {
            if (e.Value is null) return;
            if (int.TryParse(e.Value?.ToString(), out int value))
            {
                ChangeFromViewModel = 5;
                room.ChildrenList.Clear();
                for (int i = 0; i < value; i++)
                {
                    room.ChildrenList.Add(new Children());
                }
            }

            
        }
        public void Save()
        {

            var t = RoomList;
        }

    }
}
