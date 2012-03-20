using System.Runtime.Serialization;

namespace MarkPad.Services.Settings
{
    [DataContract]
    public struct BlogInfo
    {
        [DataMember]
        public string blogid;
        [DataMember]
        public string url;
        [DataMember]
        public string blogName;
    }
}
