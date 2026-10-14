using System.Globalization;

namespace InventoryApp.Converters;

public class QuantityToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal quantity)
        {
            return quantity == 0 ? Colors.Red : Colors.Black;
        }

        if (value is int intQuantity)
        {
            return intQuantity == 0 ? Colors.Red : Colors.Black;
        }

        return Colors.Black;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
