using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace FamilyExpenses.Models
{
    [DataContract]
    public class Entry : INotifyPropertyChanged
    {
        #region Id

        private Guid _id;

        [DataMember]
        public Guid Id
        {
            get { return _id; }
            set
            {
                if (value == _id) return;
                _id = value;
                OnPropertyChanged();
            }
        }

        #endregion

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

        #region Revision

        private long _revision;

        [DataMember]
        public long Revision
        {
            get { return _revision; }
            set
            {
                if (value == _revision) return;
                _revision = value;
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

        #region Owner

        private string _owner;

        [DataMember]
        public string Owner
        {
            get { return _owner; }
            set
            {
                if (value == _owner) return;
                _owner = value;
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

        public bool Modified;

        private static readonly DateTime Zero = new DateTime(1970,1,1).ToLocalTime();

        [DataMember]
        public double Timestamp
        {
            get { return (Date-Zero).TotalSeconds; }
            set { }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}