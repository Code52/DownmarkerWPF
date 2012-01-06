using System.Collections.Generic;

namespace MarkPad.Services.Interfaces
{
    public interface ISettingsService
    {
        IEnumerable<string> GetRecentFiles();
        void AddRecentFile(string path);
        void Save();
    }
}
