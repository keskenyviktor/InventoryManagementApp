using InventoryApp.ViewModels;
using ZXing.Net.Maui;

namespace InventoryApp.Views;

public partial class BarcodeScannerPage : ContentPage
{
    private readonly BarcodeScannerViewModel _viewModel;

    public BarcodeScannerPage(BarcodeScannerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.OnAppearingAsync();
    }

    protected override void OnDisappearing()
    {
        _viewModel.OnDisappearing();
        base.OnDisappearing();
    }

    private async void BarcodesDetected(object? sender, BarcodeDetectionEventArgs eventArgs)
    {
        var barcode = eventArgs.Results
            .Select(result => result.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        await _viewModel.HandleBarcodeDetectedAsync(barcode);
    }
}
