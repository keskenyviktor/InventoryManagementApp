using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryApp.Data;
using InventoryApp.Models;
using InventoryApp.Services;
using InventoryApp.Views;

namespace InventoryApp.ViewModels;

public partial class ProductListViewModel : ObservableObject
{
    private readonly IProductRepository _productRepository;
    private readonly IAppNavigation _navigation;
    private List<Product> _allProducts = new();

    public ObservableCollection<Product> Products { get; } = new();

    public List<int> PageSizes { get; } = new() { 10, 25, 50, 100 };

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private int _pageSize = 25;

    [ObservableProperty]
    private int _currentPage = 1;

    public int TotalPages { get; private set; }
    public int TotalItems => _allProducts.Count;
    public string EmptyMessage => string.IsNullOrWhiteSpace(SearchText)
        ? "Nincsenek termékek. Adjon hozzá új elemeket a + gombbal."
        : "Nincs találat a keresésre.";
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public ProductListViewModel(IProductRepository productRepository, IAppNavigation navigation)
    {
        _productRepository = productRepository;
        _navigation = navigation;
    }

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        if (IsRefreshing) return;

        try
        {
            IsRefreshing = true;
            _allProducts = await _productRepository.GetProductsAsync();
            ApplyPagination();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1;
        ApplyPagination();
        OnPropertyChanged(nameof(EmptyMessage));
    }

    partial void OnPageSizeChanged(int value)
    {
        if (value <= 0) return;

        CurrentPage = 1;
        ApplyPagination();
    }

    private void ApplyPagination()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allProducts
            : _allProducts.Where(p =>
                p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                p.Barcode.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        var items = filtered.ToList();
        TotalPages = items.Count == 0 ? 1 : (int)Math.Ceiling(items.Count / (double)PageSize);
        CurrentPage = Math.Clamp(CurrentPage, 1, TotalPages);

        var skip = (CurrentPage - 1) * PageSize;
        var pageItems = items.Skip(skip).Take(PageSize).ToList();

        Products.Clear();
        foreach (var product in pageItems)
        {
            Products.Add(product);
        }

        OnPropertyChanged(nameof(TotalItems));
        OnPropertyChanged(nameof(EmptyMessage));
        OnPropertyChanged(nameof(HasPreviousPage));
        OnPropertyChanged(nameof(HasNextPage));
        OnPropertyChanged(nameof(TotalPages));
    }

    [RelayCommand]
    private async Task GoToAddProductAsync()
    {
        await _navigation.GoToAsync(nameof(AddProductPage));
    }

    [RelayCommand]
    private async Task GoToDetailAsync(Product product)
    {
        if (product == null) return;

        await _navigation.GoToAsync($"{nameof(ProductDetailPage)}?Id={product.Id}");
    }

    [RelayCommand]
    private async Task IncreaseQuantityAsync(Product product)
    {
        if (product == null) return;

        product.Quantity += 1;
        await _productRepository.UpdateProductAsync(product);
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task DecreaseOrDeleteQuantityAsync(Product product)
    {
        if (product == null) return;

        if (product.Quantity > 0)
        {
            product.Quantity -= 1;
            await _productRepository.UpdateProductAsync(product);
        }
        else
        {
            await _productRepository.DeleteProductAsync(product);
        }

        await LoadProductsAsync();
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (!HasPreviousPage) return;
        CurrentPage--;
        ApplyPagination();
    }

    [RelayCommand]
    private void NextPage()
    {
        if (!HasNextPage) return;
        CurrentPage++;
        ApplyPagination();
    }
}
