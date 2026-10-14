using InventoryApp.Data;
using InventoryApp.ViewModels;
using InventoryApp.Views;
using Microsoft.Extensions.Logging;

namespace InventoryApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<DatabaseService>();

            builder.Services.AddTransient<ProductListViewModel>();
            builder.Services.AddTransient<AddProductViewModel>();
            builder.Services.AddTransient<ProductListPage>();
            builder.Services.AddTransient<AddProductPage>();
            builder.Services.AddTransient<ProductDetailPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
