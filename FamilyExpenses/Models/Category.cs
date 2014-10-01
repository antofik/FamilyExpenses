using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace FamilyExpenses.Models
{
    [DataContract]
    public class Category : INotifyPropertyChanged
    {
        public Category(string name)
        {
            Name = name;
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int UsageCount { get; set; }


        #region Selected

        private bool _selected;

        [IgnoreDataMember]
        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (value == _selected) return;
                _selected = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
