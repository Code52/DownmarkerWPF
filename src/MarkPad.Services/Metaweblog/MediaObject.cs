using CookComputing.XmlRpc;

namespace MarkPad.Services.Metaweblog
{
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct MediaObject
    {
        public string name;
        public string type;
        public byte[] bits;
    }
}
