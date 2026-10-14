using SQLite;

namespace InventoryApp.Models;

[Table("Products")]
public class Product
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Barcode { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public decimal Quantity { get; set; }
}
