namespace InventoryApp.Services;

public sealed class ShellAppNavigation : IAppNavigation
{
    public Task GoBackAsync() => Shell.Current.GoToAsync("..");

    public Task GoToAsync(string route) => Shell.Current.GoToAsync(route);

    public Task GoBackAsync(IDictionary<string, object> parameters) =>
        Shell.Current.GoToAsync("..", parameters);
}
