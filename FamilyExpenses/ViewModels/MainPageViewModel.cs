using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FamilyExpenses.CoreModules;
using FamilyExpenses.Models;

namespace FamilyExpenses.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public MainPageViewModel()
        {
#if DEBUG
            const string names = "Еда;Дорога;Дети;Подарки;Одежда;Лечение;Личное;Квартира;Другое";
            foreach(var name in names.Split(new[]{';'}))
                Categories.Add(new Category(name));
#endif
        }

        public void Initialize(MainPage view)
        {
            _view = view;
            Core.Storage.Load(UpdateHistory);
        }

        private void Save()
        {
            if (Cost == null) return;
            var category = string.Join(";", _view.list.SelectedItems.OfType<Category>().Select(c => c.Name));
            if (string.IsNullOrEmpty(category)) return;

            var entry = new Entry
            {
                Categories = string.Join(";", _view.list.SelectedItems.OfType<Category>().Select(c => c.Name)),
                Cost = Cost ?? 0,
                Date = DateTime.Now
            };

            Core.Storage.AddEntry(entry);
            Cost = null;
            _view.txtCost.Focus(FocusState.Programmatic);
            _view.list.SelectedItem = null;
            UpdateHistory();
        }

        private void UpdateHistory()
        {
            var items = Core.Entries
                .GroupBy(c => new {c.Date.Month, c.Date.Year, c.Categories})
                .Select(c => new {c.Key.Month, c.Key.Year, c.Key.Categories, Cost = c.Sum(x => x.Cost)})
                .GroupBy(c => new {c.Year, c.Month})
                .Select(c=>new History
                    {
                        Date = new DateTime(c.Key.Year, c.Key.Month, 1), 
                        Expenses = c.Select(x=>new Expense{Cost = x.Cost, Name = x.Categories}).OrderByDescending(x=>x.Cost).ToList()
                    })
                .OrderByDescending(c=>c.Date)
                .ToList();
            ExpensesHistory.Clear();
            foreach (var item in items)
            {
                item.Total = item.Expenses.Sum(c => c.Cost);
                ExpensesHistory.Add(item);
            }
        }

        #region Cost

        private double? _cost;

        public double? Cost
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

        public ObservableCollection<Entry> Entries
        {
            get { return Core.Entries; }
        }

        #region History

        private ObservableCollection<History> _expensesHistory = new ObservableCollection<History>();

        public ObservableCollection<History> ExpensesHistory
        {
            get { return _expensesHistory; }
            set
            {
                if (value == _expensesHistory) return;
                _expensesHistory = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Categories

        private ObservableCollection<Category> _categories = new ObservableCollection<Category>();

        public ObservableCollection<Category> Categories
        {
            get { return _categories; }
            set
            {
                if (_categories == value) return;
                _categories = value;
                OnPropertyChanged();
            }
        }

        #endregion

        private MainPage _view;

        public ICommand SaveCommand { get { return new RelayCommand(Save); } }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

}
