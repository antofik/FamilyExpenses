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
using FamilyExpenses.Common;
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

        public static readonly LogHelper Log = new LogHelper();

        public static readonly string PhoneId = string.Join("", HardwareIdentification.GetPackageSpecificToken(null).Id.ToArray().Select(c => c.ToString("X")));

        private static StatusBar _statusBar = StatusBar.GetForCurrentView();

        #region Categories

        private static readonly ObservableCollection<Category> _categories = new ObservableCollection<Category>();
        public static ObservableCollection<Category> Categories { get { return _categories; }  }

        #endregion

        #region Entries

        private static ObservableCollection<Entry> _entries = new ObservableCollection<Entry>();
        public static ObservableCollection<Entry> Entries { get { return _entries; } }

        public static void UpdateFamilyPassword(string value, Action<bool> callback)
        {
            Task.Run(() => _updatePassword(value, callback));

        }

        public static event Action Updated = delegate{};
        public static event Action Refresh = delegate{};

        public static void InvokeRefresh()
        {
            Refresh();
        }

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

            Log.Add("Updating password from {0} to {1}", Storage.FamilyPassword, value);

            Busy.Set(BusyModes.UpdateFamilyPassword);
            var task = client.PostAsync(new Uri(Server + "/family-expenses/change-family"), content);
            if (await Task.WhenAny(task, Task.Delay(AsyncTimeout)) == task)
            {
                Busy.Clear(BusyModes.UpdateFamilyPassword);

                var result = task.Result;

                if (result.IsSuccessStatusCode)
                {
                    Busy.ClearError();
                    Log.Add("Successfully update from {0} to {1}", Storage.FamilyPassword, value);
                    Storage.SetFamilyPassword(value);
                    Storage.SetRevision(0);
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        // remove items of other people
                        var othersItems = Entries.Where(c => !c.IsMine).ToList();
                        foreach (var item in othersItems)
                            Entries.Remove(item);
                        // and mark all my items as modified
                        foreach (var item in Entries)
                            item.Modified = true;
                    });
                    Syncronize();
                    callback(true);
                }
                else
                {
                    Busy.SetError("No internet connection");
                    callback(false);
                }
            }
            else
            {
                Busy.Clear(BusyModes.UpdateFamilyPassword);
                callback(false);
            }
        }
        
        public static void Syncronize(bool check = true)
        {
            if (check && string.IsNullOrWhiteSpace(Storage.FamilyPassword)) return;
            Task.Run(() => SyncronizeAsync());
        }

        private static async Task<bool> SyncronizeAsync()
        {
            lock (_syncRoot)
            {
                if (_syncronizing)
                {
                    _syncronizeOnceMore = true;
                    return false;
                }
                _syncronizeOnceMore = false;
                _syncronizing = true;
            }


            Busy.Set(BusyModes.Sync);

            bool ok;
            try
            {
                ok = await _syncronize();
                while (ok && _syncronizeOnceMore)
                {
                    _syncronizeOnceMore = false;
                    ok = await _syncronize();
                }
            }
            catch (Exception ex)
            {
                ok = false;
                Log.Add("Error while syncronizing: {0}", ex);
            }

            Busy.Clear(BusyModes.Sync);

            lock (_syncRoot)
            {
                _syncronizing = false;
            }
            return ok;
        }

        private static async Task<bool> _syncronize()
        {
            var errorMessage = "";
            try
            {
                Log.Add("Start syncronizing {0}", Storage.Revision);
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
                Log.Add("Items to upload: {0}", entries.Count);
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
                        Busy.ClearError();
                        var json = await result.Content.ReadAsStringAsync();
                        var list = Storage.Deserialize<ResponseDTO>(Encoding.UTF8.GetBytes(json));
                        Log.Add("Successfuly syncronized to {0} revision", list.Revision);
                        Log.Add("Received {0} items", list.Data.Count);
                        Storage.SetRevision(list.Revision);
                        foreach (var str in list.Data)
                        {
                            var entry = Storage.Deserialize<Entry>(Encoding.UTF8.GetBytes(str));
                            Log.Add("    item {0} {1} {2} {3} {4}", entry.Id, entry.Categories, entry.Cost, entry.Date, entry.IsMine ? "mine" : "others");
                            var existant = Entries.FirstOrDefault(c => c.Id == entry.Id);
                            if (existant != null)
                            {
                                existant.Categories = entry.Categories;
                                existant.Cost = entry.Cost;
                                existant.Owner = entry.Owner;
                                existant.Revision = entry.Revision;
                                existant.Date = entry.Date;
                                existant.Modified = false;
                                Log.Add("      - exists");
                            }
                            else
                            {
                                entry.Modified = false;
                                Entries.Add(entry);
                                Log.Add("      - new");
                            }
                        }
                        Storage.Save();
                        Log.Add("Sync finished.");
                        Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Updated());
                    }
                    else
                    {
                        errorMessage = await result.Content.ReadAsStringAsync();
                        Log.Add("Error in sync service: {0}", errorMessage);
                    }
                }
                else
                {
                    errorMessage = "No internet connection";
                    Log.Add("Cannot sync: no internet");
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Error while sync: " + ex.Message;
                Log.Add("Error while sync: {0}", ex);
                new MessageDialog("Error while sync\n" + ex, "Fatal error").ShowAsync();
            }
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Busy.SetError(errorMessage);
                return false;
            }
            Busy.ClearError();
            return true;
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
