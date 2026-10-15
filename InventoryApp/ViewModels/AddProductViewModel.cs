using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryApp.Data;
using InventoryApp.Models;
using InventoryApp.Services;
using InventoryApp.Views;

namespace InventoryApp.ViewModels;

public partial class AddProductViewModel : ObservableObject
{
    private readonly IProductRepository _databaseService;
    private readonly IAppNavigation _navigation;
    private bool _nameTouched;
    private bool _barcodeTouched;
    private bool _priceTouched;
    private bool _quantityTouched;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _barcode = string.Empty;

    [ObservableProperty]
    private string _price = string.Empty;

    [ObservableProperty]
    private string _quantity = string.Empty;

    [ObservableProperty]
    private string _nameError = string.Empty;

    [ObservableProperty]
    private string _barcodeError = string.Empty;

    [ObservableProperty]
    private string _priceError = string.Empty;

    [ObservableProperty]
    private string _quantityError = string.Empty;

    [ObservableProperty]
    private bool _isSaving;

    public bool IsSaveEnabled =>
        !_isSaving &&
        string.IsNullOrEmpty(_nameError) &&
        string.IsNullOrEmpty(_barcodeError) &&
        string.IsNullOrEmpty(_priceError) &&
        string.IsNullOrEmpty(_quantityError) &&
        !string.IsNullOrWhiteSpace(_name) &&
        !string.IsNullOrWhiteSpace(_barcode) &&
        TryParsePrice(_price, out _) &&
        TryParseQuantity(_quantity, out _);

    public bool ShowNameRequirement => string.IsNullOrEmpty(NameError) && string.IsNullOrWhiteSpace(Name);
    public bool ShowBarcodeRequirement => string.IsNullOrEmpty(BarcodeError) && string.IsNullOrWhiteSpace(Barcode);
    public bool ShowPriceRequirement => string.IsNullOrEmpty(PriceError) &&
        (!TryParsePrice(Price, out var price) || price < 0);
    public bool ShowQuantityRequirement => string.IsNullOrEmpty(QuantityError) &&
        (!TryParseQuantity(Quantity, out var quantity) || quantity < 0);
    public bool ShowNameError => !string.IsNullOrEmpty(NameError);
    public bool ShowBarcodeError => !string.IsNullOrEmpty(BarcodeError);
    public bool ShowPriceError => !string.IsNullOrEmpty(PriceError);
    public bool ShowQuantityError => !string.IsNullOrEmpty(QuantityError);

    public AddProductViewModel(IProductRepository databaseService, IAppNavigation navigation)
    {
        _databaseService = databaseService;
        _navigation = navigation;
    }

    partial void OnNameChanged(string value)
    {
        _nameTouched = true;
        NameError = _nameTouched && string.IsNullOrWhiteSpace(value)
            ? "A termék neve kötelező."
            : string.Empty;
        NotifyValidationState();
    }

    partial void OnBarcodeChanged(string value)
    {
        _barcodeTouched = true;
        BarcodeError = _barcodeTouched && string.IsNullOrWhiteSpace(value)
            ? "A vonalkód kötelező."
            : string.Empty;
        NotifyValidationState();
    }

    partial void OnPriceChanged(string value)
    {
        _priceTouched = true;
        PriceError = GetPriceError(value, _priceTouched);
        NotifyValidationState();
    }

    partial void OnQuantityChanged(string value)
    {
        _quantityTouched = true;
        QuantityError = GetQuantityError(value, _quantityTouched);
        NotifyValidationState();
    }

    partial void OnNameErrorChanged(string value) => NotifyValidationState();
    partial void OnBarcodeErrorChanged(string value) => NotifyValidationState();
    partial void OnPriceErrorChanged(string value) => NotifyValidationState();
    partial void OnQuantityErrorChanged(string value) => NotifyValidationState();
    partial void OnIsSavingChanged(bool value) => NotifyValidationState();

    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidateAll();
        if (!IsSaveEnabled)
        {
            return;
        }

        IsSaving = true;
        try
        {
            var normalizedBarcode = _barcode.Trim();
            if (await _databaseService.BarcodeExistsAsync(normalizedBarcode))
            {
                BarcodeError = "Ez a vonalkód már egy másik termékhez tartozik.";
                return;
            }

            TryParsePrice(_price, out var parsedPrice);
            TryParseQuantity(_quantity, out var parsedQuantity);
            await _databaseService.AddProductAsync(new Product
            {
                Name = _name.Trim(),
                Barcode = normalizedBarcode,
                Price = parsedPrice,
                Quantity = parsedQuantity
            });

            await _navigation.GoBackAsync();
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private Task ScanBarcodeAsync() => _navigation.GoToAsync(nameof(BarcodeScannerPage));

    private void ValidateAll()
    {
        _nameTouched = _barcodeTouched = _priceTouched = _quantityTouched = true;
        NameError = string.IsNullOrWhiteSpace(Name) ? "A termék neve kötelező." : string.Empty;
        BarcodeError = string.IsNullOrWhiteSpace(Barcode) ? "A vonalkód kötelező." : string.Empty;
        PriceError = GetPriceError(Price, true);
        QuantityError = GetQuantityError(Quantity, true);
        NotifyValidationState();
    }

    private static string GetPriceError(string value, bool showError)
    {
        if (TryParsePrice(value, out var price) && price >= 0)
        {
            return string.Empty;
        }

        return showError
            ? string.IsNullOrWhiteSpace(value) ? "Az ár megadása kötelező." : "Adjon meg érvényes, nem negatív árat."
            : string.Empty;
    }

    private static string GetQuantityError(string value, bool showError)
    {
        if (TryParseQuantity(value, out var quantity) && quantity >= 0)
        {
            return string.Empty;
        }

        return showError
            ? string.IsNullOrWhiteSpace(value) ? "A készlet megadása kötelező." : "Adjon meg nem negatív egész készletet."
            : string.Empty;
    }

    private static bool TryParsePrice(string value, out decimal price) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out price) ||
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out price);

    private static bool TryParseQuantity(string value, out decimal quantity) =>
        TryParsePrice(value, out quantity) && quantity == decimal.Truncate(quantity);

    private void NotifyValidationState()
    {
        OnPropertyChanged(nameof(IsSaveEnabled));
        OnPropertyChanged(nameof(ShowNameRequirement));
        OnPropertyChanged(nameof(ShowBarcodeRequirement));
        OnPropertyChanged(nameof(ShowPriceRequirement));
        OnPropertyChanged(nameof(ShowQuantityRequirement));
        OnPropertyChanged(nameof(ShowNameError));
        OnPropertyChanged(nameof(ShowBarcodeError));
        OnPropertyChanged(nameof(ShowPriceError));
        OnPropertyChanged(nameof(ShowQuantityError));
    }
}
