using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryApp.Data;
using InventoryApp.Models;
using InventoryApp.Views;

namespace InventoryApp.ViewModels;

public partial class ProductListViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    public ObservableCollection<Product> Products { get; } = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isRefreshing;

    private List<Product> _allProducts = new();

    public ProductListViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        if (IsRefreshing) return;

        try
        {
            IsRefreshing = true;
            _allProducts = await _databaseService.GetProductsAsync();
            FilterProducts();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterProducts();
    }

    private void FilterProducts()
    {
        Products.Clear();

        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allProducts
            : _allProducts.Where(p =>
                p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                p.Barcode.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var product in filtered)
        {
            Products.Add(product);
        }
    }

    [RelayCommand]
    private async Task GoToAddProductAsync()
    {
        await Shell.Current.GoToAsync(nameof(AddProductPage));
    }

    [RelayCommand]
    private async Task GoToDetailAsync(Product product)
    {
        if (product == null) return;

        await Shell.Current.GoToAsync($"{nameof(ProductDetailPage)}?Id={product.Id}");
    }

    [RelayCommand]
    private async Task IncreaseQuantityAsync(Product product)
    {
        if (product == null) return;

        product.Quantity += 1;
        await _databaseService.UpdateProductAsync(product);
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task DecreaseOrDeleteQuantityAsync(Product product)
    {
        if (product == null) return;

        if (product.Quantity > 0)
        {
            product.Quantity -= 1;
            await _databaseService.UpdateProductAsync(product);
        }
        else
        {
            await _databaseService.DeleteProductAsync(product);
        }

        await LoadProductsAsync();
    }
}
