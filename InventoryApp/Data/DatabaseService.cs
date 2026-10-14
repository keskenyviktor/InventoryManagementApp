using InventoryApp.Models;
using SQLite;

namespace InventoryApp.Data;

public class DatabaseService : IProductRepository
{
    private readonly SQLiteAsyncConnection _database;
    private readonly Task _initializationTask;

    public DatabaseService() : this(Path.Combine(FileSystem.AppDataDirectory, "inventory.db3"))
    {
    }

    public DatabaseService(string databasePath)
    {
        _database = new SQLiteAsyncConnection(databasePath);
        _initializationTask = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await _database.CreateTableAsync<Product>();

        if (await _database.Table<Product>().CountAsync() == 0)
        {
            var random = new Random();
            var productNames = new[]
            {
                "Alma", "Banán", "Körte", "Narancs", "Citrom", "Áfonya", "Mango", "Ananász", "Görögdinnye",
                "Szőlő", "Birs", "Málna", "Kivi", "Pite", "Dió", "Füge", "Őszibarack", "Papaya", "Joghurt",
                "Tej", "Kefir", "Sajt", "Kenyer", "Tészta", "Rizs", "Burgonya", "Paradicsom", "Uborka",
                "Paprika", "Sárgarépa", "Lilahagyma", "Gomba", "Tojás", "Csirke", "Pulyka", "Hal", "Szalonna",
                "Kávé", "Tea", "Cukor", "Só", "Olaj", "Bors", "Mustár", "Méz", "Mogyoró", "Mandarin"
            };

            var products = new List<Product>(5000);
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var usedBarcodes = new HashSet<string>();

            for (var i = 0; i < 5000; i++)
            {
                var baseName = productNames[random.Next(productNames.Length)];
                var candidateName = $"{baseName} {i:0000}";

                while (!usedNames.Add(candidateName))
                {
                    candidateName = $"{baseName} {random.Next(10000, 99999)}";
                }

                var barcode = (1000000000000L + i).ToString();
                while (!usedBarcodes.Add(barcode))
                {
                    barcode = (1000000000000L + i + random.Next(1, 1000)).ToString();
                }

                var price = (decimal)random.Next(100, 5000) + (decimal)random.NextDouble();

                products.Add(new Product
                {
                    Name = candidateName,
                    Barcode = barcode,
                    Price = Math.Round(price, 2),
                    Quantity = i % 10 == 0 ? 0 : random.Next(1, 101)
                });
            }

            await _database.InsertAllAsync(products);
        }
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        await _initializationTask;
        return await _database.Table<Product>()
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<int> AddProductAsync(Product product)
    {
        await _initializationTask;
        return await _database.InsertAsync(product);
    }

    public async Task<bool> BarcodeExistsAsync(string barcode)
    {
        await _initializationTask;
        return await _database.Table<Product>().FirstOrDefaultAsync(product => product.Barcode == barcode) is not null;
    }

    public async Task<int> UpdateProductAsync(Product product)
    {
        await _initializationTask;
        return await _database.UpdateAsync(product);
    }

    public async Task<int> DeleteProductAsync(Product product)
    {
        await _initializationTask;
        return await _database.DeleteAsync(product);
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        await _initializationTask;
        return await _database.Table<Product>().FirstOrDefaultAsync(p => p.Id == id);
    }
}
