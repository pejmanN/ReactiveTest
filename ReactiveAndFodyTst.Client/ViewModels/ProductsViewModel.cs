using ReactiveAndFodyTst.Client.Services;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using ReactiveAndFodyTst.Client.Models;
using System.Reactive.Linq;

namespace ReactiveAndFodyTst.Client.ViewModels
{
    public class ProductsViewModel : ReactiveObject
    {
        private readonly IProductService _productService;
        [Reactive] public List<Product> Products { get; private set; }

        public ProductsViewModel(IProductService productService)
        {
            _productService = productService;

            _productService.Products
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(products => Products = products);
        }
    }

}
