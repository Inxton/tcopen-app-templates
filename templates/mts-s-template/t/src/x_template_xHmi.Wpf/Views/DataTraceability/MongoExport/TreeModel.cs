using System.Collections.Generic;
using System.ComponentModel;

namespace x_template_xHmi.Wpf.Data.MongoExport

{
    public class TreeModel : INotifyPropertyChanged
    {
        public string OutputLocation { get; set; } = string.Empty;
        public List<HeaderItem> Items { get; set; } = new List<HeaderItem>();

        public List<HeaderItem> InspectorsItems { get; set; } = new List<HeaderItem>();

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
