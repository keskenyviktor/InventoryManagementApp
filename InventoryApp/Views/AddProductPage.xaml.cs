using InventoryApp.ViewModels;

namespace InventoryApp.Views;

public partial class AddProductPage : ContentPage, IQueryAttributable
{
    private readonly AddProductViewModel _viewModel;

    public AddProductPage(AddProductViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _viewModel.ApplyQueryAttributes(query);
}
