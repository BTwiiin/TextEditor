using System.Globalization;
using System.Windows.Data;

namespace TextEditor.UI.Converters
{
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return 0.0;

            if (value is double size && double.TryParse(parameter.ToString(), out double percentage))
            {
                return size * percentage;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
