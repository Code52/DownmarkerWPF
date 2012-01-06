using System.ServiceModel;

namespace MarkPad.Services.Services
{
    [ServiceContract]
    public interface IOpenFileAction
    {
        [OperationContract]
        void OpenFile(string path);
    }
}