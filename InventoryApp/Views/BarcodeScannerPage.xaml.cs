using InventoryApp.Services;
using ZXing.Net.Maui;

namespace InventoryApp.Views;

public partial class BarcodeScannerPage : ContentPage
{
    private int _scanHandled;
    private bool _isActive;
    private readonly IAppNavigation _navigation;

    public BarcodeScannerPage(IAppNavigation navigation)
    {
        InitializeComponent();
        _navigation = navigation;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _isActive = true;
        Interlocked.Exchange(ref _scanHandled, 0);

        try
        {
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert(
                    "Kameraengedély szükséges",
                    "A vonalkód beolvasásához engedélyezze a kamera használatát.",
                    "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            if (_isActive)
            {
                CameraView.IsDetecting = true;
            }
        }
        catch (Exception exception)
        {
            CameraView.IsDetecting = false;
            await DisplayAlert("Kamera hiba", exception.Message, "OK");
            await _navigation.GoBackAsync();
        }
    }

    protected override void OnDisappearing()
    {
        _isActive = false;
        CameraView.IsDetecting = false;
        base.OnDisappearing();
    }

    private void BarcodesDetected(object? sender, BarcodeDetectionEventArgs eventArgs)
    {
        var barcode = eventArgs.Results
            .Select(result => result.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        if (barcode is null || Interlocked.Exchange(ref _scanHandled, 1) != 0)
        {
            return;
        }

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            CameraView.IsDetecting = false;
            await _navigation.GoBackAsync(
                new Dictionary<string, object>
                {
                    ["ScannedBarcode"] = barcode
                });
        });
    }
}
