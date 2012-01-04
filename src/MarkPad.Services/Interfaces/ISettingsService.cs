namespace MarkPad.Services.Interfaces
{
    public interface ISettingsService
    {
        T Get<T>(string key);
        void Set<T>(string key, T value);
        void Save();
    }
}