using System.Linq;
using System.Reflection;
using Caliburn.Micro;
using Microsoft.Win32;

namespace MarkPad.Settings
{
    // TODO: saving for .mdown file extension

    public class SettingsViewModel : Screen
    {
        private const string markpadKeyName = "markpad.md";

        public SettingsViewModel()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes"))
            {
                FileMDBinding = key.GetSubKeyNames().Contains(Constants.DefaultExtensions[0]) &&
                    !string.IsNullOrEmpty(key.OpenSubKey(Constants.DefaultExtensions[0]).GetValue("").ToString());

                FileMarkdownBinding = key.GetSubKeyNames().Contains(Constants.DefaultExtensions[1]) &&
                    !string.IsNullOrEmpty(key.OpenSubKey(Constants.DefaultExtensions[1]).GetValue("").ToString());

                FileMarkdownBinding = key.GetSubKeyNames().Contains(Constants.DefaultExtensions[2]) &&
                    !string.IsNullOrEmpty(key.OpenSubKey(Constants.DefaultExtensions[2]).GetValue("").ToString());
            }
        }

        public bool FileMDBinding { get; set; }
        public bool FileMarkdownBinding { get; set; }
        public bool FileMDownBinding { get; set; }

        public void Accept()
        {
            UpdateExtensionRegistryKeys();

            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }

        private void UpdateExtensionRegistryKeys()
        {
            string exePath = Assembly.GetEntryAssembly().Location;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes", true))
            {
                for (int i = 0; i < Constants.DefaultExtensions.Length; i++)
                {
                    using (RegistryKey extensionKey = key.CreateSubKey(Constants.DefaultExtensions[i]))
                    {
                        if ((i == 0 && FileMDBinding) ||
                            (i == 1 && FileMarkdownBinding))
                            extensionKey.SetValue("", markpadKeyName);
                        else
                            extensionKey.SetValue("", "");
                    }
                }

                using (RegistryKey markpadKey = key.CreateSubKey(markpadKeyName))
                {
                    // Can't get this to work right now.
                    //using (RegistryKey defaultIconKey = markpadKey.CreateSubKey("DefaultIcon"))
                    //{
                    //    defaultIconKey.SetValue("", exePath + ",1");
                    //}

                    using (RegistryKey shellKey = markpadKey.CreateSubKey("shell"))
                    {
                        using (RegistryKey openKey = shellKey.CreateSubKey("open"))
                        {
                            using (RegistryKey commandKey = openKey.CreateSubKey("command"))
                            {
                                commandKey.SetValue("", "\"" + exePath + "\" \"%1\"");
                            }
                        }
                    }
                }
            }
        }
    }
}
