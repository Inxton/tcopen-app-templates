using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using x_template_xPlcConnector;

namespace x_template_xHmi.Wpf.SynologyStatus
{
    /// <summary>
    /// Interaction logic for SynologyNotificationView.xaml
    /// </summary>
    public partial class SynologyNotificationView : UserControl
    {
        private SynologyStatusViewModel viewModel;

        public SynologyNotificationView()
        {
            InitializeComponent();
            viewModel = new SynologyStatusViewModel();
            DataContext = viewModel;
        }



        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(SynologyNotificationView));


    }
}
