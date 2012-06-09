using System.ComponentModel;
using System.Runtime.Serialization;

namespace MarkPad.Settings.Models
{
    [DataContract]
    public class BlogSetting : INotifyPropertyChanged, IEditableObject
    {
        [DataMember]
        public string BlogName { get; set; }

        [DataMember]
        public string WebAPI { get; set; }

        [DataMember]
        public BlogInfo BlogInfo { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string Language { get; set; }

        public bool IsWebAPICompleted
        {
            get
            {
                if (string.IsNullOrWhiteSpace(WebAPI) ||
                    string.IsNullOrWhiteSpace(Username) ||
                    string.IsNullOrWhiteSpace(Password))
                {
                    return false;
                }

                return true;
            }
        }

        public bool IsBlogSettingCompleted
        {
            get
            {
                if (string.IsNullOrWhiteSpace(WebAPI) ||
                    string.IsNullOrWhiteSpace(Username) ||
                    string.IsNullOrWhiteSpace(Password) ||
                    string.IsNullOrWhiteSpace(BlogName) ||
                    string.IsNullOrWhiteSpace(Language) ||
                    string.IsNullOrWhiteSpace(BlogInfo.blogid))
                {
                    return false;
                }

                return true;
            }
        }

        private string beginblogName;
        private string beginwebApi;
        private BlogInfo beginblogInfo;
        private string beginusername;
        private string beginpassword;
        private string beginlanguage;

        public void BeginEdit()
        {
            beginblogName = BlogName;
            beginwebApi = WebAPI;
            beginblogInfo = BlogInfo;
            beginusername = Username;
            beginpassword = Password;
            beginlanguage = Language;
        }

        public void CancelEdit()
        {
            BlogName = beginblogName;
            WebAPI = beginwebApi;
            BlogInfo = beginblogInfo;
            Username = beginusername;
            Password = beginpassword;
            Language = beginlanguage;
        }

        public void EndEdit()
        {
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
    }
}
