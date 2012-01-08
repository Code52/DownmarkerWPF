using System;
using System.ComponentModel;
using Caliburn.Micro;
using MarkPad.Metaweblog;

namespace MarkPad.Settings
{
    [Serializable]
    public class BlogSetting : PropertyChangedBase, IEditableObject
    {
        public string BlogName { get; set; }

        public string WebAPI { get; set; }

        public BlogInfo BlogInfo { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

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
    }
}