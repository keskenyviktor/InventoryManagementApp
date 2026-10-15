namespace InventoryApp.Services;

public sealed class MauiUiDispatcher : IUiDispatcher
{
    public Task InvokeAsync(Func<Task> action)
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await action();
                completion.SetResult();
            }
            catch (Exception exception)
            {
                completion.SetException(exception);
            }
        });

        return completion.Task;
    }
}
