using System.Collections.Generic;
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

        private readonly List<string> authors = new List<string>
                                                    {
                                                         "Cameron MacFarland",
                                                         "Mike Minutillo",
                                                         "Jake Ginnivan",
                                                         "Ian Randall",
                                                         "Andrew Tobin",
                                                         "Brendan Forster",
                                                         "Paul Jenkins"

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
                                                         "NLog",
                                                         "Ookii Dialogs",
                                                         "XML-RPC.NET",
                                                         "MarkdownSharp",
                                                         "JSON.NET"
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
            } catch //I forget what exceptions can be raised if the browser is crashed?
            {
                
            }
        }
    }
}
