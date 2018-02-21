// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
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
                return (d / 100d).ToString("P1", CultureInfoHelper.CurrentCulture);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    class EqualizerGainToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double d)
            {
                if (d >= 0)
                {
                    return $"{d.ToString("+0.0", CultureInfoHelper.CurrentCulture)}dB";
                }
                return $"{d.ToString("0.0", CultureInfoHelper.CurrentCulture)}dB";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
