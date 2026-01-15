using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace LibraryManagementSystem.Converters;

/// <summary>
/// Converts boolean to Visibility
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Dacă este true, inversează vizibilitatea (true -> Collapsed, false -> Visible)
    /// </summary>
    public bool InvertVisibility { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            bool invert = InvertVisibility || parameter?.ToString()?.ToLower() == "invert";
            return (boolValue ^ invert) ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts null/empty string to Visibility (null/empty = Collapsed, not null = Visible)
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool invert = parameter?.ToString()?.ToLower() == "invert";
        bool isNullOrEmpty = value == null || (value is string str && string.IsNullOrWhiteSpace(str));
        return (isNullOrEmpty ^ invert) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts status to color (error = red, success = green)
/// </summary>
public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isError)
        {
            return isError ? Brushes.Red : Brushes.Green;
        }
        return Brushes.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts overdue status to color
/// </summary>
public class OverdueToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isOverdue)
        {
            return isOverdue ? Brushes.Red : Brushes.Black;
        }
        return Brushes.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts availability to color
/// </summary>
public class AvailabilityToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isAvailable)
        {
            return isAvailable ? Brushes.Green : Brushes.Orange;
        }
        return Brushes.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts decimal to currency string in Lei (RON)
/// </summary>
public class CurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal amount)
        {
            // Use Romanian culture for Lei currency display
            var romanianCulture = new CultureInfo("ro-RO");
            return amount.ToString("N2", romanianCulture) + " Lei";
        }
        return "0 Lei";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            // Remove "Lei" suffix and parse
            str = str.Replace("Lei", "").Trim();
            var romanianCulture = new CultureInfo("ro-RO");
            if (decimal.TryParse(str, NumberStyles.Any, romanianCulture, out decimal result))
            {
                return result;
            }
        }
        return 0m;
    }
}

/// <summary>
/// Converts date to formatted string
/// </summary>
public class DateFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            string format = parameter?.ToString() ?? "dd/MM/yyyy";
            return date.ToString(format);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
