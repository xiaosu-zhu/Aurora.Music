using Aurora.Shared.Helpers;
using System;
using Windows.UI.Xaml.Data;

namespace Aurora.Music.Controls
{
    class SliderToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double d)
            {
                return $"{d.ToString("0.0", CultureInfoHelper.CurrentCulture)}%";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
