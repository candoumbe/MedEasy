using System;
using System.Globalization;
using Xamarin.Forms;

namespace MedEasy.Mobile.Core.Converters
{
    /// <summary>
    /// Negate any boolean
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                throw new ArgumentOutOfRangeException();
            }

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                throw new ArgumentOutOfRangeException();
            }

            return !(bool)value;
        }
    }
}
