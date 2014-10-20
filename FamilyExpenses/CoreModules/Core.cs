using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Profile;
using FamilyExpenses.Models;

namespace FamilyExpenses.CoreModules
{
    public static class Core
    {
#if DEBUG
        private const string Server = "http://meimew.com";
#else
        private const string Server = "http://localhost:8000";
#endif

        public static readonly StorageImpl Storage = new StorageImpl();

        public static readonly string PhoneId = string.Join("", HardwareIdentification.GetPackageSpecificToken(null).Id.ToArray().Select(c => c.ToString("X")));

        #region Categories

        private static ObservableCollection<Category> _categories = new ObservableCollection<Category>();
        public static ObservableCollection<Category> Categories { get { return _categories; } }

        #endregion

        #region Entries

        private static ObservableCollection<Entry> _entries = new ObservableCollection<Entry>();
        public static ObservableCollection<Entry> Entries { get { return _entries; } set { _entries = value; } }

        public static string FamilyPassword = "";

        public static void UpdateFamilyPassword(string value)
        {
            Task.Run(() => _updatePassword(value));

        }

        #endregion

        public static long Revision = -1;

        private static readonly object _syncRoot = new Object();
        private static bool _syncronizing;
        private static bool _syncronizeOnceMore;

        public static async void _updatePassword(string value)
        {
            var client = new HttpClient();
            var content = new MultipartFormDataContent
            {
                {new StringContent(PhoneId), "PhoneId"},
                {new StringContent(value), "FamilyPassword"},
            };
            var result = await client.PostAsync(new Uri(Server + "/family-expenses/change-family"), content);
            if (result.IsSuccessStatusCode)
            {
                FamilyPassword = value;
                Syncronize();
            }
            else
            {
                //TODO show error message
            }
        }
        
        public static void Syncronize()
        {
            if (string.IsNullOrWhiteSpace(FamilyPassword)) return;
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
            await _syncronize();
            while (_syncronizeOnceMore)
            {
                _syncronizeOnceMore = false;
                await _syncronize();
            }
            lock (_syncRoot)
            {
                _syncronizing = false;
            }
        }

        private static async Task _syncronize()
        {
            var client = new HttpClient();

            var entries = Entries.Where(c => c.Modified)
                .Select(c => new EntryDTO
                {
                    Id = c.Id,
                    Revision = c.Revision,
                    Owner = c.Owner,
                    FamilyPassword = FamilyPassword,
                    Data = Storage.Serialize(c)
                })
                .ToList();
            var data = Storage.Serialize(entries);

            var content = new MultipartFormDataContent
            {
                {new StringContent(PhoneId), "PhoneId"},
                {new StringContent(FamilyPassword), "FamilyPassword"},
                {new StringContent(Revision.ToString()), "Revision"},
                {new StringContent(data), "Data"},
            };
            var result = await client.PostAsync(new Uri(Server + "/family-expenses"), content);
            if (result.IsSuccessStatusCode)
            {
                var json = await result.Content.ReadAsStringAsync();
                var list = Storage.Deserialize<ResponseDTO>(Encoding.UTF8.GetBytes(json));
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
            }
            else
            {
                //todo log error
                var str = await result.Content.ReadAsStringAsync();
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
