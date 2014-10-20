using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FamilyExpenses.Common;

namespace FamilyExpenses.Models
{
    public class History
    {
        public DateTime Date { get; set; }

        public string Month
        {
            get { return Date.ToString("MMMM", new CultureInfo("ru-RU")); }
        }

        public string Year
        {
            get { return Date.ToString("yyyy"); }
        }

        public List<Expense> Expenses { get; set; }

        public double Total { get; set; }

        public ICommand Goto
        {
            get 
            { 
                return new RelayCommand(() => ((Frame) Window.Current.Content).Navigate(typeof (MonthReport), this));
            }
        }
    }

    public class Expense
    {
        public string Name { get; set; }
        public double Cost { get; set; }

        public ICommand Goto
        {
            get
            {
                return new RelayCommand(() => ((Frame)Window.Current.Content).Navigate(typeof(CategoryReport), Name));
            }
        }
    }
}
