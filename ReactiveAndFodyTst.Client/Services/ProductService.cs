using ReactiveAndFodyTst.Client.Models;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ReactiveAndFodyTst.Client.Services
{
    public class ProductService : IProductService
    {
        private readonly BehaviorSubject<List<Product>> _products;

        public ProductService()
        {
            _products = new BehaviorSubject<List<Product>>(new List<Product>());
        }

        public IObservable<List<Product>> Products => _products.AsObservable();

        public void AddProduct(Product product)
        {
            var currentList = _products.Value.ToList();
            currentList.Add(product);
            _products.OnNext(currentList);
        }
    }

}
