using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryApp.Data;
using InventoryApp.Models;
using InventoryApp.Services;
using InventoryApp.Views;

namespace InventoryApp.ViewModels;

public partial class ProductDetailViewModel : ObservableObject
{
    private readonly IProductRepository _productRepository;
    private readonly IAppNavigation _navigation;
    private Product? _product;
    private bool _isLoading;

    [ObservableProperty]
    private int _productId;

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
    private bool _isBusy;

    public bool IsSaveEnabled =>
        !IsBusy &&
        _product is not null &&
        string.IsNullOrWhiteSpace(NameError) &&
        string.IsNullOrWhiteSpace(BarcodeError) &&
        string.IsNullOrWhiteSpace(PriceError) &&
        string.IsNullOrWhiteSpace(QuantityError) &&
        !string.IsNullOrWhiteSpace(Name) &&
        !string.IsNullOrWhiteSpace(Barcode) &&
        TryParseNonNegativeDecimal(Price, out _) &&
        TryParseNonNegativeDecimal(Quantity, out _);

    public ProductDetailViewModel(IProductRepository productRepository, IAppNavigation navigation)
    {
        _productRepository = productRepository;
        _navigation = navigation;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Id", out var id) &&
            int.TryParse(Convert.ToString(id, CultureInfo.InvariantCulture), out var parsedId))
        {
            ProductId = parsedId;
        }

        if (query.TryGetValue("ScannedBarcode", out var value) && value is string barcode)
        {
            Barcode = barcode;
        }
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (_isLoading || ProductId <= 0 || _product is not null)
        {
            return;
        }

        _isLoading = true;
        IsBusy = true;
        try
        {
            _product = await _productRepository.GetProductByIdAsync(ProductId);
            if (_product is null)
            {
                await _navigation.GoBackAsync();
                return;
            }

            Name = _product.Name;
            Barcode = _product.Barcode;
            Price = _product.Price.ToString(CultureInfo.InvariantCulture);
            Quantity = _product.Quantity.ToString(CultureInfo.InvariantCulture);
            ValidateAll();
        }
        finally
        {
            _isLoading = false;
            IsBusy = false;
            NotifySaveState();
        }
    }

    partial void OnNameChanged(string value)
    {
        NameError = string.IsNullOrWhiteSpace(value) ? "A termék neve kötelező." : string.Empty;
        NotifySaveState();
    }

    partial void OnBarcodeChanged(string value)
    {
        BarcodeError = string.IsNullOrWhiteSpace(value) ? "A vonalkód kötelező." : string.Empty;
        NotifySaveState();
    }

    partial void OnPriceChanged(string value)
    {
        PriceError = GetNumberError(value, "Az ár", requireInteger: false);
        NotifySaveState();
    }

    partial void OnQuantityChanged(string value)
    {
        QuantityError = GetNumberError(value, "A készlet", requireInteger: true);
        NotifySaveState();
    }

    partial void OnNameErrorChanged(string value) => NotifySaveState();
    partial void OnBarcodeErrorChanged(string value) => NotifySaveState();
    partial void OnPriceErrorChanged(string value) => NotifySaveState();
    partial void OnQuantityErrorChanged(string value) => NotifySaveState();
    partial void OnIsBusyChanged(bool value) => NotifySaveState();
    partial void OnProductIdChanged(int value) => NotifySaveState();

    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidateAll();
        if (!IsSaveEnabled || _product is null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var normalizedBarcode = Barcode.Trim();
            if (!string.Equals(normalizedBarcode, _product.Barcode, StringComparison.Ordinal) &&
                await _productRepository.BarcodeExistsAsync(normalizedBarcode))
            {
                BarcodeError = "Ez a vonalkód már egy másik termékhez tartozik.";
                return;
            }

            TryParseNonNegativeDecimal(Price, out var parsedPrice);
            TryParseNonNegativeDecimal(Quantity, out var parsedQuantity);
            _product.Name = Name.Trim();
            _product.Barcode = normalizedBarcode;
            _product.Price = parsedPrice;
            _product.Quantity = parsedQuantity;

            await _productRepository.UpdateProductAsync(_product);
            await _navigation.GoBackAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task ScanBarcodeAsync() => _navigation.GoToAsync(nameof(BarcodeScannerPage));

    private void ValidateAll()
    {
        NameError = string.IsNullOrWhiteSpace(Name) ? "A termék neve kötelező." : string.Empty;
        BarcodeError = string.IsNullOrWhiteSpace(Barcode) ? "A vonalkód kötelező." : string.Empty;
        PriceError = GetNumberError(Price, "Az ár", requireInteger: false);
        QuantityError = GetNumberError(Quantity, "A készlet", requireInteger: true);
        NotifySaveState();
    }

    private static string GetNumberError(string value, string fieldName, bool requireInteger)
    {
        if (TryParseNonNegativeDecimal(value, out var parsed) &&
            (!requireInteger || parsed == decimal.Truncate(parsed)))
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return $"{fieldName} megadása kötelező.";
        }

        return requireInteger
            ? "Adjon meg nem negatív egész készletmennyiséget."
            : "Adjon meg érvényes, nem negatív árat.";
    }

    private static bool TryParseNonNegativeDecimal(string value, out decimal parsed)
    {
        var parsedSuccessfully =
            decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out parsed) ||
            decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed);
        return parsedSuccessfully && parsed >= 0;
    }

    private void NotifySaveState() => OnPropertyChanged(nameof(IsSaveEnabled));
}
