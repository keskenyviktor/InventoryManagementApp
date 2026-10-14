namespace InventoryApp.Services;

public sealed class ShellAppNavigation : IAppNavigation
{
    public Task GoBackAsync() => Shell.Current.GoToAsync("..");

    public Task GoToAsync(string route) => Shell.Current.GoToAsync(route);
}
