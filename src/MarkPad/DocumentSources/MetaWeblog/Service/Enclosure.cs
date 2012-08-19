using CookComputing.XmlRpc;

namespace MarkPad.DocumentSources.MetaWeblog.Service
{
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct Enclosure
    {
        public int length;
        public string type;
        public string url;
    }
}
