using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using FamilyExpenses.Models;

namespace FamilyExpenses.CoreModules
{
    public static class Core
    {
        public const int AsyncTimeout = 5000;

#if DEBUG
        private const string Server = "http://meimew.com";
#else
        private const string Server = "http://meimew.com";
#endif

        public static readonly StorageImpl Storage = new StorageImpl();

        public static readonly BusyImpl Busy = new BusyImpl();

        public static readonly string PhoneId = string.Join("", HardwareIdentification.GetPackageSpecificToken(null).Id.ToArray().Select(c => c.ToString("X")));

        private static StatusBar _statusBar = StatusBar.GetForCurrentView();

        #region Categories

        private static ObservableCollection<Category> _categories = new ObservableCollection<Category>();
        public static ObservableCollection<Category> Categories { get { return _categories; } }

        #endregion

        #region Entries

        private static ObservableCollection<Entry> _entries = new ObservableCollection<Entry>();
        public static ObservableCollection<Entry> Entries { get { return _entries; } set { _entries = value; } }

        public static void UpdateFamilyPassword(string value, Action<bool> callback)
        {
            Task.Run(() => _updatePassword(value, callback));

        }

        public static event Action Updated = delegate{};

        #endregion
        
        public static CoreDispatcher Dispatcher;

        private static readonly object _syncRoot = new Object();
        private static bool _syncronizing;
        private static bool _syncronizeOnceMore;

        public static async void _updatePassword(string value, Action<bool> callback)
        {
            var client = new HttpClient();
            var content = new MultipartFormDataContent
            {
                {new StringContent(PhoneId), "PhoneId"},
                {new StringContent(value), "FamilyPassword"},
            };

            Busy.Set(BusyModes.UpdateFamilyPassword);
            var result = await client.PostAsync(new Uri(Server + "/family-expenses/change-family"), content);
            Busy.Clear(BusyModes.UpdateFamilyPassword);

            if (result.IsSuccessStatusCode)
            {
                Storage.FamilyPassword = value;
                Storage.Revision = 0;
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, Entries.Clear);
                Syncronize();
                callback(true);
            }
            else
            {
                Busy.SetError("No internet connection");
                callback(false);
            }
        }
        
        public static void Syncronize()
        {
            if (string.IsNullOrWhiteSpace(Storage.FamilyPassword)) return;
            Task.Run(() => SyncronizeAsync());
        }

        private static async void SyncronizeAsync()
        {
            lock (_syncRoot)
            {
                if (_syncronizing)
                {
                    _syncronizeOnceMore = true;
                    return;
                }
                _syncronizeOnceMore = false;
                _syncronizing = true;
            }


            Busy.Set(BusyModes.Sync);

            await _syncronize();
            while (_syncronizeOnceMore)
            {
                _syncronizeOnceMore = false;
                await _syncronize();
            }
            Busy.Clear(BusyModes.Sync);

            lock (_syncRoot)
            {
                _syncronizing = false;
            }
        }

        private static async Task _syncronize()
        {
            var errorMessage = "";
            try
            {
                var client = new HttpClient();

                var entries = Entries.Where(c => c.Modified)
                    .Select(c => new EntryDTO
                    {
                        Id = c.Id,
                        Revision = c.Revision,
                        Owner = c.Owner,
                        FamilyPassword = Storage.FamilyPassword,
                        Data = Storage.Serialize(c)
                    })
                    .ToList();
                var data = Storage.Serialize(entries);

                var content = new MultipartFormDataContent
                {
                    {new StringContent(PhoneId), "PhoneId"},
                    {new StringContent(Storage.FamilyPassword), "FamilyPassword"},
                    {new StringContent(Storage.Revision.ToString(CultureInfo.InvariantCulture)), "Revision"},
                    {new StringContent(data), "Data"},
                };

                var task = client.PostAsync(new Uri(Server + "/family-expenses"), content);
                if (await Task.WhenAny(task, Task.Delay(AsyncTimeout)) == task)
                {
                    var result = task.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var json = await result.Content.ReadAsStringAsync();
                        var list = Storage.Deserialize<ResponseDTO>(Encoding.UTF8.GetBytes(json));
                        Storage.Revision = list.Revision;
                        foreach (var str in list.Data)
                        {
                            var entry = Storage.Deserialize<Entry>(Encoding.UTF8.GetBytes(str));
                            var existant = Entries.FirstOrDefault(c => c.Id == entry.Id);
                            if (existant != null)
                            {
                                existant.Categories = entry.Categories;
                                existant.Cost = entry.Cost;
                                existant.Owner = entry.Owner;
                                existant.Revision = entry.Revision;
                                existant.Date = entry.Date;
                                existant.Modified = false;
                            }
                            else
                            {
                                entry.Modified = false;
                                Entries.Add(entry);
                            }
                        }
                        Storage.Save();
                        Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Updated());
                    }
                    else
                    {
                        errorMessage = await result.Content.ReadAsStringAsync();
                    }
                }
                else
                {
                    errorMessage = "No internet connection";
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Error while sync: " + ex.Message;
                new MessageDialog("Error while sync\n" + ex, "Fatal error").ShowAsync();
            }
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Busy.SetError(errorMessage);
            }
            else
            {
                Busy.ClearError();
            }
        }
    }


    [DataContract]
    public class ResponseDTO
    {
        [DataMember]
        public double Revision { get; set; }

        [DataMember]
        public List<string> Data { get; set; } 
    }

    public class EntryDTO
    {
        public Guid Id { get; set; }
        public double Revision { get; set; }
        public string Owner { get; set; }
        public string Data { get; set; }
        public string FamilyPassword { get; set; }
    }
}
