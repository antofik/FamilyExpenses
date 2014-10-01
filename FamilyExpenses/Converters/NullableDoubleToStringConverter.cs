using System;
using Windows.UI.Xaml.Data;

namespace FamilyExpenses.Converters
{
    public class NullableDoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return "" + value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            double d;
            return double.TryParse((string) value ?? "", out d) ? d : 0;
        }
    }
}
