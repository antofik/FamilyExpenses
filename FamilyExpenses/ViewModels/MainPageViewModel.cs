using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Windows.Input;
using Windows.Phone.UI.Input;
using Windows.System.Profile;
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
            const string names = "Еда;Дорога;Дети;Подарки;Одежда;Лечение;Личное;Квартира;Другое";
            foreach(var name in names.Split(new[]{';'}))
                Categories.Add(new Category(name));
            Core.Updated += UpdateHistory;
        }

        public void Initialize(MainPage view)
        {
            _view = view;
            Core.Storage.Load(() =>
            {
                UpdateHistory();
                UpdateCategory();
                
                HardwareButtons.BackPressed += (s, e) =>
                {
                    if (Category != null)
                    {
                        e.Handled = true;
                        Category = null;
                    }
                };
            });
        }

        private async void Save()
        {

            if (Cost == null) return;
            var category = string.Join(";", _view.list.SelectedItems.OfType<Category>().Select(c => c.Name));
            if (string.IsNullOrEmpty(category)) return;

            var entry = new Entry
            {
                Id = Guid.NewGuid(),
                Revision = 0,
                Categories = string.Join(";", _view.list.SelectedItems.OfType<Category>().Select(c => c.Name)),
                Cost = Cost ?? 0,
                Date = DateTime.Now,
                Owner = Core.PhoneId,
                Modified = true
            };

            Core.Storage.AddEntry(entry);
            Cost = null;
            _view.txtCost.Focus(FocusState.Programmatic);
            _view.list.SelectedItem = null;
            UpdateHistory();
        }

        private void Settings()
        {
            _view.Frame.Navigate(typeof (SettingsPage), this);
        }

        private void Sync()
        {
            Core.Syncronize();
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

        #region Category

        private Category _category;

        public Category Category
        {
            get { return _category; }
            set
            {
                if (_category == value) return;
                _category = value;
                OnPropertyChanged();

                UpdateCategory();
            }
        }

        private void UpdateCategory()
        {
            if (Category != null)
            {
                _view.panelCost.Visibility = Visibility.Visible;
                _view.txtCost.Focus(FocusState.Programmatic);
                _view.list.Visibility = Visibility.Collapsed;
            }
            else
            {
                _view.panelCost.Visibility = Visibility.Collapsed;
                _view.list.Visibility = Visibility.Visible;
            }
        }

        #endregion

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

        public ICommand SyncCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Core.Storage.Revision = (DateTime.Now - Entry.Zero).TotalSeconds - TimeSpan.FromMinutes(10).TotalSeconds;
                    Sync();
                });
            }
        }

        public ICommand SettingsCommand { get { return new RelayCommand(Settings); } }

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
