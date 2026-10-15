using InventoryApp.Data;
using InventoryApp.Models;

namespace InventoryApp.Tests;

public class DatabaseServiceTests
{
    [Fact]
    public async Task EmptyDatabaseSeedsUniqueProductsAndSupportsCrud()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"inventory-tests-{Guid.NewGuid():N}.db3");
        var database = new DatabaseService(databasePath);

        var seededProducts = await database.GetProductsAsync();
        Assert.Equal(5000, seededProducts.Count);
        Assert.Equal(5000, seededProducts.Select(product => product.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(5000, seededProducts.Select(product => product.Barcode).Distinct().Count());
        Assert.Contains(seededProducts, product => product.Quantity == 0);

        var product = new Product
        {
            Name = "Integration test product",
            Barcode = $"TEST-{Guid.NewGuid():N}",
            Price = 19.95m,
            Quantity = 7
        };

        await database.AddProductAsync(product);
        Assert.True(product.Id > 0);
        Assert.True(await database.BarcodeExistsAsync(product.Barcode));

        product.Quantity = 9;
        await database.UpdateProductAsync(product);
        Assert.Equal(9, (await database.GetProductByIdAsync(product.Id))?.Quantity);

        await database.DeleteProductAsync(product);
        Assert.Null(await database.GetProductByIdAsync(product.Id));
        Assert.False(await database.BarcodeExistsAsync(product.Barcode));
    }
}
