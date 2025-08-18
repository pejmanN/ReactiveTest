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

        public Product GetById(Guid id)
        {
            return _products.Value.FirstOrDefault(p => p.Id == id);
        }

        public void UpdateProduct(Product product)
        {
            var existingProduct = _products.Value.FirstOrDefault(p => p.Id == product.Id);
            if (existingProduct != null)
            {
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                _products.OnNext(_products.Value);
            }
        }
    }

}
