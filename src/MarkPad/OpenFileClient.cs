using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;

namespace MarkPad
{
    public class OpenFileClient
    {
        public void SendMessage(IList<string> args)
        {
            if (args.Count != 1)
                return;

            var path = args[0];
            if (!File.Exists(path))
                return;

            var pipeFactory = new ChannelFactory<IOpenFileAction>(
                new NetNamedPipeBinding(),
                new EndpointAddress("net.pipe://localhost/OpenFileAction"));

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
