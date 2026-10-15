using InventoryApp.Data;
using InventoryApp.Converters;
using InventoryApp.Models;
using InventoryApp.Services;
using InventoryApp.ViewModels;
using InventoryApp.Views;
using System.Globalization;
using Moq;

namespace InventoryApp.Tests;

public class ViewModelTests
{
    [Fact]
    public void AddProductViewModel_RequiresValidValuesBeforeEnablingSave()
    {
        var viewModel = new AddProductViewModel(
            Mock.Of<IProductRepository>(),
            Mock.Of<IAppNavigation>());

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
        var repository = new Mock<IProductRepository>();
        var navigation = new Mock<IAppNavigation>();
        repository.Setup(database => database.BarcodeExistsAsync("12345")).ReturnsAsync(true);
        var viewModel = new AddProductViewModel(repository.Object, navigation.Object)
        {
            Name = "New product",
            Barcode = " 12345 ",
            Price = "12",
            Quantity = "2"
        };

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Contains("már", viewModel.BarcodeError, StringComparison.OrdinalIgnoreCase);
        repository.Verify(database => database.AddProductAsync(It.IsAny<Product>()), Times.Never);
        navigation.Verify(service => service.GoBackAsync(), Times.Never);
        Assert.True(viewModel.ShowBarcodeError);
    }

