using FluentValidation;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ReactiveAndFodyTst.Client;
using ReactiveAndFodyTst.Client.Models;
using ReactiveAndFodyTst.Client.Services;
using ReactiveAndFodyTst.Client.Validators;
using ReactiveAndFodyTst.Client.ViewModels;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddScoped<IValidator<Product>, ProductValidator>();
builder.Services.AddScoped<AdminProductViewModel>();
builder.Services.AddScoped<ProductsViewModel>();
builder.Services.AddScoped<ProductEditViewModel>();
builder.Services.AddScoped<ProductSearchViewModel>();


builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
