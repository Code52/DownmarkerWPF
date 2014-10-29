using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

namespace MarkPad.Settings
{
    public class FileSystemStorageSettingsStore : JsonSettingsStoreBase
    {
        private readonly SemaphoreSlim fileReadWriteMutex = new SemaphoreSlim(1);

        protected override void WriteTextFile(string filename, string fileContents)
        {
            fileReadWriteMutex.Wait();
            try
            {
                var dir = GetSettingsDirectory();
                File.WriteAllText(Path.Combine(dir, filename), fileContents, Encoding.UTF8);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                fileReadWriteMutex.Release();
            }

        }

        protected override string ReadTextFile(string filename)
        {
            fileReadWriteMutex.Wait();
            try
            {
                var dir = GetSettingsDirectory();
                var settingsFile = Path.Combine(dir, filename);

                if (!File.Exists(settingsFile))
                    return null;

                return File.ReadAllText(settingsFile, Encoding.UTF8);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                fileReadWriteMutex.Release();
            }

        }

        static string GetSettingsDirectory()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var settingsDir = Path.Combine(appData, "Markpad");
            Directory.CreateDirectory(settingsDir);
            return settingsDir;
        }
    }

    public abstract class JsonSettingsStoreBase : ISettingsStorage
    {
        public string SerializeList(List<string> listOfItems)
        {
            var ms = new MemoryStream();
            var writer = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.Unicode);
            new DataContractJsonSerializer(typeof(List<string>)).WriteObject(ms, listOfItems);
            writer.Flush();
            var jsonString = Encoding.Default.GetString(ms.ToArray());

            return jsonString;
        }

        public List<string> DeserializeList(string serializedList)
        {
            return (List<string>)new DataContractJsonSerializer(typeof(List<string>))
                .ReadObject(new MemoryStream(Encoding.Default.GetBytes(serializedList)));
        }

        public void Save(string key, Dictionary<string, string> settings)
        {
            var filename = key + ".settings";

            var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>));
            var ms = new MemoryStream();
            var writer = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.Unicode);
            serializer.WriteObject(ms, settings);
            writer.Flush();
            var jsonString = Encoding.Default.GetString(ms.ToArray());
            WriteTextFile(filename, jsonString);
        }

        protected abstract void WriteTextFile(string filename, string fileContents);

        public Dictionary<string, string> Load(string key)
        {
            var filename = key + ".settings";

            var readTextFile = ReadTextFile(filename);
            if (!string.IsNullOrEmpty(readTextFile))
            {
                var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>));
                return (Dictionary<string, string>)serializer.ReadObject(new MemoryStream(Encoding.Default.GetBytes(readTextFile)));
            }

            return new Dictionary<string, string>();
        }

        protected abstract string ReadTextFile(string filename);
    }
}
