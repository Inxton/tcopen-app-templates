using x_template_xHmi.Wpf.Data;
using x_template_xHmi.Wpf.Properties;
using x_template_xHmi.Wpf.Views.Operator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcOpen.Inxton.Local.Security.Wpf;
using x_template_xPlc;
using x_template_xHmi.Wpf.Views.Diagnostics;
using TcOpen.Inxton;
using System.Windows;
using x_template_xHmi.Wpf.Views.Data.ProcessTraceability;
using x_template_xHmi.Wpf.DataTraceability;
using x_template_xPlcConnector;
using System.Diagnostics;

namespace x_template_xHmi.Wpf.Views.MainView
{
    public class MainViewModel : MenuControlViewModel
    {
        public MainViewModel()
        {
            Title = strings.Technology;
            OpenCommand(this.AddCommand(typeof(OperatorView), strings.Operator));
            AddCommand(typeof(DataView), strings.Settings);
            AddCommand(typeof(DataTraceabilityView), strings.ProductionData);
            AddCommand(typeof(TechnologyView), strings.Technology);
            AddCommand(typeof(UserManagementGroupManagementView), strings.UserManagement);
            AddCommand(typeof(DiagnosticsView), strings.Diagnostics);
            OpenLoginWindowCommand = new TcOpen.Inxton.Input.RelayCommand(a => OpenLoginWindow());
            LogOutWindowCommand = new TcOpen.Inxton.Input.RelayCommand(a => TcOpen.Inxton.TcoAppDomain.Current.AuthenticationService.DeAuthenticateCurrentUser() );
            OpenLanguageCommand = new TcOpen.Inxton.Input.RelayCommand(a => OpenLanguageWindow());
            CloseApplicationCommand = new TcOpen.Inxton.Input.RelayCommand(a => CloseApplication());
            OpenKeyboardCommand = new TcOpen.Inxton.Input.RelayCommand(a => OpenKeyboard());
        }

        private void CloseApplication()
        {
            TcoAppDomain.Current.Dispatcher.Invoke(
              (Action)(() =>
              {
                  var win = new ShutdownView();
                  var viewInstance = Activator.CreateInstance((Type)win.GetType());

                  win = viewInstance as ShutdownView;
                  if (win != null)
                  {
                      win.DataContext = App.AppShutdownModel;
                      win.ShowDialog();


                  }
              })
            );
        }

        private void OpenKeyboard()
        {
            TcoAppDomain.Current.Dispatcher.Invoke(
           (Action)(() =>
           {
               Process.Start("osk.exe");
           })
           );
        }

        public TcOpen.Inxton.Input.RelayCommand CloseApplicationCommand { get; private set; }
        public TcOpen.Inxton.Input.RelayCommand OpenLoginWindowCommand { get; private set; }
        public TcOpen.Inxton.Input.RelayCommand LogOutWindowCommand { get; private set; }
        public TcOpen.Inxton.Input.RelayCommand OpenLanguageCommand { get; private set; }
        public TcOpen.Inxton.Input.RelayCommand OpenKeyboardCommand { get; private set; }
        public bool ControlButtonVisibility { get => Entry.Settings.ShowControlButton; }



        private void OpenLanguageWindow()
        {
            TcoAppDomain.Current.Dispatcher.Invoke(
           (Action)(() =>
           {


               var win = new LanguageSelectionView();
               var viewInstance = Activator.CreateInstance((Type)win.GetType());

               win = viewInstance as LanguageSelectionView;
               if (win != null)
               {
                   win.DataContext = App.LanguageSelectionModel;
                   win.ShowDialog();


               }
       })
       );
        }
        public void OpenLoginWindow()
        {
            var loginWindow = new LoginWindow();
            loginWindow.ShowDialog();
        }
        public x_template_xPlcTwinController x_template_xPlc { get { return App.x_template_xPlc; } }

       
    }

}
