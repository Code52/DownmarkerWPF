using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Caliburn.Micro;

namespace MarkPad.About
{
    public class AboutViewModel : Screen
    {
        public string Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string Configuration
        {
            get
            {
                var attr = Assembly.GetExecutingAssembly()
                    .GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false)
                    .OfType<AssemblyConfigurationAttribute>()
                    .FirstOrDefault() ?? new AssemblyConfigurationAttribute("");

                return attr.Configuration;
            }
        }

        private readonly List<string> authors = new List<string>
                                                    {
                                                         "Cameron MacFarland",
                                                         "Mike Minutillo",
                                                         "Jake Ginnivan",
                                                         "Ian Randall",
                                                         "Paul Stovell",
                                                         "Andrew Tobin",
                                                         "Brendan Forster",
                                                         "Paul Jenkins",
                                                         "Drew Marsh",
                                                         "Brandon Montgomery",
                                                         "Ben Scott",
                                                         "Scott Hanselman"
                                                     };
        public string Authors
        {
            get { return string.Join(", ", authors); }
        }

        private readonly List<string> components = new List<string>
                                                       {
                                                         "Autofac",
                                                         "Awesomium",
                                                         "Caliburn Micro",
                                                         "AvalonEdit",
                                                         "MahApps.Metro",
                                                         "Ookii Dialogs",
                                                         "XML-RPC.NET",
                                                         "MarkdownDeep",
                                                         "Notify Property Weaver"
                                                     };
        public string Components
        {
            get { return string.Join(", ", components); }
        }

        public void Visit()
        {
            try
            {
                System.Diagnostics.Process.Start("http://code52.org/DownmarkerWPF/");
            }
            catch //I forget what exceptions can be raised if the browser is crashed?
            {

            }
        }
    }
}
