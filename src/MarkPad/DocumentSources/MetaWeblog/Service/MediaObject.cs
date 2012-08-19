using CookComputing.XmlRpc;

namespace MarkPad.DocumentSources.MetaWeblog.Service
{
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct MediaObject
    {
        public string name;
        public string type;
        public byte[] bits;
    }
}
