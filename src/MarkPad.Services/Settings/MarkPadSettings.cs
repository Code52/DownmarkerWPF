using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using MarkPad.Services.Implementation;
using MarkPad.Services.Interfaces;

namespace MarkPad.Services.Settings
{
    public class MarkpadSettings
    {
        [DefaultValue(SpellingLanguages.Australian)]
        public SpellingLanguages Language { get; set; }

        public List<string> RecentFiles { get; set; }

        [DefaultValue(FontSizes.FontSize12)]
        public FontSizes FontSize { get; set; }

        [DefaultValue("Segoe UI")]
        public string FontFamily { get; set; }

        public List<BlogSetting> GetBlogs()
        {
            if (string.IsNullOrEmpty(BlogsJson))
                return new List<BlogSetting>();

            return (List<BlogSetting>)new DataContractJsonSerializer(typeof(List<BlogSetting>))
                                           .ReadObject(new MemoryStream(Encoding.Default.GetBytes(BlogsJson)));
        }

        public void SaveBlogs(List<BlogSetting> blogs)
        {
            var ms = new MemoryStream();
            var writer = JsonReaderWriterFactory.CreateJsonWriter(ms);
            new DataContractJsonSerializer(typeof(List<BlogSetting>)).WriteObject(ms, blogs);
            writer.Flush();
            BlogsJson = Encoding.Default.GetString(ms.ToArray());
        }

        public string BlogsJson { get; set; }
    }
}
