using System.Windows;
using Vortex.Presentation.Wpf;
using x_template_xPlc;

namespace x_template_xPlc
{
    public class NotificationPanelViewModel : RenderableViewModel
    {
        public NotificationPanel Component
        {
            get => _component;
            private set => SetProperty(ref _component, value, nameof(Component));
        }

        public override object Model
        {
            get => Component;
            set => Component = value as NotificationPanel;
        }

        private NotificationPanel _component = new NotificationPanel();
    }
}