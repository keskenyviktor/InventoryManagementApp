using InventoryApp.Models;

namespace InventoryApp.Data;

public interface IProductRepository
{
    Task<List<Product>> GetProductsAsync();
    Task<int> AddProductAsync(Product product);
    Task<int> UpdateProductAsync(Product product);
    Task<int> DeleteProductAsync(Product product);
    Task<Product?> GetProductByIdAsync(int id);
    Task<bool> BarcodeExistsAsync(string barcode);
}
