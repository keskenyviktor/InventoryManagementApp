namespace InventoryApp.Services;

public sealed class MauiCameraPermissionService : ICameraPermissionService
{
    public async Task<bool> RequestPermissionAsync() =>
        await Permissions.RequestAsync<Permissions.Camera>() == PermissionStatus.Granted;
}
