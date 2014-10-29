using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.UI.Core;
using FamilyExpenses.CoreModules;

namespace FamilyExpenses.Common
{
    public class LogHelper
    {
        public LogHelper()
        {
            Initialize();
        }

        private async void Initialize()
        {
            _history = new ObservableCollection<HistoryLogItem>();
        }

        private ObservableCollection<HistoryLogItem> _history;
        public ObservableCollection<HistoryLogItem> History { get { return _history; } }

        public void Add(string message, params object[] parameters)
        {
            Task.Run(async () => { 
                if (parameters != null && parameters.Length > 0)
                    message = string.Format(message, parameters);
                var item = new HistoryLogItem
                {
                    Date = DateTime.Now,
                    Message = message
                };
                await Core.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    History.Add(item);
                    if (History.Count > 100)
                        History.RemoveAt(0);
                });
                Save();
            });
        }

        private void Save()
        {
            var data = Core.Storage.Serialize(_history);
            //Core.Storage.Save("HistoryLog", data);
        }
    }

    [DataContract]
    public class HistoryLogItem
    {
        [DataMember]
        public DateTime Date { get; set; }

        [DataMember]
        public string Message { get; set; }
    }
}
