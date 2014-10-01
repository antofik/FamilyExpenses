using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }

    public class Expense
    {
        public string Name { get; set; }
        public double Cost { get; set; }
    }
}
