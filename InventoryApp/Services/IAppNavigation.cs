namespace InventoryApp.Services;

public interface IAppNavigation
{
    Task GoBackAsync();
    Task GoToAsync(string route);
    Task GoBackAsync(IDictionary<string, object> parameters);
}
