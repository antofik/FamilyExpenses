using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using FamilyExpenses.Models;

namespace FamilyExpenses.CoreModules
{
    public class StorageImpl
    {
        private readonly ApplicationDataContainer Settings = ApplicationData.Current.LocalSettings;

        public async void Load(Action callback)
        {
            var entries = Settings.Values["entries"] as byte[];
            Core.Entries = Deserialize<ObservableCollection<Entry>>(entries) ?? new ObservableCollection<Entry>();
            Core.Syncronize();
            callback();
        }

        public void AddEntry(Entry entry)
        {
            Core.Entries.Add(entry);
            Save();
            Core.Syncronize();
            
        }

        public string FamilyPassword
        {
            get { return Settings.Values["FamilyPassword"] as string ?? ""; }
            set { Settings.Values["FamilyPassword"] = value ?? ""; }
        }

        public double Revision
        {
            get { return (double) (Settings.Values["Revision"] ?? 0d); }
            set { Settings.Values["Revision"] = value; }
        }

        public async void Save()
        {
            var value = Serialize((object)Core.Entries);
            Settings.Values["Entries"] = value;
        }

        public T Deserialize<T>(byte[] bytes)
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof (T));
                using (var stream = new MemoryStream(bytes))
                {
                    return (T) serializer.ReadObject(stream);
                }
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        public string Serialize<T>(T obj) where T : class
        {
            var bytes = Serialize((object)obj);
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public byte[] Serialize(object obj)
        {
            var type = obj.GetType();
            var serializer = new DataContractJsonSerializer(type);
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, obj);
                return stream.ToArray();
            }
        }
    }
}
