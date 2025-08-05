using ReactiveAndFodyTst.Client.Models;

namespace ReactiveAndFodyTst.Client.Services
{
    public interface IProductService
    {
        IObservable<List<Product>> Products { get; }
        void AddProduct(Product product);

    }
}
