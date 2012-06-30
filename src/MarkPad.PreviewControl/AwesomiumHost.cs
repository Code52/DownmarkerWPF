using System;
using System.Security.Permissions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Awesomium.Windows.Controls;

namespace MarkPad.PreviewControl
{
    public class AwesomiumHost : MarshalByRefObject, IDisposable
    {
        WebControl wb;
        long? windowHandle;
        readonly Application app;
        readonly UserControl control;
        readonly ManualResetEvent loadedWaitHandle;
        readonly string baseDirectory;
        static bool awesomiumInitialised;

        public AwesomiumHost(string filename, string baseDirectory)
        {
            if (Application.Current == null)
                app = new Application();
            FileName = filename;
            this.baseDirectory = baseDirectory;

            loadedWaitHandle = new ManualResetEvent(false);

            // We need a hosting user control because awesomium has issues rendering if we create it before
            // dispatcher is running
            control = new UserControl();
            control.Loaded += ControlLoaded;
        }

        public string FileName { get; private set; }
        public string Html { get; set; }

        void ControlLoaded(object sender, RoutedEventArgs e)
        {
            control.Loaded -= ControlLoaded;

            wb = new WebControl
            {
                UseLayoutRounding = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            wb.Loaded += WbLoaded;
            AwesomiumResourceHandler.Host = this;
            wb.OpenExternalLink += AwesomiumResourceHandler.WebControlLinkClicked;
            wb.ResourceRequest += AwesomiumResourceHandler.WebControlResourceRequest;
            wb.LoadHTML(Html);

            control.Content = wb;
        }

        void WbLoaded(object sender, RoutedEventArgs e)
        {
            WbProcentualZoom();
            LoadedWaitHandle.Set();
        }

        public double ScrollPercentage { get; set; }

        public IntPtr ControlHandle
        {
            get
            {
                if (windowHandle == null)
                {
                    windowHandle = CreateWindowHandle(control);
                }
                return new IntPtr(windowHandle.Value);
            }
        }

        public ManualResetEvent LoadedWaitHandle
        {
            get { return loadedWaitHandle; }
        }

        /// <summary>
        /// Convert the framework element to a Window Handle so it can be serialized.
        /// </summary>
        /// <param name="frameworkElement">The framework element to convert to a Window Handle.</param>
        /// <returns>The Window Handle that is hosting the framework element.</returns>
        static long CreateWindowHandle(Visual frameworkElement)
        {
            // ReSharper disable InconsistentNaming
            const int WS_VISIBLE = 0x10000000;
            // ReSharper restore InconsistentNaming

            var parameters = new HwndSourceParameters(String.Format("NewWindowHost{0}", Guid.NewGuid()), 1, 1);
            parameters.WindowStyle &= ~WS_VISIBLE;

            var intPtr = new HwndSource(parameters)
                {
                    RootVisual = frameworkElement
                }.Handle;
            return intPtr.ToInt64();
        }

        public void SetZoom(int getZoomLevel)
        {
            app.Dispatcher.BeginInvoke(new Action(() =>
                                                      {
                                                          if (wb == null) return;
                                                          wb.Zoom = getZoomLevel;
                                                      }));
        }

        public void SetHtml(string content)
        {
            if (Application.Current == null) return;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!awesomiumInitialised)
                {
                    var c = new Awesomium.Core.WebCoreConfig
                    {
                        CustomCSS = @"body { font-family: Segoe UI, sans-serif; font-size:0.8em;}
                              ::-webkit-scrollbar { width: 12px; height: 12px; }
                              ::-webkit-scrollbar-track { background-color: white; }
                              ::-webkit-scrollbar-thumb { background-color: #B9B9B9; }
                              ::-webkit-scrollbar-thumb:hover { background-color: #000000; }",
                    };

                    Awesomium.Core.WebCore.Initialize(c, true);
                    Awesomium.Core.WebCore.BaseDirectory = baseDirectory;
                    awesomiumInitialised = true;
                }

                Html = content;
                if (wb == null) return;

                wb.CacheMode = new BitmapCache();
                EventHandler webControlOnLoadCompleted = null;
                webControlOnLoadCompleted = (sender, args) =>
                {
                    wb.LoadCompleted -= webControlOnLoadCompleted;
                    WbProcentualZoom();
                    wb.CacheMode = null;
                };
                wb.LoadCompleted += webControlOnLoadCompleted;
                wb.LoadHTML(content);
            }));
        }

        public void WbProcentualZoom()
        {
            if (!app.Dispatcher.CheckAccess())
            {
                app.Dispatcher.BeginInvoke(new Action(WbProcentualZoom));
                return;
            }

            if (wb == null) return;
            var javascript = string.Format("window.scrollTo(0,{0} * (document.body.scrollHeight - document.body.clientHeight));", ScrollPercentage);
            wb.ExecuteJavascript(javascript);
        }

        public void Dispose()
        {
            app.Dispatcher.Invoke(new Action(() =>
            {
                app.Shutdown();
                wb.Close();
            }));
        }

        public void Print()
        {
            app.Dispatcher.BeginInvoke(new Action(() => wb.Print()));
        }

        public void Run()
        {
            app.Run();
        }

        // http://social.msdn.microsoft.com/Forums/en-US/netfxremoting/thread/3ab17b40-546f-4373-8c08-f0f072d818c9/
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}