using System;
using System.ServiceModel;

namespace MarkPad
{
    public class OpenFileListener : IDisposable
    {
        private ServiceHost host;

        public OpenFileListener()
        {
        }

        public void Start()
        {
            host = new ServiceHost(typeof(OpenFileAction), new[] { new Uri("net.pipe://localhost") });
            host.AddServiceEndpoint(typeof(IOpenFileAction), new NetNamedPipeBinding(), "OpenFileAction");
            host.Open();
        }

        public void Dispose()
        {
            host.Close();
        }
    }

    [ServiceContract]
    public interface IOpenFileAction
    {
        [OperationContract]
        void OpenFile(string path);
    }

    public class OpenFileAction : IOpenFileAction
    {
        public void OpenFile(string path)
        {
            
        }
    }

}
