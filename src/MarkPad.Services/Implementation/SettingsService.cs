using System.Collections.Generic;
using MarkPad.Services.Interfaces;

namespace MarkPad.Services.Implementation
{
    public class SettingsService : ISettingsService
    {
        public IEnumerable<string> GetRecentFiles()
        {
            return new List<string>
                       {
                           @"D:\Code\github\code52\code52website\_posts\2011-12-31-introduction.md",
                           @"D:\Code\github\code52\code52website\_posts\2012-01-02-downmarker.md"
                       };
        }

        public void AddRecentFile(string path)
        {
            throw new System.NotImplementedException();
        }
    }
}