using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using TcoInspectors;

namespace x_template_xPlc
{
    public class OverallResultDescriptionConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var result = (eOverallResult)Enum.ToObject(typeof(eOverallResult), value);
                //(VortexCore.enumCheckResult)((int)value);

                switch (result)
                {
                    case eOverallResult.NoAction:
                        return x_template_xHmi.Wpf.Properties.strings.NoAction;
                    case eOverallResult.InProgress:
                        return x_template_xHmi.Wpf.Properties.strings.InProgress;
                    case eOverallResult.Passed:
                        return x_template_xHmi.Wpf.Properties.strings.Passed;
                    case eOverallResult.Failed:
                        return x_template_xHmi.Wpf.Properties.strings.Failed;
                    default:
                        break;
                }

            }
            catch (Exception)
            {

                // swallow
            }

            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
