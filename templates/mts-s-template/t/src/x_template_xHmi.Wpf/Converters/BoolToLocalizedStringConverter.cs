using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace x_template_xPlc
{
    class BoolToLocalizedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return x_template_xHmi.Wpf.Properties.strings.Invalid;

            if (value is bool)
            {
                if ((bool)value) return x_template_xHmi.Wpf.Properties.strings.True;
                else return x_template_xHmi.Wpf.Properties.strings.False;
            }
            else if (value is string)
            {
                bool b;
                if (bool.TryParse((string)value, out b))
                {
                    if (b) return x_template_xHmi.Wpf.Properties.strings.True;
                    else return x_template_xHmi.Wpf.Properties.strings.False;
                }
                else 
                {
                    return x_template_xHmi.Wpf.Properties.strings.Invalid;
                }
            }
            else return x_template_xHmi.Wpf.Properties.strings.Invalid;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
