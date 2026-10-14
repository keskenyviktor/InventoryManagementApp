using System.Globalization;
using InventoryApp.Data;
using InventoryApp.Models;

namespace InventoryApp.Views;

public partial class AddProductPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    public AddProductPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
    }

    private async void SaveButton_Clicked(object sender, EventArgs e)
    {
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

        var product = new Product
        {
            Name = name,
            Barcode = barcode,
            Price = price,
            Quantity = quantity
        };

        await _databaseService.AddProductAsync(product);
        await Shell.Current.GoToAsync("..");
    }
}
