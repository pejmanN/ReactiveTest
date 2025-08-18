using FluentValidation;
using Microsoft.AspNetCore.Components;
using ReactiveAndFodyTst.Client.Services;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;
using ReactiveAndFodyTst.Client.Models;
using System.Reactive.Linq;

namespace ReactiveAndFodyTst.Client.ViewModels
{
    public class ProductEditViewModel : ReactiveObject
    {
        private readonly IProductService _productService;
        private readonly IValidator<Product> _validator;
        private readonly NavigationManager _navigationManager;

        [Reactive]
        public string Name { get; set; }

        [Reactive]
        public decimal Price { get; set; }

        private Guid _productId;

        public ReactiveCommand<Unit, Unit> UpdateCommand { get; }

        public ProductEditViewModel(
            IProductService productService,
            IValidator<Product> validator,
            NavigationManager navigationManager)
        {
            _productService = productService;
            _validator = validator;
            _navigationManager = navigationManager;

            var canUpdate = this.WhenAnyValue(
                x => x.Name,
                x => x.Price
            )
            .Select(tuple =>
            {
                var (name, price) = tuple;
                var product = new Product { Name = name, Price = price };
                var result = _validator.Validate(product);
                return result.IsValid;
            });

            UpdateCommand = ReactiveCommand.Create(ExecuteUpdateAsync, canUpdate);
        }

        public async Task LoadProduct(Guid id)
        {
            await Task.CompletedTask;
            _productId = id;
            var product = _productService.GetById(id);
            if (product != null)
            {
                Name = product.Name;
                Price = product.Price;
            }
        }

        private void ExecuteUpdateAsync()
        {
            var product = new Product
            {
                Id = _productId,
                Name = Name,
                Price = Price
            };

            _productService.UpdateProduct(product);
            _navigationManager.NavigateTo("/adminproduct");
        }
    }
}
