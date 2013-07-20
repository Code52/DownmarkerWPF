using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MarkPad.Settings.UI;
using Microsoft.Win32;

namespace MarkPad.Settings
{
    public interface IMarkpadRegistryEditor
    {
        void UpdateExtensionRegistryKeys(IEnumerable<ExtensionViewModel> extensions);
        IEnumerable<ExtensionViewModel> GetExtensionsFromRegistry();
    }

    public class MarkpadRegistryEditor : IMarkpadRegistryEditor
    {
        private const string MarkpadKeyName = "markpad.md";

        public void UpdateExtensionRegistryKeys(IEnumerable<ExtensionViewModel> extensions)
        {
            var exePath = Assembly.GetEntryAssembly().Location;

            var software = Registry.CurrentUser.OpenSubKey("Software");
            if (software == null) return;
            using (var classesKey = software.OpenSubKey("Classes", true))
            {
                if (classesKey == null) return;
                foreach (var ext in extensions)
                {
                    using (var extensionKey = classesKey.CreateSubKey(ext.Extension))
                    {
                        if (extensionKey != null)
                            extensionKey.SetValue("", ext.Enabled ? MarkpadKeyName : "");
                    }
                }

                using (var markpadKey = classesKey.CreateSubKey(MarkpadKeyName))
                {
                    if (markpadKey == null) return;
                    using (var defaultIconKey = markpadKey.CreateSubKey("DefaultIcon"))
                    {
                        if (defaultIconKey != null)
                            defaultIconKey.SetValue("", Path.Combine(Constants.IconDir, Constants.Icons[0]));
                    }

                    using (var shellKey = markpadKey.CreateSubKey("shell"))
                    {
                        if (shellKey == null) return;
                        using (var openKey = shellKey.CreateSubKey("open"))
                        {
                            if (openKey == null) return;
                            using (var commandKey = openKey.CreateSubKey("command"))
                            {
                                if (commandKey != null) commandKey.SetValue("", "\"" + exePath + "\" \"%1\"");
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<ExtensionViewModel> GetExtensionsFromRegistry()
        {
            IEnumerable<ExtensionViewModel> extensions = new ExtensionViewModel[0];
            var softwareKey = Registry.CurrentUser.OpenSubKey("Software");
            if (softwareKey != null)
            {
                using (var key = softwareKey.OpenSubKey("Classes"))
                {
                    if (key != null)
                    {
                        extensions = Constants.DefaultExtensions
                                              .Select(s =>
                                              {

                                                  var openSubKey = key.OpenSubKey(s);
                                                  return new ExtensionViewModel(s, Enabled(s, key, openSubKey));
                                              })
                                              .Where(e => e != null)
                                              .ToArray();
                    }
                }
            }
            return extensions;
        }

        private bool Enabled(string s, RegistryKey key, RegistryKey openSubKey)
        {
            var defaultNameValue = openSubKey == null ? null : openSubKey.GetValue(null);
            var defaultNameValueAsString = defaultNameValue == null ? null : defaultNameValue.ToString();
            return openSubKey != null &&
                   (key.GetSubKeyNames().Contains(s) &&
                    !string.IsNullOrEmpty(defaultNameValueAsString));
        }
    }
}