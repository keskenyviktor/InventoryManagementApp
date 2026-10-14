using InventoryApp.Views;

namespace InventoryApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(AddProductPage), typeof(AddProductPage));
            Routing.RegisterRoute(nameof(ProductDetailPage), typeof(ProductDetailPage));
        }
    }
}
