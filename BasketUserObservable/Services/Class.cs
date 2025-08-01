namespace BasketUserObservable.Services
{
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    namespace BasketUserObservable.Services
    {
        public class BasketService2
        {
            private Subject<string> productAddedSubject = new Subject<string>();
            private IObservable<string> transformedProducts; // New stream for transformed products

            public IObservable<string> ProductAdded => transformedProducts ??= InitializeTransformedProductsStream();

            public BasketService2()
            {
                // Transform the products
                transformedProducts = productAddedSubject
                    .Where(x => x != null)
                    .Select(x => x + " suffix added");
            }

            private IObservable<string> InitializeTransformedProductsStream()
            {
                return transformedProducts ??= productAddedSubject
                    .Where(x => x != null)
                    .Select(x => x + " suffix added");
            }

            public void AddProduct(string product)
            {
                productAddedSubject.OnNext(product);
            }
        }
    }

}
