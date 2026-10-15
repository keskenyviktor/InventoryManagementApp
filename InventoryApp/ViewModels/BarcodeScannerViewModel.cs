using CommunityToolkit.Mvvm.ComponentModel;
using InventoryApp.Services;

namespace InventoryApp.ViewModels;

public partial class BarcodeScannerViewModel : ObservableObject
{
    private readonly ICameraPermissionService _cameraPermission;
    private readonly IUiDispatcher _uiDispatcher;
    private readonly IAppNavigation _navigation;
    private int _scanHandled;
    private bool _isPageActive;

    [ObservableProperty]
    private bool _isDetecting;

    [ObservableProperty]
    private string _statusMessage = "Tartsa a vonalkódot a kamera elé.";

    public BarcodeScannerViewModel(
        ICameraPermissionService cameraPermission,
        IUiDispatcher uiDispatcher,
        IAppNavigation navigation)
    {
        _cameraPermission = cameraPermission;
        _uiDispatcher = uiDispatcher;
        _navigation = navigation;
    }

    public async Task OnAppearingAsync()
    {
        _isPageActive = true;
        Interlocked.Exchange(ref _scanHandled, 0);
        IsDetecting = false;

        try
        {
            if (!await _cameraPermission.RequestPermissionAsync())
            {
                StatusMessage = "A vonalkódolvasáshoz kameraengedély szükséges.";
                await _navigation.GoBackAsync();
                return;
            }

            if (_isPageActive)
            {
                StatusMessage = "Tartsa a vonalkódot a kamera elé.";
                IsDetecting = true;
            }
        }
        catch (Exception exception)
        {
            IsDetecting = false;
            StatusMessage = $"Nem sikerült elindítani a kamerát: {exception.Message}";
        }
    }

    public void OnDisappearing()
    {
        _isPageActive = false;
        IsDetecting = false;
    }

    public async Task HandleBarcodeDetectedAsync(string? barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode) ||
            !IsDetecting ||
            Interlocked.Exchange(ref _scanHandled, 1) != 0)
        {
            return;
        }

        await _uiDispatcher.InvokeAsync(async () =>
        {
            IsDetecting = false;
            await _navigation.GoBackAsync(
                new Dictionary<string, object>
                {
                    ["ScannedBarcode"] = barcode.Trim()
                });
        });
    }
}
