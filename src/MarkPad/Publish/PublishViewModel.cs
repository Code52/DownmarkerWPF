using Caliburn.Micro;
using MarkPad.Services.Interfaces;

namespace MarkPad.Publish
{
    public class PublishViewModel : Screen
    {
        private readonly ISettingsService _settings;

        public PublishViewModel(ISettingsService settings)
        {
            _settings = settings;
        }

        public string BlogUrl
        {
            get { return _settings.Get<string>("BlogUrl"); }
            set { _settings.Set("BlogUrl", value); }
        }

        public string Username
        {
            get { return _settings.Get<string>("Username"); }
            set { _settings.Set("Username", value); }
        }

        public string Password
        {
            get { return _settings.Get<string>("Password"); }
            set { _settings.Set("Password", value); }
        }
    }
}
