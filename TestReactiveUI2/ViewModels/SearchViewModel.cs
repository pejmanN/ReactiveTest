using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
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
        private bool _addPostfixToSearch; // this is for checking if enable so we will add postfix at the end of search
        private IDisposable _cd { get; set; }


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

        public bool AddPostfixToSearch
        {
            get => _addPostfixToSearch;
            set => this.RaiseAndSetIfChanged(ref _addPostfixToSearch, value);
        }


        private ReactiveCommand<string, IEnumerable<SearchResult>> _searchCommand;
        public ReactiveCommand<string, IEnumerable<SearchResult>> SearchCommand
        {
            get => _searchCommand;
            private set => this.RaiseAndSetIfChanged(ref _searchCommand, value);
        }

        public SearchViewModel()
        {
            SearchResults = Array.Empty<SearchResult>();

            this.WhenAnyValue(x => x.AddPostfixToSearch)
                .Select(SearchCommandDispatcher)
               .Do(command =>
               {
                   _cd?.Dispose();
                   SearchCommand = command;
               })
                 .Subscribe(x => { _cd = SearchCommand?.Subscribe(results => SearchResults = results); });


            this.WhenAnyValue(x => x.SearchTerm)
            .Do(x => Console.WriteLine($"1. Value changed to: {x}"))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Do(x => Console.WriteLine($"2. Non-empty value: {x}"))
            //.Throttle(TimeSpan.FromMilliseconds(400))
            .Do(x => Console.WriteLine($"3. After throttle: {x}"))
           .InvokeCommand(this, x => x.SearchCommand);
            //.InvokeCommand(SearchCommand);

            // Subscribe to search results
            _cd = SearchCommand?.Subscribe(results => SearchResults = results);
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

        private ReactiveCommand<string, IEnumerable<SearchResult>> SearchCommandDispatcher(bool addPostfixToSearch)
        {
            if (addPostfixToSearch)
            {
                return ReactiveCommand.CreateFromTask<string, IEnumerable<SearchResult>>(
                    execute: async term =>
                    {
                        IsSearching = true;
                        try
                        {
                            //var data = await PerformSearch(term);
                            //SearchResults = data.Select(x => new SearchResult { Title = x.Title + " , Postfix", Description = x.Description });
                            //return SearchResults;
                            var data = await PerformSearch(term);
                            return data.Select(x => new SearchResult { Title = x.Title + " , Postfix", Description = x.Description });
                        }
                        finally
                        {
                            IsSearching = false;
                        }
                    },
                    canExecute: this.WhenAnyValue(x => x.IsSearchEnabled)
                                    .Do(isEnabled => Debug.WriteLine($"CanExecute changed to: {isEnabled}"))
                );
            }
            else
            {

                return ReactiveCommand.CreateFromTask<string, IEnumerable<SearchResult>>(
                        execute: async term =>
                        {
                            IsSearching = true;
                            try
                            {
                                //SearchResults = await PerformSearch(term);
                                //return SearchResults;
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
            }

        }
    }

    public class SearchResult
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}