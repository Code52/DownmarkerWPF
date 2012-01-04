using System.Collections;
using System.IO;
using MarkPad.Services.Interfaces;
using Polenter.Serialization;
using Polenter.Serialization.Core;

namespace MarkPad.Services.Implementation
{
    internal class SettingsService :ISettingsService
    {
        private readonly Hashtable _storage = new Hashtable();
        private readonly SharpSerializer _serializer = new SharpSerializer();
        private const string Filename = "settings.xml";

        public SettingsService()
        {
            try
            {
                _storage = (Hashtable)_serializer.Deserialize(Filename);
            }
            catch (FileNotFoundException ex)
            {
                
            }
            catch (DeserializingException ex)
            {
                //File probably doesn't exist or is corrupt
            }
        }
        public T Get<T>(string key)
        {
            if (_storage.Contains(key))
                return (T) _storage[key];
            return default(T);
        }

        public void Set<T>(string key, T value)
        {
            if (!_storage.ContainsKey(key))
                _storage.Add(key, value);
            else
                _storage[key] = value;

        }

        public void Save()
        {
            _serializer.Serialize(_storage, Filename);
        }
    }
}