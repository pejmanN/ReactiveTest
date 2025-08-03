using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TestReactiveUI2.Pages;

namespace TestReactiveUI2.ViewModels
{
    public class SearchViewModel : ReactiveObject
    {
        private string _searchTerm;
        private bool _isSearching;
        private IEnumerable<SearchResult> _searchResults;
        private bool _isSearchEnabled; // this is for checking if enable so the SearchCommand can be triggered, otherwise it cant be triggerd


        public string SearchTerm
        {
            get => _searchTerm;
            set => this.RaiseAndSetIfChanged(ref _searchTerm, value);
        }

        public bool IsSearching
        {
            get => _isSearching;
            set => this.RaiseAndSetIfChanged(ref _isSearching, value);
        }

        public IEnumerable<SearchResult> SearchResults
        {
            get => _searchResults;
            set => this.RaiseAndSetIfChanged(ref _searchResults, value);
        }

        public bool IsSearchEnabled
        {
            get => _isSearchEnabled;
            set => this.RaiseAndSetIfChanged(ref _isSearchEnabled, value);
        }


        public ReactiveCommand<string, IEnumerable<SearchResult>> SearchCommand { get; }

        public SearchViewModel()
        {
            SearchResults = Array.Empty<SearchResult>();

            SearchCommand = ReactiveCommand.CreateFromTask<string, IEnumerable<SearchResult>>(
                    execute: async term =>
                    {
                        IsSearching = true;
                        try
                        {
                            return await PerformSearch(term);
                        }
                        finally
                        {
                            IsSearching = false;
                        }
                    },
                    canExecute: this.WhenAnyValue(x => x.IsSearchEnabled)
                                    .Do(isEnabled => Debug.WriteLine($"CanExecute changed to: {isEnabled}"))
                );

            // Setup automatic search when term changes
            //this.WhenAnyValue(x => x.SearchTerm)
            //    .Throttle(TimeSpan.FromMilliseconds(400))
            //    .Where(term => !string.IsNullOrWhiteSpace(term))
            //    .InvokeCommand(SearchCommand);
            this.WhenAnyValue(x => x.SearchTerm)
            .Do(x => Console.WriteLine($"1. Value changed to: {x}"))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Do(x => Console.WriteLine($"2. Non-empty value: {x}"))
            .Throttle(TimeSpan.FromMilliseconds(4000))
            .Do(x => Console.WriteLine($"3. After throttle: {x}"))
            .InvokeCommand(SearchCommand);

            // Subscribe to search results
            SearchCommand.Subscribe(results => SearchResults = results);
        }

        private async Task<IEnumerable<SearchResult>> PerformSearch(string term)
        {
            // Simulate API call
            await Task.Delay(1000);
            return new[]
            {
                new SearchResult { Title = $"Result 1 for {term}", Description = "Description 1" },
                new SearchResult { Title = $"Result 2 for {term}", Description = "Description 2" },
                new SearchResult { Title = $"Result 3 for {term}", Description = "Description 3" }
            };
        }
    }

    public class SearchResult
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}