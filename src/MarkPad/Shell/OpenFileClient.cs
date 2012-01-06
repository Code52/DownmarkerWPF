using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using MarkPad.Services.Services;

namespace MarkPad.Shell
{
    /// <summary>
    /// Proxy class for sending the "Open File" command to another process
    /// </summary>
    public class OpenFileClient
    {
        public void SendMessage(IList<string> args)
        {
            if (args.Count != 1)
                return;

            var path = args[0];
            if (!File.Exists(path))
                return;

            var pipeFactory = new ChannelFactory<IOpenFileCommand>(
                new NetNamedPipeBinding(),
                new EndpointAddress("net.pipe://localhost/OpenFileCommand"));

            try
            {
                var httpProxy = pipeFactory.CreateChannel();
                httpProxy.OpenFile(path);
            }
            catch (Exception)
            {
                // TODO: log exception
            }
            finally
            {
                pipeFactory.Close();
            }

        }
    }
}
