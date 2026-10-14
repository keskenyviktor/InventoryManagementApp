using System.Globalization;
using InventoryApp.Data;
using InventoryApp.Models;

namespace InventoryApp.Views;

[QueryProperty(nameof(ProductId), "Id")]
public partial class ProductDetailPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private Product? _product;

    public ProductDetailPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
    }

    public int ProductId { get; set; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (ProductId <= 0)
        {
            return;
        }

        _product = await _databaseService.GetProductByIdAsync(ProductId);

        if (_product == null)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        NameEntry.Text = _product.Name;
        BarcodeEntry.Text = _product.Barcode;
        PriceEntry.Text = _product.Price.ToString(CultureInfo.InvariantCulture);
        QuantityEntry.Text = _product.Quantity.ToString(CultureInfo.InvariantCulture);
    }

    private async void SaveButton_Clicked(object sender, EventArgs e)
    {
        if (_product == null)
        {
            return;
        }

        var name = NameEntry.Text?.Trim();
        var barcode = BarcodeEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(barcode))
        {
            await DisplayAlert("Hiányzó adat", "A termék neve és vonalkódja is kötelező.", "OK");
            return;
        }

        if (!decimal.TryParse(PriceEntry.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) ||
            !decimal.TryParse(QuantityEntry.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var quantity))
        {
            await DisplayAlert("Érvénytelen érték", "Az ár és a mennyiség szám formátumú kell legyen.", "OK");
            return;
        }

        _product.Name = name;
        _product.Barcode = barcode;
        _product.Price = price;
        _product.Quantity = quantity;

        await _databaseService.UpdateProductAsync(_product);
        await Shell.Current.GoToAsync("..");
    }
}
