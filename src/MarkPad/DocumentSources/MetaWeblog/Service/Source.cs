using CookComputing.XmlRpc;

namespace MarkPad.DocumentSources.MetaWeblog.Service
{
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct Source
    {
        public string name;
        public string url;
    }
}
