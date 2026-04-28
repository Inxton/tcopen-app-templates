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
using x_template_xHmi.Wpf.Properties;

namespace x_template_xHmi.Wpf.Views.MainView
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void ControlButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            if (sender.Equals(Start))
            {
                App.x_template_xPlc.MAIN._technology._components.VirtualButtons.StartButton.Synchron = true;
            }


            if (sender.Equals(Stop))
            {
                App.x_template_xPlc.MAIN._technology._components.VirtualButtons.StopButton.Synchron = true;
            }



            System.Threading.Thread.Sleep(2000);

            App.x_template_xPlc.MAIN._technology._components.VirtualButtons.StopButton.Synchron = false;
            App.x_template_xPlc.MAIN._technology._components.VirtualButtons.StartButton.Synchron = false;
        }



    }
}
