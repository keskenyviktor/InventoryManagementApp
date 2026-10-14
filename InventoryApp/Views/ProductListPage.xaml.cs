using InventoryApp.ViewModels;

namespace InventoryApp.Views;

public partial class ProductListPage : ContentPage
{
    private readonly ProductListViewModel _viewModel;

    public ProductListPage(ProductListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadProductsAsync();
    }
}
