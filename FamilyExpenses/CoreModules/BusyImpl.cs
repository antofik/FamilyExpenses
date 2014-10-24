using System;
using System.Linq;
using System.Reflection;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

namespace FamilyExpenses.CoreModules
{
    public class BusyImpl
    {
        private BusyModes _mode = BusyModes.None;
        public StatusBar StatusBar { get; set; }

        private string ErrorMessage;

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

        private async void _check()
        {
            if (_mode == BusyModes.None)
            {
                Core.Dispatcher.RunAsync(CoreDispatcherPriority.Low, delegate
                {
                    if (string.IsNullOrEmpty(ErrorMessage))
                        StatusBar.ProgressIndicator.HideAsync();
                    else
                    {
                        StatusBar.BackgroundOpacity = 1;
                        StatusBar.ProgressIndicator.Text = ErrorMessage;
                        StatusBar.ForegroundColor = Colors.Red;
                        StatusBar.ProgressIndicator.ProgressValue = 0;
                        StatusBar.ProgressIndicator.ShowAsync();
                    }
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
                    StatusBar.ProgressIndicator.ProgressValue = null;
                    StatusBar.ForegroundColor = null;
                    StatusBar.ProgressIndicator.ShowAsync();
                });
            }
        }

        public void SetError(string errorMessage)
        {
            ErrorMessage = errorMessage;
            _check();
        }

        public void ClearError()
        {
            ErrorMessage = string.Empty;
            _check();
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
