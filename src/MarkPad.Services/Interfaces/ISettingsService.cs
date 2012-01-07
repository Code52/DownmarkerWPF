using System.Collections.Generic;

namespace MarkPad.Services.Interfaces
{
    public interface ISettingsService
    {
        IList<string> GetRecentFiles();
        void UpdateRecentFiles(IList<string> files);
        void Save();
    }
}