    [Fact]
    public async Task AddProductViewModel_ReportsMissingAndMalformedFieldsWhenSaving()
    {
        var repository = new Mock<IProductRepository>();
        var viewModel = new AddProductViewModel(repository.Object, Mock.Of<IAppNavigation>())
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
        repository.Verify(database => database.AddProductAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task AddProductViewModel_SavesTrimmedProductAndNavigatesBack()
    {
        var repository = new Mock<IProductRepository>();
        var navigation = new Mock<IAppNavigation>();
        Product? savedProduct = null;
        repository.Setup(database => database.BarcodeExistsAsync("54321")).ReturnsAsync(false);
        repository.Setup(database => database.AddProductAsync(It.IsAny<Product>()))
            .Callback<Product>(product => savedProduct = product)
            .Returns(Task.CompletedTask);
        navigation.Setup(service => service.GoBackAsync()).Returns(Task.CompletedTask);
        var viewModel = new AddProductViewModel(repository.Object, navigation.Object)
        {
            Name = "  New product  ",
            Barcode = " 54321 ",
            Price = "12.50",
            Quantity = "3"
        };

        await viewModel.SaveCommand.ExecuteAsync(null);

        var saved = Assert.IsType<Product>(savedProduct);
        Assert.Equal("New product", saved.Name);
        Assert.Equal("54321", saved.Barcode);
        Assert.Equal(12.50m, saved.Price);
        Assert.Equal(3m, saved.Quantity);
        repository.Verify(database => database.AddProductAsync(It.IsAny<Product>()), Times.Once);
        navigation.Verify(service => service.GoBackAsync(), Times.Once);
    }

    [Fact]
    public async Task AddProductViewModel_ScanCommandOpensScanner()
    {
        var navigation = new Mock<IAppNavigation>();
        navigation.Setup(service => service.GoToAsync(nameof(BarcodeScannerPage)))
            .Returns(Task.CompletedTask);
        var viewModel = new AddProductViewModel(
            Mock.Of<IProductRepository>(),
            navigation.Object);

        await viewModel.ScanBarcodeCommand.ExecuteAsync(null);

        navigation.Verify(service => service.GoToAsync(nameof(BarcodeScannerPage)), Times.Once);
    }

    [Fact]
    public void AddProductViewModel_AppliesScannedBarcode()
    {
        var viewModel = new AddProductViewModel(
            Mock.Of<IProductRepository>(),
            Mock.Of<IAppNavigation>());

        viewModel.ApplyQueryAttributes(new Dictionary<string, object>
        {
            ["ScannedBarcode"] = "987654321"
        });

        Assert.Equal("987654321", viewModel.Barcode);
    }

    [Fact]
    public async Task ProductDetailViewModel_LoadsAndSavesChangesThroughRepository()
    {
        var product = new Product
        {
            Id = 7,
            Name = "Original",
            Barcode = "OLD-CODE",
            Price = 4.50m,
            Quantity = 2
        };
        var repository = new Mock<IProductRepository>();
        var navigation = new Mock<IAppNavigation>();
        repository.Setup(database => database.GetProductByIdAsync(7)).ReturnsAsync(product);
        repository.Setup(database => database.BarcodeExistsAsync("NEW-CODE")).ReturnsAsync(false);
        repository.Setup(database => database.UpdateProductAsync(product)).Returns(Task.CompletedTask);
        navigation.Setup(service => service.GoBackAsync()).Returns(Task.CompletedTask);
        var viewModel = new ProductDetailViewModel(repository.Object, navigation.Object);
        viewModel.ApplyQueryAttributes(new Dictionary<string, object> { ["Id"] = "7" });

        await viewModel.LoadCommand.ExecuteAsync(null);
        Assert.Equal("Original", viewModel.Name);
        Assert.Equal("OLD-CODE", viewModel.Barcode);
        Assert.True(viewModel.IsSaveEnabled);

        viewModel.Name = "Updated";
        viewModel.Barcode = "NEW-CODE";
        viewModel.Price = "10.25";
        viewModel.Quantity = "5";
        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Updated", product.Name);
        Assert.Equal("NEW-CODE", product.Barcode);
        Assert.Equal(10.25m, product.Price);
        Assert.Equal(5m, product.Quantity);
        repository.Verify(database => database.UpdateProductAsync(product), Times.Once);
        navigation.Verify(service => service.GoBackAsync(), Times.Once);
    }

    [Fact]
    public void ProductDetailViewModel_AppliesProductAndScannedBarcodeQueryValues()
    {
        var viewModel = new ProductDetailViewModel(
            Mock.Of<IProductRepository>(),
            Mock.Of<IAppNavigation>());

        viewModel.ApplyQueryAttributes(new Dictionary<string, object>
        {
            ["Id"] = "27",
            ["ScannedBarcode"] = "SCANNED-CODE"
        });

        Assert.Equal(27, viewModel.ProductId);
        Assert.Equal("SCANNED-CODE", viewModel.Barcode);
    }

    [Fact]
    public async Task BarcodeScannerViewModel_RequestsPermissionStartsAndStopsDetection()
    {
        var permission = new Mock<ICameraPermissionService>();
        var navigation = new Mock<IAppNavigation>();
        permission.Setup(service => service.RequestPermissionAsync()).ReturnsAsync(true);
        var viewModel = new BarcodeScannerViewModel(
            permission.Object,
            Mock.Of<IUiDispatcher>(),
            navigation.Object);

        await viewModel.OnAppearingAsync();
        Assert.True(viewModel.IsDetecting);

        viewModel.OnDisappearing();
        Assert.False(viewModel.IsDetecting);
        permission.Verify(service => service.RequestPermissionAsync(), Times.Once);
    }

    [Fact]
    public async Task BarcodeScannerViewModel_ReturnsOnlyOneScannedBarcodeOnUiDispatcher()
    {
        var permission = new Mock<ICameraPermissionService>();
        permission.Setup(service => service.RequestPermissionAsync()).ReturnsAsync(true);
        var dispatcher = new Mock<IUiDispatcher>();
        var navigation = new Mock<IAppNavigation>();
        navigation.Setup(service => service.GoBackAsync(It.IsAny<IDictionary<string, object>>()))
            .Returns(Task.CompletedTask);
        dispatcher.Setup(service => service.InvokeAsync(It.IsAny<Func<Task>>()))
            .Returns((Func<Task> action) => action());
        var viewModel = new BarcodeScannerViewModel(permission.Object, dispatcher.Object, navigation.Object);
        await viewModel.OnAppearingAsync();

        await viewModel.HandleBarcodeDetectedAsync(" 123456789 ");
        await viewModel.HandleBarcodeDetectedAsync("987654321");

        dispatcher.Verify(service => service.InvokeAsync(It.IsAny<Func<Task>>()), Times.Once);
        navigation.Verify(
            service => service.GoBackAsync(It.Is<IDictionary<string, object>>(parameters =>
                parameters["ScannedBarcode"].Equals("123456789"))),
            Times.Once);
        Assert.False(viewModel.IsDetecting);
    }

    [Fact]
    public async Task ProductListViewModel_FiltersAndPaginatesProducts()
    {
        var repository = new Mock<IProductRepository>();
        var products = Enumerable.Range(1, 26).Select(index =>
            new Product { Id = index, Name = $"Product {index:00}", Barcode = $"100{index:00}", Quantity = 1 });
        var allProducts = products.Append(new Product { Id = 27, Name = "Special item", Barcode = "UNIQUE", Quantity = 1 }).ToList();
        repository.Setup(database => database.GetProductsAsync()).ReturnsAsync(() => allProducts.ToList());
        var viewModel = new ProductListViewModel(repository.Object, Mock.Of<IAppNavigation>());

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
        var repository = new Mock<IProductRepository>();
        var stocked = new Product { Id = 1, Name = "Stocked", Barcode = "A", Quantity = 2 };
        var zeroStock = new Product { Id = 2, Name = "Empty", Barcode = "B", Quantity = 0 };
        var products = new List<Product> { stocked, zeroStock };
        repository.Setup(database => database.GetProductsAsync()).ReturnsAsync(() => products.ToList());
        repository.Setup(database => database.UpdateProductAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
        repository.Setup(database => database.DeleteProductAsync(zeroStock))
            .Callback(() => products.Remove(zeroStock))
            .Returns(Task.CompletedTask);
        var viewModel = new ProductListViewModel(repository.Object, Mock.Of<IAppNavigation>());
        await viewModel.LoadProductsCommand.ExecuteAsync(null);

        await viewModel.IncreaseQuantityCommand.ExecuteAsync(stocked);
        Assert.Equal(3m, stocked.Quantity);
        repository.Verify(database => database.UpdateProductAsync(stocked), Times.Once);

        await viewModel.DecreaseOrDeleteQuantityCommand.ExecuteAsync(stocked);
        Assert.Equal(2m, stocked.Quantity);
        repository.Verify(database => database.UpdateProductAsync(stocked), Times.Exactly(2));

        await viewModel.DecreaseOrDeleteQuantityCommand.ExecuteAsync(zeroStock);
        repository.Verify(database => database.DeleteProductAsync(zeroStock), Times.Once);
        Assert.DoesNotContain(viewModel.Products, product => product.Id == zeroStock.Id);
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
