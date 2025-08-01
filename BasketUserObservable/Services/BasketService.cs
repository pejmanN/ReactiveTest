using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace BasketUserObservable.Services
{
    public class BasketService
    {
        private BehaviorSubject<string> productAddedSubject = new BehaviorSubject<string>("Pejman comes first");

        public IObservable<string> ProductAdded => productAddedSubject.AsObservable();


        public BasketService()
        {
            //Without Subscribing
            //productAddedSubject
            // .Where(x => x != null)
            //.Select(x => x + " suffix added");

            //Empty Subscribe
            productAddedSubject
             .Where(x => x != null)
            .Select(x => x + " suffix added")
            .Subscribe();

            //onNext Subscribe
            productAddedSubject
             .Where(x => x != null)
             .Select(x => x + " suffix added")          
             .Subscribe(
                onNext: val => Console.WriteLine(val),
                onError: ex => Console.WriteLine($"Error: {ex.Message}"),
                onCompleted: () => Console.WriteLine("Sequence completed")
                );


        }
        public void AddProduct(string product)
        {

            productAddedSubject.OnNext(product);


        }
    }
}
