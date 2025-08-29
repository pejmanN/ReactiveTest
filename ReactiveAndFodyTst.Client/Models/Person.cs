using ReactiveUI.Fody.Helpers;

namespace ReactiveAndFodyTst.Client.Models
{
    public class Room
    {
        public int AdultCount { get; set; }
        public int ChildrenCount { get; set; }
        public List<Children> ChildrenList { get; set; } = new();
    }
    public class Children
    {
        public int Age { get; set; }
    }
}
