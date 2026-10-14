using System.Globalization;

namespace InventoryApp.Converters;

public class QuantityToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal quantity)
        {
            return quantity == 0 ? Colors.Red : Colors.BlueViolet;
        }

        if (value is int intQuantity)
        {
            return intQuantity == 0 ? Colors.Red : Colors.BlueViolet;
        }

        return Colors.Black;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class QuantityToDecreaseVisibleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal quantity)
        {
            return quantity > 0;
        }

        if (value is int intQuantity)
        {
            return intQuantity > 0;
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class QuantityToDeleteVisibleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal quantity)
        {
            return quantity <= 0;
        }

        if (value is int intQuantity)
        {
            return intQuantity <= 0;
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
