using ReactiveAndFodyTst.Client.Services;
using ReactiveAndFodyTst.Client.Validators;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveAndFodyTst.Client.Models;

namespace ReactiveAndFodyTst.Client.ViewModels
{
    public class AdminProductViewModel : ReactiveObject
    {
        private readonly IProductService _productService;
        private readonly ProductValidator _validator;

        [Reactive] public string Name { get; set; }
        [Reactive] public string Description { get; set; }
        [Reactive] public decimal Price { get; set; }
        [Reactive] public List<Product> ProductList { get; private set; }

        public ReactiveCommand<Unit, Unit> AddCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearCommand { get; }

        public AdminProductViewModel(IProductService productService)
        {
            _productService = productService;
            _validator = new ProductValidator();

            // Subscribe to product updates
            _productService.Products
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(products => ProductList = products);

            // Setup validation
            var canAdd = this.WhenAnyValue(
                x => x.Name,
                x => x.Price,
                (name, price) =>
                {
                    var product = new Product { Name = name, Price = price };
                    var result = _validator.Validate(product);
                    return result.IsValid;
                });

            AddCommand = ReactiveCommand.Create(ExecuteAdd, canAdd);
            ClearCommand = ReactiveCommand.Create(ExecuteClear);
        }

        private void ExecuteAdd()
        {
            var product = new Product
            {
                Name = Name,
                Description = Description,
                Price = Price
            };

            _productService.AddProduct(product);
            ExecuteClear();
        }

        private void ExecuteClear()
        {
            Name = string.Empty;
            Description = string.Empty;
            Price = 0;
        }
    }

}
