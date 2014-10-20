using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace FamilyExpenses.CoreModules
{
    public class BusyImpl
    {
        private BusyModes _mode = BusyModes.None;
        public StatusBar StatusBar { get; set; }

        public void Set(BusyModes mode)
        {
            _mode |= mode;
            _check();
        }

        public void Clear(BusyModes mode)
        {
            if (_mode.HasFlag(mode))
                _mode ^= mode;
            _check();
        }

        private StatusBar _bar = StatusBar.GetForCurrentView();

        private async void _check()
        {
            if (_mode == BusyModes.None)
            {
                Core.Dispatcher.RunAsync(CoreDispatcherPriority.Low, delegate
                {
                    StatusBar.ProgressIndicator.HideAsync();
                });
            }
            else
            {
                var modes = typeof (BusyModes)
                    .GetRuntimeFields()
                    .Where(c=>c.FieldType==typeof(BusyModes))
                    .Select(c => (BusyModes) c.GetValue(null))
                    .Where(c=>c != BusyModes.None && _mode.HasFlag(c))
                    .ToList();
                var member = _mode.GetType().GetRuntimeField(modes[0].ToString());
                var text = member.GetCustomAttribute<LabelAttribute>().Text;
                Core.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async delegate
                {
                    StatusBar.BackgroundOpacity = 1;
                    StatusBar.ProgressIndicator.Text = text;
                    StatusBar.ProgressIndicator.ShowAsync();
                });
            }
        }
    }

    [Flags]
    public enum BusyModes
    {
        None = 0,

        [Label(Text="Синхронизация")]
        Sync,

        [Label(Text="Обновление пароля семьи")]
        UpdateFamilyPassword,

    }

    internal class LabelAttribute : Attribute
    {
        public string Text { get; set; }
    }
}
