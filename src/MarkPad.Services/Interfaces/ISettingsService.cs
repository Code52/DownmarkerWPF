using System;

namespace MarkPad.Services.Interfaces
{
    [Obsolete]
    public interface ISettingsService
    {
        T Get<T>(string key);
        void Set<T>(string key, T value);
        void Save();
		void SetAsDefault<T>(string key, T value);
    }
}
