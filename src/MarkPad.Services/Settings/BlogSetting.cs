using System.ComponentModel;
using System.Runtime.Serialization;

namespace MarkPad.Services.Settings
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
                if (string.IsNullOrWhiteSpace(this.WebAPI) ||
                    string.IsNullOrWhiteSpace(this.Username) ||
                    string.IsNullOrWhiteSpace(this.Password))
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
                if (string.IsNullOrWhiteSpace(this.WebAPI) ||
                    string.IsNullOrWhiteSpace(this.Username) ||
                    string.IsNullOrWhiteSpace(this.Password) ||
                    string.IsNullOrWhiteSpace(this.BlogName) ||
                    string.IsNullOrWhiteSpace(this.Language) ||
                    string.IsNullOrWhiteSpace(this.BlogInfo.blogid))
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
