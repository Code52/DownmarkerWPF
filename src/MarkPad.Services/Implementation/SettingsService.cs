using System.Collections.Generic;
using System.IO;
using MarkPad.Services.Interfaces;
using Newtonsoft.Json;

namespace MarkPad.Services.Implementation
{
    public class SettingsService : ISettingsService
    {
        private readonly string filePath;
        private readonly Settings settings;

        public SettingsService()
        {
            filePath = Path.Combine(Path.GetTempPath(), "settings.json");
            
            if (File.Exists(filePath))
            {
                var contents = File.ReadAllText(filePath);
                settings = JsonConvert.DeserializeObject<Settings>(contents);
            }
            else
            {
                settings = new Settings();
            }
        }

        public IList<string> GetRecentFiles()
        {
            return settings.RecentFiles;
        }

        public void UpdateRecentFiles(IList<string> files)
        {
            settings.RecentFiles = files;
        }

        public void AddRecentFile(string path)
        {
            settings.RecentFiles.Insert(0, path);
        }

        public void Save()
        {
            var contents = JsonConvert.SerializeObject(settings);
            File.WriteAllText(filePath, contents);
        }
    }

    internal class Settings
    {
        public Settings()
        {
            RecentFiles = new List<string>();
        }

        public IList<string> RecentFiles { get; set; }
    }
}