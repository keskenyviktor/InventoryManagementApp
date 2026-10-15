namespace InventoryApp.Services;

public interface IUiDispatcher
{
    Task InvokeAsync(Func<Task> action);
}
