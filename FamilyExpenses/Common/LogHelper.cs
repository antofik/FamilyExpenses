using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using FamilyExpenses.CoreModules;

namespace FamilyExpenses.Common
{
    public class LogHelper
    {
        private readonly ObservableCollection<HistoryLogItem> _history = Core.Storage.Load("HistoryLog", new ObservableCollection<HistoryLogItem>());
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
            Core.Storage.Save("HistoryLog", data);
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
