using MaterialDesignThemes.Wpf;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using TcoCore.Wpf;


namespace x_template_xHmi.Wpf.SynologyStatus
{
    public class SynologyStatusViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly Synology synology;
        private readonly DispatcherTimer timer;

        private string status;
        private PackIconKind statusIcon;
        private Brush backgroundColor;

        public SynologyStatusViewModel(string ip, string username, string password, int updateIntervalMinutes)
        {
            _ip = ip;
            IsVisible =  string.IsNullOrEmpty(_ip);
            synology = new Synology(ip, username, password);

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(updateIntervalMinutes)
            };
            timer.Tick += Timer_Tick;
        }

        #region Properties

        public string Status
        {
            get => status;
            set
            {
                if (status != value)
                {
                    status = value;
                    UpdateForegroundColorAndIcon();
                    OnPropertyChanged(nameof(Status));
                }
            }
        }
        
        public Brush ForegroundColor
        {
            get => backgroundColor;
            set
            {
                if (backgroundColor != value)
                {
                    backgroundColor = value;
                    OnPropertyChanged(nameof(ForegroundColor));
                }
            }
        }

        private string _ip;

        public bool IsVisible
        {
            get => isVisible;
            set
            {
              
                    isVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
               
            }
        }
        public PackIconKind StatusIcon
        {
            get => statusIcon;
            set
            {
                if (statusIcon != value)
                {
                    statusIcon = value;
                    OnPropertyChanged(nameof(StatusIcon));
                }
            }
        }

        #endregion

        #region Methods

        public async void StartUpdatingStatus()
        {
            await UpdateStatus();
            timer.Start();
        }

        public async Task UpdateStatus()
        {
            IsVisible = string.IsNullOrEmpty(_ip);
      
            string newStatus = await synology.ReadHealthStatus();
            Status = newStatus;
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            await UpdateStatus();
        }

        private void UpdateForegroundColorAndIcon()
        {
            switch (status?.ToLowerInvariant())
            {
                case "healthy":
                    ForegroundColor = Brushes.Green;
                    StatusIcon = PackIconKind.CheckCircleOutline;
                    break;

                case "warning":
                    ForegroundColor = TcoColors.Warning;
                    StatusIcon = PackIconKind.Alert;
                    break;

                case "critical":
                    ForegroundColor = TcoColors.Error;
                    StatusIcon = PackIconKind.AlertCircleOutline;
                    break;

                case "unknown":
                default:
                    ForegroundColor = Brushes.Gray;
                    StatusIcon = PackIconKind.HelpCircleOutline;
                    break;
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion

        #region IDisposable

        private bool disposed;
        private bool isVisible;

        public void Dispose()
        {
            if (disposed) return;

            timer.Stop();
            timer.Tick -= Timer_Tick;

            // If Synology itself holds unmanaged resources, dispose it too.
            (synology as IDisposable)?.Dispose();

            disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
