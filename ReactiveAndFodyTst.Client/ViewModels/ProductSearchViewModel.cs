using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveAndFodyTst.Client.Models;
using ReactiveAndFodyTst.Client.Services;
using System.Security.Principal;
using System.Xml.Linq;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.Threading;

namespace ReactiveAndFodyTst.Client.ViewModels
{
    public class ProductSearchViewModel : ReactiveObject, IDisposable
    {
        private readonly IProductService _productService;
        private readonly CompositeDisposable _disposables = new();

        // Local cache we can observe with DynamicData
        //we are going to use this property in CombineLatest, so we need Observable type of collection
        private readonly ObservableCollection<Product> _products = new();


        [Reactive] public string? SearchText { get; set; } = string.Empty;

        // A projection function derived from SearchText
        [ObservableAsProperty]
        public Func<IReadOnlyCollection<Product>, IReadOnlyCollection<Product>>? ProjectionFunction { get; }

        // Final filtered list (bindable)
        [ObservableAsProperty]
        public IReadOnlyCollection<Product>? Filtered { get; }

        public ProductSearchViewModel(IProductService productService)
        {
            _productService = productService;

            // Keep the ObservableCollection<Product> in sync with the latest products
            _productService.Products
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(list =>
                {
                    _products.Clear();
                    //foreach (var p in list)
                        _products.AddRange(list);
                })
                .DisposeWith(_disposables);





            // Build a projection function from SearchText, - An observable watches SearchText changes
            this.WhenAnyValue(vm => vm.SearchText)
                .Select(t => t?.Trim() ?? string.Empty)
                .Throttle(TimeSpan.FromMilliseconds(250), RxApp.TaskpoolScheduler)
                .DistinctUntilChanged() //- DistinctUntilChanged ensures identical consecutive terms don’t reprocess.

    //            * That stabilized term is transformed into a function:
    //             -If the term is empty / whitespace, the function returns the input items unchanged(identity).
    //             - Otherwise, the function returns only items whose Name or Description contains the term(case‑insensitive).
                .Select(term => (Func<IReadOnlyCollection<Product>, IReadOnlyCollection<Product>>)(items =>
                {
                    if (string.IsNullOrWhiteSpace(term))
                        return items;

                    return items
                        .Where(p =>
                            (!string.IsNullOrEmpty(p.Name) && p.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                            (!string.IsNullOrEmpty(p.Description) && p.Description.Contains(term, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }))
                .ToPropertyEx(this, vm => vm.ProjectionFunction) //- above function is stored in ProjectionFunction via ToPropertyEx, which raises PropertyChanged for ProjectionFunction.
                .DisposeWith(_disposables);





            // Combine the latest projection function with a change set created from the ObservableCollection
            // ToObservableChangeSet: observe changes of the in-memory collection (keyed by Id)
            // ToCollection: materialize it to a bindable IReadOnlyCollection<Product>

            // CombineLatest: it pairs the latest value from A with the latest value from B, so in our case it pairs the projection function with the latest collection,
            //-CombineLatest pairs the latest value from A with the latest value from B:
            //    -Important: CombineLatest does not emit until both A and B have produced at least one value.
            //    -After the first emission, any new value from A is immediately paired with “the latest B,” and any new value from B is paired with “the latest A.”
            //    -It always emits a tuple containing “the latest from both,” even if only one source just changed.

            // Switch: ensure we only publish the most recent result if updates come fast
            this.WhenAnyValue(vm => vm.ProjectionFunction)
                .WhereNotNull()
                .CombineLatest(
                    _products
                        .ToObservableChangeSet(p => p.Id)
                        .ToCollection()
                )
                //At this point because of CombineLatest, we have a pair, First is the latest projection-function, second is the latest productList
                // Turn the pair into an inner observable to demonstrate Switch (and avoid stale emissions)
                .Select(pair => Observable.Return(pair.First.Invoke(pair.Second)))

                //-It’s there as a safety / future‑proofing pattern: if projection later becomes asynchronous or long‑running, Switch prevents stale results from earlier computations overriding newer ones.
                .Switch()
            .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, vm => vm.Filtered)
                .DisposeWith(_disposables);
        }

        public void Dispose() => _disposables.Dispose();
    }
}