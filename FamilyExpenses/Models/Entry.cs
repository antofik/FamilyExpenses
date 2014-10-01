using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace FamilyExpenses.Models
{
    [DataContract]
    public class Entry : INotifyPropertyChanged
    {
        #region Cost

        private double _cost;

        [DataMember]
        public double Cost
        {
            get { return _cost; }
            set
            {
                if (value == _cost) return;
                _cost = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Categories

        private string _categories;

        [DataMember]
        public string Categories
        {
            get { return _categories; }
            set
            {
                if (value == _categories) return;
                _categories = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Date

        private DateTime _date;

        [DataMember]
        public DateTime Date
        {
            get { return _date; }
            set
            {
                if (value == _date) return;
                _date = value;
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