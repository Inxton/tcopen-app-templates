using System.ComponentModel;

namespace x_template_xHmi.Wpf.Data.MongoExport

{
    public class FieldItem : INotifyPropertyChanged
    {
        private bool _included;
        public string Field { get; set; } = string.Empty;
        public string AttributeName { get; set; } = string.Empty;
        public bool Included
        {
            get => _included;
            set { _included = value; NotifyPropertyChanged(nameof(Included)); }
        }

        public override string ToString() { return Field; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
