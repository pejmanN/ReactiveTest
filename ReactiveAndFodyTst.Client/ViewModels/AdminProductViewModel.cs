using ReactiveAndFodyTst.Client.Services;
using ReactiveAndFodyTst.Client.Validators;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveAndFodyTst.Client.Models;
using FluentValidation;
using FluentValidation.Results;

namespace ReactiveAndFodyTst.Client.ViewModels
{
    public class AdminProductViewModel : ReactiveObject
    {
        private readonly IProductService _productService;
        private readonly IValidator<Product> _validator;

        [Reactive] public string Name { get; set; }
        [Reactive] public string Description { get; set; }
        [Reactive] public decimal Price { get; set; }
        [Reactive] public List<Product> ProductList { get; private set; }
        [Reactive] public ValidationResult ValidationResult { get; set; }

        public ReactiveCommand<Unit, Unit> AddCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearCommand { get; }

        public AdminProductViewModel(IProductService productService,
                                     IValidator<Product> validator)
        {
            _productService = productService;
            _validator = validator;

            // Subscribe to product updates
            _productService.Products
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(products => ProductList = products);

            //var canAdd = this.WhenAnyValue(x => x.Name, x => x.Price)
            //    .Throttle(TimeSpan.FromMilliseconds(300))
            //    .Select(tuple =>
            //   {
            //       var (name, price) = tuple;
            //       var product = new Product { Name = name, Price = price };
            //       var result = _validator.Validate(product);

            //       ValidationResult = result; // Also update the view
            //       return result.IsValid;
            //   });

            var canAdd = this.WhenAnyValue(
               x => x.Name,  // Subscribe to Name changes
               x => x.Price  // Subscribe to Price changes
             )
            .Do(tuple => Console.WriteLine($"Properties changed: Name={tuple.Item1}, Price={tuple.Item2}"))
            .Select(tuple =>
                   {
                       var (name, price) = tuple;
                       var product = new Product { Name = name, Price = price };
                       var result = _validator.Validate(product);
                       return result.IsValid;
                   })
            .Do(isValid => Console.WriteLine($"Validation result: {isValid}"));


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
