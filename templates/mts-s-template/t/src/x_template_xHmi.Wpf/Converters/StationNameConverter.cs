using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using TcoInspectors;

namespace x_template_xPlc
{
    public class StationNameConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is short)
            {
                var result = (eStations)Enum.ToObject(typeof(eStations), value);
                switch (result)
                {
                    case eStations.NONE:
                        return x_template_xHmi.Wpf.Properties.strings.None;
                   
                                      
                    default: return $"ST{value}";
                }
            }
            else return x_template_xHmi.Wpf.Properties.strings.None;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
