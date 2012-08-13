using System;
using System.Net;
using CookComputing.XmlRpc;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.Infrastructure.DialogService;
using MarkPad.Settings.Models;

namespace MarkPad.Document
{
    public class PublishService : IPublishService
    {
        readonly IDialogService dialogService;
        readonly Func<string, IMetaWeblogService> getMetaWeblog;
 
        public PublishService(IDialogService dialogService, Func<string, IMetaWeblogService> getMetaWeblog)
        {
            this.dialogService = dialogService;
            this.getMetaWeblog = getMetaWeblog;
        }

        public Post? Publish(string postid, string postTitle, string displayname, string[] categories, BlogSetting blog, string content)
        {
            if (categories == null) categories = new string[0];

            var proxy = getMetaWeblog(blog.WebAPI);

            var newpost = new Post();
            try
            {
                if (string.IsNullOrWhiteSpace(postid))
                {
                    var permalink = displayname.Split('.')[0] == "New Document"
                                        ? postTitle
                                        : displayname.Split('.')[0];

                    newpost = new Post
                    {
                        permalink = permalink,
                        title = postTitle,
                        dateCreated = DateTime.Now,
                        description = blog.Language == "HTML" ? DocumentParser.GetBodyContents(content) : content,
                        categories = categories,
                        format = blog.Language
                    };
                    newpost.postid = proxy.NewPost(blog, newpost, true);
                }
                else
                {
                    newpost = proxy.GetPost(postid, blog);
                    newpost.title = postTitle;
                    newpost.description = blog.Language == "HTML" ? DocumentParser.GetBodyContents(content) : content;
                    newpost.categories = categories;
                    newpost.format = blog.Language;

                    proxy.EditPost(postid, blog, newpost, true);
                }
            }
            catch (WebException ex)
            {
                dialogService.ShowError("Error Publishing", ex.Message, "");
                return null;
            }
            catch (XmlRpcException ex)
            {
                dialogService.ShowError("Error Publishing", ex.Message, "");
                return null;
            }
            catch (XmlRpcFaultException ex)
            {
                dialogService.ShowError("Error Publishing", ex.Message, "");
                return null;
            }

            return newpost;
        }
    }
}