using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using FamilyExpenses.CoreModules;
using FamilyExpenses.Models;
using FamilyExpenses.ViewModels;

namespace FamilyExpenses
{
    public class MyListView : ListView
    {
        private static MenuFlyout menu;

        public MyListView()
        {
            Loaded += delegate
            {
                menu = new MenuFlyout();
                var item = new MenuFlyoutItem {Text = "Упорядочить"};
                item.Click += (s, e) =>
                {
                    var element = (FrameworkElement) s;
                    var category = element.DataContext as Category;
                    var viewModel = (MainPageViewModel) DataContext;
                    if (viewModel != null)
                        viewModel.Reorder(_category);
                };
                menu.Items.Add(item);
                item = new MenuFlyoutItem {Text = "Переименовать"};
                item.Click += (s, e) =>
                {
                    var element = (FrameworkElement) s;
                    var category = element.DataContext as Category;
                    var viewModel = (MainPageViewModel) DataContext;
                    if (viewModel != null)
                        viewModel.Rename(_category);
                };
                menu.Items.Add(item);
                item = new MenuFlyoutItem {Text = "Удалить"};
                item.Click += (s, e) =>
                {
                    var element = (FrameworkElement) s;
                    var category = element.DataContext as Category;
                    var viewModel = (MainPageViewModel) DataContext;
                    if (viewModel != null)
                        viewModel.Delete(_category);
                };
                menu.Items.Add(item);
            };
        }

        private Category _category;

        protected override DependencyObject GetContainerForItemOverride()
        {
            var i = base.GetContainerForItemOverride() as ListViewItem;

            if (i != null)
            {
                i.IsHoldingEnabled = true;
                i.IsTapEnabled = false;
                i.Holding += (s, e) =>
                {
                    _category = ((ListViewItem) s).Content as Category;
                    menu.ShowAt((FrameworkElement) s);
                };
                i.Tapped += (s, e) =>
                {
                    if (ReorderMode == ListViewReorderMode.Enabled)
                    {
                        ReorderMode = ListViewReorderMode.Disabled;
                        Core.Storage.Save();
                    }
                    else
                    {
                        _category = ((ListViewItem) s).Content as Category;
                        var viewModel = (MainPageViewModel) DataContext;
                        viewModel.CategorySelected(_category);
                    }
                };
            }
            return i;
        }
    }
}