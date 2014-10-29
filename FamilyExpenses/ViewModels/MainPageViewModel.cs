using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.Phone.UI.Input;
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
            Core.Updated += UpdateHistory;
            Core.Refresh += UpdateHistory;
        }

        public void Initialize(MainPage view)
        {
            _view = view;
            view.Loaded += delegate { HardwareButtons.BackPressed += OnBackPressed; };
            view.Unloaded += delegate { HardwareButtons.BackPressed -= OnBackPressed; };

            Core.Storage.Load(() =>
            {
                UpdateHistory();
                UpdateCategory();
                Category = Categories.FirstOrDefault();
                _view.list.Focus(FocusState.Programmatic);
            });
        }

        private void OnBackPressed(object s, BackPressedEventArgs e)
        {
            if (!_categorySelected) return;
            e.Handled = true;
            _categorySelected = false;
            UpdateCategory();
        }

        private void Save()
        {
            if (Cost == null) return;
            _categorySelected = false;
            UpdateCategory();

            _view.cmdSave.IsEnabled = false;
            var category = string.Join(";", _view.list.SelectedItems.OfType<Category>().Select(c => c.Name));
            if (string.IsNullOrEmpty(category)) return;

            Core.Log.Add("Adding {0} {1}p. {2}", category, Cost, Core.PhoneId);

            var entry = new Entry
            {
                Id = Guid.NewGuid(),
                Revision = 0,
                Categories = category,
                Cost = Cost ?? 0,
                Date = DateTime.Now,
                Owner = Core.PhoneId,
                Modified = true
            };

            Core.Storage.AddEntry(entry);
            Cost = null;
            UpdateHistory();
        }

        private void Settings()
        {
            _view.Frame.Navigate(typeof (SettingsPage), this);
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
            }
        }

        private void UpdateCategory()
        {
            if (_categorySelected)
            {
                _view.cmdSave.IsEnabled = true;
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

        private bool _categorySelected;
        public void CategorySelected(Category category)
        {
            _categorySelected = true;
            UpdateCategory();
        }

        public void Reorder(Category category)
        {
            _view.list.ReorderMode = ListViewReorderMode.Enabled;
        }

        public void Rename(Category category)
        {
            _view.Frame.Navigate(typeof (CategoriesPage), category);
        }

        public void Delete(Category category)
        {
            Core.Categories.Remove(category);
            Core.Storage.Save();
        }


        #region Categories

        public ObservableCollection<Category> Categories
        {
            get { return Core.Categories; }
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
                    Core.Storage.SetRevision((DateTime.Now - Entry.Zero).TotalSeconds - TimeSpan.FromMinutes(10).TotalSeconds);
                    Core.Syncronize();
                });
            }
        }

        public ICommand SettingsCommand { get { return new RelayCommand(Settings); } }

        public ICommand CategoriesCommand { get { return new RelayCommand(() => _view.Frame.Navigate(typeof (CategoriesPage))); } }

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
