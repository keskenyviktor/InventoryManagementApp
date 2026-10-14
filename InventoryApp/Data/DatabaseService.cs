using InventoryApp.Models;
using SQLite;

namespace InventoryApp.Data;

public class DatabaseService
{
    private readonly SQLiteAsyncConnection _database;

    public DatabaseService()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "inventory.db3");
        _database = new SQLiteAsyncConnection(dbPath);
        InitializeAsync().FireAndForget();
    }

    private async Task InitializeAsync()
    {
        await _database.CreateTableAsync<Product>();

        if (await _database.Table<Product>().CountAsync() == 0)
        {
            var sampleProducts = new[]
            {
                new Product { Name = "Alma", Barcode = "1111111111111", Price = 249m, Quantity = 12m },
                new Product { Name = "Körte", Barcode = "2222222222222", Price = 329m, Quantity = 0m },
                new Product { Name = "Banán", Barcode = "3333333333333", Price = 199m, Quantity = 8m },
                new Product { Name = "Tej", Barcode = "4444444444444", Price = 420m, Quantity = 0m }
            };

            await _database.InsertAllAsync(sampleProducts);
        }
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        await _database.CreateTableAsync<Product>();
        return await _database.Table<Product>()
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<int> AddProductAsync(Product product)
    {
        return await _database.InsertAsync(product);
    }

    public async Task<int> UpdateProductAsync(Product product)
    {
        return await _database.UpdateAsync(product);
    }

    public async Task<int> DeleteProductAsync(Product product)
    {
        return await _database.DeleteAsync(product);
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _database.Table<Product>().FirstOrDefaultAsync(p => p.Id == id);
    }
}

internal static class TaskExtensions
{
    public static void FireAndForget(this Task task)
    {
        _ = task.ContinueWith(
            static t => Console.WriteLine(t.Exception),
            TaskContinuationOptions.OnlyOnFaulted);
    }
}
