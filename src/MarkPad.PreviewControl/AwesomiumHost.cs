using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Awesomium.Core;
using Awesomium.Core.Data;
using Awesomium.Windows.Controls;
using Application = System.Windows.Application;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

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
        MarkpadDataSource markpadDataSource;

        public AwesomiumHost(string filename, string baseDirectory)
        {
            if (Application.Current == null)
            {
                app = new Application();
                app.DispatcherUnhandledException += (sender, args) =>
                {
                    // If the preview dies, it is not the end of the world, 
                    // but most exceptions shouldn't actually cause the preview to stop
                    args.Handled = true;
                };
            }
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

            var session = WebCore.CreateWebSession(baseDirectory, new WebPreferences(true)
            {
                WebGL = true,
                EnableGPUAcceleration = true,
                SmoothScrolling = true,
                CustomCSS = @"body { font-family: Segoe UI, sans-serif; font-size:0.8em;}
                              ::-webkit-scrollbar { width: 12px; height: 12px; }
                              ::-webkit-scrollbar-track { background-color: white; }
                              ::-webkit-scrollbar-thumb { background-color: #B9B9B9; }
                              ::-webkit-scrollbar-thumb:hover { background-color: #000000; }"
            });
            markpadDataSource = new MarkpadDataSource();
            session.AddDataSource("markpad", markpadDataSource);
            wb = new WebControl
            {
                WebSession = session,
                UseLayoutRounding = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Source = new Uri("asset://markpad/MarkpadPreviewRender.html"),
            };
            wb.Loaded += WbLoaded;
            AwesomiumResourceHandler.Host = this;
            WebCore.ResourceInterceptor = AwesomiumResourceHandler.ResourceInterceptor;
            wb.ShowCreatedWebView += AwesomiumResourceHandler.ShowCreatedWebView;
            LoadHtml(Html);

            control.Content = wb;
        }

        void LoadHtml(string html)
        {
            markpadDataSource.Render = html;
            wb.Reload(true);
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
                if (wb == null || !wb.IsDocumentReady) return;
                wb.Zoom = getZoomLevel;
            }));
        }

        public void SetHtml(string content)
        {
            if (Application.Current == null) return;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Html = content;
                if (wb == null) return;
                LoadHtml(content);
            }));
        }

        public void WbProcentualZoom()
        {
            if (!app.Dispatcher.CheckAccess())
            {
                app.Dispatcher.BeginInvoke(new Action(WbProcentualZoom));
                return;
            }

            if (wb == null || !wb.IsDocumentReady) return;
            var javascript =
                string.Format("window.scrollTo(0,{0} * (document.body.scrollHeight - document.body.clientHeight));",
                              ScrollPercentage);
            try
            {
                wb.ExecuteJavascript(javascript);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Failed to scroll: {0}", ex));
            }
        }

        public void Dispose()
        {
            app.Dispatcher.Invoke(new Action(() =>
            {
                app.Shutdown();
                if (wb != null)
                {
                    wb.Dispose();
                }
            }));
        }

        public void Print()
        {
            app.Dispatcher.BeginInvoke(new Action(() => MessageBox.Show(
                "Printing is currently disabled due to Awesomium no longer supporting printing. We will try to restore this functionality asap")));
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

    class MarkpadDataSource : DataSource
    {
        public string Render { get; set; }

        protected override void OnRequest(DataSourceRequest request)
        {
            var encoding = new UTF8Encoding();
            var bytes = encoding.GetBytes(Render);
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);
            try
            {
                SendResponse(request, new DataSourceResponse
                {
                    MimeType = "text/html",
                    Size = (uint)bytes.Length,
                    Buffer = unmanagedPointer
                });
            }
            finally
            {
                // Call unmanaged code
                Marshal.FreeHGlobal(unmanagedPointer);
            }
        }
    }
}