using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace TestReactiveUI2.ViewModels
{
    public class SearchViewModel : ReactiveObject
    {
        private string _searchTerm;
        private bool _isSearching;
        private IEnumerable<SearchResult> _searchResults;

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

        public ReactiveCommand<string, IEnumerable<SearchResult>> SearchCommand { get; }

        public SearchViewModel()
        {
            SearchResults = Array.Empty<SearchResult>();

            SearchCommand = ReactiveCommand.CreateFromTask<string, IEnumerable<SearchResult>>(
                async term =>
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
                });

            // Setup automatic search when term changes
            this.WhenAnyValue(x => x.SearchTerm)
                .Throttle(TimeSpan.FromMilliseconds(400))
                .Where(term => !string.IsNullOrWhiteSpace(term))
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