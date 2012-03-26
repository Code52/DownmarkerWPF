using CookComputing.XmlRpc;

namespace MarkPad.Services.Metaweblog
{
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct Enclosure
    {
        public int length;
        public string type;
        public string url;
    }
}
