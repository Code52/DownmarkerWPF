using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Formatters.Binary;
using MarkPad.Services.Interfaces;

namespace MarkPad.Services.Implementation
{
    internal class SettingsService : ISettingsService
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IsolatedStorageScope scope = IsolatedStorageScope.Assembly | IsolatedStorageScope.User | IsolatedStorageScope.Roaming;

        private const string Filename = "settings.bin";
        private Dictionary<string, object> _storage = new Dictionary<string, object>();

        public SettingsService()
        {
            _storage = new Dictionary<string, object>();

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(scope, null, null))
            {
                string[] filenames = isoStore.GetFileNames(Filename);
                if (Filename.Length > 0)
                {
                    LoadStorage(isoStore);
                }
            }
        }

        public T Get<T>(string key)
        {
            if (_storage.ContainsKey(key))
                return (T)_storage[key];
            return default(T);
        }

        public void Set<T>(string key, T value)
        {
            if (!_storage.ContainsKey(key))
                _storage.Add(key, value);
            else
                _storage[key] = value;

        }

        private void LoadStorage(IsolatedStorageFile isoStore)
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();

                using (var stream = new IsolatedStorageFileStream(Filename, FileMode.Open, isoStore))
                    _storage = (Dictionary<string, object>)formatter.Deserialize(stream);
            }
            catch (Exception e)
            {
                logger.WarnException(e.Message, e);
            }
        }

        public void Save()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(scope, null, null))
            using (var stream = new IsolatedStorageFileStream(Filename, FileMode.Create, isoStore))
                formatter.Serialize(stream, _storage);
        }
    }
}