using System.Collections.Generic;
using System.ComponentModel;

namespace x_template_xHmi.Wpf.Data.MongoExport

{
    public class HeaderItem : INotifyPropertyChanged
    {
        private bool? _required = false; ///3-way checkmark
        private bool _midState;

        public HeaderItem()
        {
            Subscribe();
        }


        public void Subscribe()
        {
            Items.ForEach(x => x.PropertyChanged -= FieldItem_PropertyChanged);///ensure there are no duplicates
            Items.ForEach(x => x.PropertyChanged += FieldItem_PropertyChanged);
        }
        public string Name { get; set; } = string.Empty; ///Won't change once constructed
        public bool MidState
        {
            get => _midState;
            set { _midState = value; OnPropertyChanged(nameof(MidState)); }
        }
        public bool? Required
        {
            get => _required;
            set
            {
                if (value != _required)
                {
                    _required = value;
                    OnPropertyChanged(nameof(Required));
                }

            }
        }

        public string Fields { get; set; } = string.Empty;
        public List<FieldItem> Items { get; set; } = new List<FieldItem>();
        public void Add(FieldItem item)
        {
            if (item.Field == string.Empty) return;
            if (Items.Find(x => x.Field.Equals(item.Field)) is null)
            {
                Items.Add(item);
                item.PropertyChanged += FieldItem_PropertyChanged;
            }
        }

        public override string ToString()
        {
            string value = string.Empty;

            foreach (var item in Items)
            {
                if (item.Included) value += item.Field + '\n';
            }
            if (value != string.Empty || Required == true) value = Fields + value;
            return value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {

            if (PropertyChanged != null)
            {
                if (propertyName == nameof(Required))
                {
                    if (Required == true) { Items.ForEach(x => x.Included = true); }
                    if (Required == false) { Items.ForEach(x => x.Included = false); }
                    if (Required != null) MidState = false;
                }
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void FieldItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is FieldItem && e.PropertyName == nameof(FieldItem.Included))
            {
                var c = Items.FindAll(x => x.Included).Count;

                ///non-identical checkmarks
                if (c != Items.Count && c != 0) this.Required = null;
                ///identical checkmarks
                else { Required = c == Items.Count; }
            }
        }
    }
}
