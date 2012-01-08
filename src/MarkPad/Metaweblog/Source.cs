using CookComputing.XmlRpc;

namespace MarkPad.Metaweblog
{
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct Source
    {
        public string name;
        public string url;
    }
}