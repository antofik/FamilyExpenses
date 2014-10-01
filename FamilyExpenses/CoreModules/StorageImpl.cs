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
using Windows.Storage.Streams;
using FamilyExpenses.Models;

namespace FamilyExpenses.CoreModules
{
    public class StorageImpl
    {
        public async void Load(Action callback)
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("entries", CreationCollisionOption.OpenIfExists);
            using (var read = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (var stream = read.AsStreamForRead())
                {
                    try
                    {
                        var length = (int) stream.Length;
                        var bytes = new byte[length];
                        stream.Read(bytes, 0, length);

                        Core.Entries = Deserialize<ObservableCollection<Entry>>(bytes) ??
                                       new ObservableCollection<Entry>();
                    }
                    catch (Exception)
                    {
                        Core.Entries = new ObservableCollection<Entry>();
                    }
                    callback();
                }
            }
        }

        public void AddEntry(Entry entry)
        {
            Core.Entries.Add(entry);
            Save();
        }

        public async void Save()
        {
            var value = Serialize(Core.Entries);
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("entries", CreationCollisionOption.ReplaceExisting);
            using (var read = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (var stream = read.AsStreamForWrite())
                {
                    stream.Write(value, 0, value.Length);
                }
            }
        }

        public T Deserialize<T>(byte[] bytes)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var stream = new MemoryStream(bytes))
            {
                return (T) serializer.ReadObject(stream);
            }

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


        /// <summary>
        /// Syncronize data with server
        /// </summary>
        public void Sync()
        {
            
        }
    }
}
