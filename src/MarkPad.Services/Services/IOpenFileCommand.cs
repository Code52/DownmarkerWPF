using System.ServiceModel;

namespace MarkPad.Services.Services
{
    [ServiceContract]
    public interface IOpenFileCommand
    {
        [OperationContract]
        void OpenFile(string path);
    }
}