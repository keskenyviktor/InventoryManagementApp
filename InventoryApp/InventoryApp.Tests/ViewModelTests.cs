using InventoryApp.Data;
using InventoryApp.Converters;
using InventoryApp.Models;
using InventoryApp.Services;
using InventoryApp.ViewModels;
using System.Globalization;

namespace InventoryApp.Tests;

public class ViewModelTests
{
    [Fact]
    public void AddProductViewModel_RequiresValidValuesBeforeEnablingSave()
    {
        var viewModel = new AddProductViewModel(new FakeProductRepository(), new FakeAppNavigation());

        Assert.False(viewModel.IsSaveEnabled);
        Assert.True(viewModel.ShowNameRequirement);
        Assert.True(viewModel.ShowBarcodeRequirement);
        Assert.True(viewModel.ShowPriceRequirement);
        Assert.True(viewModel.ShowQuantityRequirement);

        viewModel.Name = "Test product";
        viewModel.Barcode = "12345";
        viewModel.Price = "-1";
        viewModel.Quantity = "1.5";

        Assert.False(viewModel.IsSaveEnabled);
        Assert.True(viewModel.ShowPriceError);
        Assert.True(viewModel.ShowQuantityError);

        viewModel.Price = "12.50";
        viewModel.Quantity = "4";

        Assert.True(viewModel.IsSaveEnabled);
        Assert.False(viewModel.ShowPriceRequirement);
        Assert.False(viewModel.ShowQuantityRequirement);
    }

