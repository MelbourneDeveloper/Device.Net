using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace UnoCrossPlatform
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            value is bool boolValue ? boolValue ? Visibility.Visible : Visibility.Collapsed : value;

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
