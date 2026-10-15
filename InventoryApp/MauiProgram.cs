using InventoryApp.Data;
using InventoryApp.Services;
using InventoryApp.ViewModels;
using InventoryApp.Views;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;

namespace InventoryApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseBarcodeReader()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<IProductRepository, DatabaseService>();
            builder.Services.AddSingleton<IAppNavigation, ShellAppNavigation>();

            builder.Services.AddTransient<ProductListViewModel>();
            builder.Services.AddTransient<AddProductViewModel>();
            builder.Services.AddTransient<ProductListPage>();
            builder.Services.AddTransient<AddProductPage>();
            builder.Services.AddTransient<ProductDetailPage>();
            builder.Services.AddTransient<BarcodeScannerPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
