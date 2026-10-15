namespace InventoryApp.Services;

public interface ICameraPermissionService
{
    Task<bool> RequestPermissionAsync();
}
