using System.Windows.Controls;
using x_template_xPlcConnector;

namespace x_template_xHmi.Wpf.SynologyStatus
{
    public partial class SynologyStatusView : UserControl
    {
        private SynologyStatusViewModel viewModel;

        public SynologyStatusView()
        {
            InitializeComponent();

            viewModel = new SynologyStatusViewModel();
            DataContext = viewModel;

            viewModel.StartUpdatingStatus();
        }
    }
}