using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CookComputing.XmlRpc;
using MarkPad.Document;
using MarkPad.DocumentSources.GitHub;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.Infrastructure.DialogService;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources.WebSources
{
    public class WebDocumentService : IWebDocumentService
    {
        readonly Func<string, IMetaWeblogService> getMetaWeblog;
        readonly IGithubApi githubApi;
        readonly IDialogService dialogService;

        public WebDocumentService(
            IGithubApi githubApi, 
            Func<string, IMetaWeblogService> getMetaWeblog, 
            IDialogService dialogService)
        {
            this.githubApi = githubApi;
            this.getMetaWeblog = getMetaWeblog;
            this.dialogService = dialogService;
        }

        public async Task DeleteDocument(BlogSetting blog, Post post)
        {
            if (blog.WebSourceType == WebSourceType.MetaWebLog)
            {
                await getMetaWeblog(blog.WebAPI).DeletePostAsync((string)post.postid, blog);
                return;
            }
            if (blog.WebSourceType == WebSourceType.GitHub)
            {
                return;
            }

            throw new ArgumentException(string.Format("Unsupported WebSourceType ({0})", blog.WebSourceType));
        }

        public Task<string> SaveDocument(BlogSetting blog, WebDocument document)
        {
            if (blog.WebSourceType == WebSourceType.MetaWebLog)
            {
                return TaskEx.Run(() =>
                {
                    var categories = document.Categories.ToArray();
                    return CreateOrUpdateMetaWebLogPost(document.Id, document.Title, categories, document.MarkdownContent,
                                                 document.ImagesToSaveOnPublish, blog);
                });
            }
            if (blog.WebSourceType == WebSourceType.GitHub)
            {
                return CreateOrUpdateGithubPost(document.Title, document.MarkdownContent, document.ImagesToSaveOnPublish, blog);   
            }

            return TaskEx.Run(new Func<string>(() =>
            {
                throw BadWebSourceTypeException(blog);
            }));
        }

        static ArgumentException BadWebSourceTypeException(BlogSetting blog)
        {
            return new ArgumentException(string.Format("WebSource Type is invalid ({0})", blog.WebSourceType));
        }

        public async Task<string> GetDocumentContent(BlogSetting blog, string id)
        {
            if (blog.WebSourceType == WebSourceType.MetaWebLog)
            {
                var post = await getMetaWeblog(blog.WebAPI).GetPostAsync(id, blog);
                return post.description;
            }
            if (blog.WebSourceType == WebSourceType.GitHub)
            {
                return await githubApi.FetchFileContents(blog.Token, blog.Username, blog.WebAPI, id);
            }

            throw BadWebSourceTypeException(blog);            
        }

        async Task<string> CreateOrUpdateGithubPost(string postTitle, string content, 
            ICollection<string> imagesToUpload, BlogSetting blog)
        {
            var treeToUpload = new GitTree();
            if (imagesToUpload.Count > 0)
            {
                foreach (var imageToUpload in imagesToUpload)
                {
                    var imageContent = Convert.ToBase64String(File.ReadAllBytes(imageToUpload));
                    var item = new GitFile
                    {
                        type = "tree",
                        path = imageToUpload,
                        mode = ((int)GitTreeMode.SubDirectory),
                        content = imageContent
                    };
                    treeToUpload.tree.Add(item);
                }
            }

            var gitFile = new GitFile
            {
                path = postTitle,
                content = content,
                mode = (int)GitTreeMode.File,
                type = "blob"
            };
            treeToUpload.tree.Add(gitFile);

            var newTree = await githubApi.NewTree(blog.Token, blog.Username, blog.WebAPI, treeToUpload);
            var uploadedFile = newTree.tree.Single(t => t.sha == gitFile.sha);
            return uploadedFile.sha;
        }

        string CreateOrUpdateMetaWebLogPost(
            string postid, string postTitle,
            string[] categories, string content,
            ICollection<string> imagesToUpload,
            BlogSetting blog)
        {
            var proxy = getMetaWeblog(blog.WebAPI);

            if (imagesToUpload.Count > 0)
            {
                foreach (var imageToUpload in imagesToUpload)
                {
                    var response = proxy.NewMediaObject(blog, new MediaObject
                    {
                        name = imageToUpload,
                        type = "image/png",
                        bits = File.ReadAllBytes(imageToUpload)
                    });

                    content = content.Replace(imageToUpload, response.url);
                }
            }

            var newpost = new Post();
            try
            {
                if (string.IsNullOrWhiteSpace(postid))
                {
                    var permalink = postTitle;

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
            }
            catch (XmlRpcException ex)
            {
                dialogService.ShowError("Error Publishing", ex.Message, "");
            }
            catch (XmlRpcFaultException ex)
            {
                dialogService.ShowError("Error Publishing", ex.Message, "");
            }

            return (string)newpost.postid;
        }

        public static string GetSha1(string value)
        {
            var data = Encoding.ASCII.GetBytes(value);
            var hashData = new SHA1Managed().ComputeHash(data);

            return hashData.Aggregate(string.Empty, (current, b) => current + b.ToString("X2"));
        }
    }
}