using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace x_template_xPlc
{
    public class IconConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                switch (value)
                {
                    case "ControlVoltage": return PackIconKind.Power;
                    case "AirPressure": return PackIconKind.OxygenTank;
                    case "EmergencyStop": return PackIconKind.Alert;
                    case "SafetyDoor": return PackIconKind.ShieldCheck;
                    case "DoorClosed": return PackIconKind.Door;
                    case "DoorLocked": return PackIconKind.DoorClosedLock;
                    case "ProcessDataLoaded": return PackIconKind.CogOutline;
                    case "TechnologyDataLoaded": return PackIconKind.ClipboardCheckOutline;
                    case "OpticBarrier": return PackIconKind.Hand;
                    case "Robot": return PackIconKind.RobotIndustrial;
                    case "PlcConnection": return PackIconKind.HeartPulse;          
                    case "AutomatAllowed": return PackIconKind.RunFast;
                    case "None": return PackIconKind.BorderNoneVariant;   
                }
                if (Enum.TryParse(value.ToString(), out PackIconKind kind)) return kind;
            }
            return PackIconKind.HelpCircle;
            
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

    }
}
