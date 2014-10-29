using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using FamilyExpenses.Models;

namespace FamilyExpenses.CoreModules
{
    public class StorageImpl
    {
        private bool _runSaver;
        private readonly Dictionary<SaveEntity, bool> _entitiesToSave = new Dictionary<SaveEntity, bool>();
        public async void Load(Action callback)
        {
            var entries = await _load("entries", new ObservableCollection<Entry>());
            try
            {
                var settings = ApplicationData.Current.LocalSettings;
                var bytes = settings.Values["entries"] as byte[];
                if (bytes != null && bytes.Length > 0)
                {
                    var items = Deserialize(bytes, new ObservableCollection<Entry>());
                    foreach(var item in items)
                        Core.Entries.Add(item);
                    Core.Storage.Save();
                    settings.Values["entries"] = null;
                }
            }
            catch (Exception ex) { }
            foreach(var entry in entries)
                Core.Entries.Add(entry);
            var categories = await _load("categories", 
                new ObservableCollection<Category>("Еда;Дорога;Дети;Подарки;Одежда;Лечение;Личное;Квартира;Другое".Split(';').Select(c => new Category(c))));
            foreach(var category in categories)
                Core.Categories.Add(category);

            FamilyPassword = await _load("familyPassword", "");
            Revision = await _load("revision", 0d);
            //Revision = Core.Entries.Count > 0 ? Core.Entries.Max(c => c.Revision) : 0d;
            
            Core.Syncronize();
            callback();

            if (!_runSaver)
            {
                _runSaver = true;
                Task.Run((Func<Task>) Saver);
            }
        }

        private async Task Saver()
        {
            while (_runSaver)
            {
                await Task.Delay(100);
                foreach (SaveEntity value in Enum.GetValues(typeof(SaveEntity)))
                {
                    bool ok;
                    if (!_entitiesToSave.TryGetValue(value, out ok) || !ok) continue;
                    _entitiesToSave[value] = false;
                    object obj = null;

                    switch (value)
                    {
                        case SaveEntity.Categories:
                            obj = Core.Categories;
                            break;
                        case SaveEntity.Entries:
                            obj = Core.Entries;
                            break;
                        case SaveEntity.FamilyPassword:
                            obj = FamilyPassword;
                            break;
                        case SaveEntity.Revision:
                            obj = Revision;
                            break;
                        case SaveEntity.HistoryLog:
                            obj = Core.Log.History;
                            break;
                    }
                    if (obj != null)
                        await _save(value.ToString(), obj);
                }
            }
        }

        public void AddEntry(Entry entry)
        {
            Core.Entries.Add(entry);
            _entitiesToSave[SaveEntity.Entries] = true;
            Core.Syncronize();
        }
        
        public string FamilyPassword { get; private set; }

        public void SetFamilyPassword(string value)
        {
            FamilyPassword = value;
            _entitiesToSave[SaveEntity.FamilyPassword] = true;
        }

        public double Revision { get; private set; }

        public void SetRevision(double value)
        {
            Revision = value;
            _entitiesToSave[SaveEntity.Revision] = true;
            
        }

        public void Save()
        {
            _entitiesToSave[SaveEntity.Categories] = true;
            _entitiesToSave[SaveEntity.Entries] = true;
        }

        public T Deserialize<T>(byte[] bytes, T defaultValue = default(T))
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
                return defaultValue;
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

        private async Task _save(string name, object value)
        {
            try
            {
                var stream = new MemoryStream();
                var serializer = new DataContractSerializer(value.GetType());
                serializer.WriteObject(stream, value);

                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(string.Format("{0}.settings", name), CreationCollisionOption.ReplaceExisting);
                using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    await stream.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                Core.Log.Add("Error while saving {0}", ex);
            }
        }

        private async Task<T> _load<T>(string name, T defaultValue = default(T))
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(string.Format("{0}.settings", name));
                using (var inStream = await file.OpenSequentialReadAsync())
                {
                    var serializer = new DataContractSerializer(typeof (T));
                    return (T) serializer.ReadObject(inStream.AsStreamForRead());
                }
            }
            catch (Exception ex)
            {
                return defaultValue;
            }
        }
    }

    internal enum SaveEntity
    {
        Entries,
        Categories,
        Revision,
        FamilyPassword,
        HistoryLog,
    }
}