    [Fact]
    public async Task AddProductViewModel_DuplicateBarcodeShowsFieldErrorAndDoesNotSave()
    {
        var repository = new FakeProductRepository();
        repository.Products.Add(new Product { Name = "Existing", Barcode = "12345" });
        var viewModel = new AddProductViewModel(repository, new FakeAppNavigation())
        {
            Name = "New product",
            Barcode = " 12345 ",
            Price = "12",
            Quantity = "2"
        };

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Contains("már", viewModel.BarcodeError, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(repository.AddedProducts);
        Assert.True(viewModel.ShowBarcodeError);
    }

    [Fact]
    public async Task AddProductViewModel_ReportsMissingAndMalformedFieldsWhenSaving()
    {
        var viewModel = new AddProductViewModel(new FakeProductRepository(), new FakeAppNavigation())
        {
            Name = "Product",
            Barcode = "12345",
            Price = "invalid",
            Quantity = "-1"
        };

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Contains("érvényes", viewModel.PriceError, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("nem negatív", viewModel.QuantityError, StringComparison.OrdinalIgnoreCase);
        Assert.False(viewModel.IsSaveEnabled);
    }

    [Fact]
    public async Task AddProductViewModel_SavesTrimmedProductAndNavigatesBack()
    {
        var repository = new FakeProductRepository();
        var navigation = new FakeAppNavigation();
        var viewModel = new AddProductViewModel(repository, navigation)
        {
            Name = "  New product  ",
            Barcode = " 54321 ",
            Price = "12.50",
            Quantity = "3"
        };

        await viewModel.SaveCommand.ExecuteAsync(null);

        var saved = Assert.Single(repository.AddedProducts);
        Assert.Equal("New product", saved.Name);
        Assert.Equal("54321", saved.Barcode);
        Assert.Equal(12.50m, saved.Price);
        Assert.Equal(3m, saved.Quantity);
        Assert.Equal(1, navigation.GoBackCalls);
    }

    [Fact]
    public async Task ProductListViewModel_FiltersAndPaginatesProducts()
    {
        var repository = new FakeProductRepository();
        repository.Products.AddRange(Enumerable.Range(1, 26).Select(index =>
            new Product { Id = index, Name = $"Product {index:00}", Barcode = $"100{index:00}", Quantity = 1 }));
        repository.Products.Add(new Product { Id = 27, Name = "Special item", Barcode = "UNIQUE", Quantity = 1 });
        var viewModel = new ProductListViewModel(repository, new FakeAppNavigation());

        viewModel.PageSize = 10;
        await viewModel.LoadProductsCommand.ExecuteAsync(null);

        Assert.Equal(27, viewModel.TotalItems);
        Assert.Equal(3, viewModel.TotalPages);
        Assert.Equal(10, viewModel.Products.Count);
        Assert.True(viewModel.HasNextPage);

        viewModel.NextPageCommand.Execute(null);
        Assert.Equal(2, viewModel.CurrentPage);
        Assert.Equal(10, viewModel.Products.Count);

        viewModel.SearchText = "unique";
        Assert.Equal(1, viewModel.CurrentPage);
        Assert.Single(viewModel.Products);
        Assert.Equal("Special item", viewModel.Products[0].Name);

        viewModel.SearchText = "not present";
        Assert.Empty(viewModel.Products);
        Assert.Contains("találat", viewModel.EmptyMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProductListViewModel_IncreasesDecreasesAndDeletesZeroStock()
    {
        var repository = new FakeProductRepository();
        var stocked = new Product { Id = 1, Name = "Stocked", Barcode = "A", Quantity = 2 };
        var zeroStock = new Product { Id = 2, Name = "Empty", Barcode = "B", Quantity = 0 };
        repository.Products.AddRange([stocked, zeroStock]);
        var viewModel = new ProductListViewModel(repository, new FakeAppNavigation());
        await viewModel.LoadProductsCommand.ExecuteAsync(null);

        await viewModel.IncreaseQuantityCommand.ExecuteAsync(stocked);
        Assert.Equal(3m, stocked.Quantity);

        await viewModel.DecreaseOrDeleteQuantityCommand.ExecuteAsync(stocked);
        Assert.Equal(2m, stocked.Quantity);

        await viewModel.DecreaseOrDeleteQuantityCommand.ExecuteAsync(zeroStock);
        Assert.DoesNotContain(repository.Products, product => product.Id == zeroStock.Id);
    }

    [Fact]
    public void QuantityConverters_ShowZeroStockWarningAndCorrectQuantityActions()
    {
        var colorConverter = new QuantityToColorConverter();
        var decreaseConverter = new QuantityToDecreaseVisibleConverter();
        var deleteConverter = new QuantityToDeleteVisibleConverter();
        var culture = CultureInfo.InvariantCulture;

        Assert.Equal(Colors.Red, Assert.IsType<Color>(colorConverter.Convert(0m, typeof(Color), null, culture)));
        Assert.Equal(Colors.BlueViolet, Assert.IsType<Color>(colorConverter.Convert(2, typeof(Color), null, culture)));
        Assert.False(Assert.IsType<bool>(decreaseConverter.Convert(0m, typeof(bool), null, culture)));
        Assert.True(Assert.IsType<bool>(deleteConverter.Convert(0m, typeof(bool), null, culture)));
        Assert.True(Assert.IsType<bool>(decreaseConverter.Convert(1, typeof(bool), null, culture)));
        Assert.False(Assert.IsType<bool>(deleteConverter.Convert(1, typeof(bool), null, culture)));
    }
}

internal sealed class FakeProductRepository : IProductRepository
{
    public List<Product> Products { get; } = [];
    public List<Product> AddedProducts { get; } = [];

    public Task<List<Product>> GetProductsAsync() => Task.FromResult(Products.ToList());

    public Task<int> AddProductAsync(Product product)
    {
        AddedProducts.Add(product);
        product.Id = Products.Count == 0 ? 1 : Products.Max(item => item.Id) + 1;
        Products.Add(product);
        return Task.FromResult(1);
    }

    public Task<int> UpdateProductAsync(Product product) => Task.FromResult(1);

    public Task<int> DeleteProductAsync(Product product)
    {
        Products.RemoveAll(item => item.Id == product.Id);
        return Task.FromResult(1);
    }

    public Task<Product?> GetProductByIdAsync(int id) =>
        Task.FromResult(Products.FirstOrDefault(product => product.Id == id));

    public Task<bool> BarcodeExistsAsync(string barcode) =>
        Task.FromResult(Products.Any(product => product.Barcode == barcode));
}

internal sealed class FakeAppNavigation : IAppNavigation
{
    public int GoBackCalls { get; private set; }

    public Task GoBackAsync()
    {
        GoBackCalls++;
        return Task.CompletedTask;
    }

    public Task GoToAsync(string route) => Task.CompletedTask;
}
