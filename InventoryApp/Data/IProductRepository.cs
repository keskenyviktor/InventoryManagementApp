using InventoryApp.Models;

namespace InventoryApp.Data;

public interface IProductRepository
{
    Task<List<Product>> GetProductsAsync();
    Task AddProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(Product product);
    Task<Product?> GetProductByIdAsync(int id);
    Task<bool> BarcodeExistsAsync(string barcode);
}
