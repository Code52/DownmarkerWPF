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
using MarkPad.Plugins;
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

        public Task<SaveResult> SaveDocument(BlogSetting blog, WebDocument document)
        {
            if (blog.WebSourceType == WebSourceType.MetaWebLog)
            {
                return TaskEx.Run(() =>
                {
                    var categories = document.Categories.ToArray();
                    return CreateOrUpdateMetaWebLogPost(document, categories, blog);
                });
            }
            if (blog.WebSourceType == WebSourceType.GitHub)
            {
                return CreateOrUpdateGithubPost(document.Title, document.MarkdownContent, document.AssociatedFiles, blog);   
            }

            return TaskEx.Run(new Func<SaveResult>(() =>
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

        async Task<SaveResult> CreateOrUpdateGithubPost(string postTitle, string content, IEnumerable<FileReference> referencedFiles, BlogSetting blog)
        {
            var treeToUpload = new GitTree();
            var imagesToUpload = referencedFiles.Where(f=>!f.Saved).ToList();
            if (imagesToUpload.Count > 0)
            {
                foreach (var imageToUpload in imagesToUpload)
                {
                    var imageContent = Convert.ToBase64String(File.ReadAllBytes(imageToUpload.FullPath));
                    var item = new GitFile
                    {
                        type = "tree",
                        path = imageToUpload.FullPath,
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

            var newTree = await githubApi.NewTree(blog.Token, blog.Username, blog.WebAPI, blog.BlogInfo.blogid, treeToUpload);
            var uploadedFile = newTree.Item1.tree.Single(t => t.path == gitFile.path);
            foreach (var fileReference in imagesToUpload)
            {
                fileReference.Saved = true;
            }

            return new SaveResult
                   {
                       Id = uploadedFile.sha,
                       NewDocumentContent = content
                   };
        }

        SaveResult CreateOrUpdateMetaWebLogPost(WebDocument document, string[] categories, BlogSetting blog)
        {
            var newContent = document.MarkdownContent;
            var proxy = getMetaWeblog(blog.WebAPI);

            if (document.AssociatedFiles.Count(f=>!f.Saved) > 0)
            {
                foreach (var imageToUpload in document.AssociatedFiles.Where(f=>!f.Saved))
                {
                    var response = proxy.NewMediaObject(blog, new MediaObject
                    {
                        name = imageToUpload.FullPath,
                        type = "image/png",
                        bits = File.ReadAllBytes(imageToUpload.FullPath)
                    });

                    newContent = newContent.Replace(imageToUpload.RelativePath, response.url);
                    imageToUpload.Saved = true;
                }
            }

            var newpost = new Post();
            try
            {
                if (string.IsNullOrWhiteSpace(document.Id))
                {
                    var permalink = document.Title;

                    newpost = new Post
                    {
                        permalink = permalink,
                        title = document.Title,
                        dateCreated = DateTime.Now,
                        description = blog.Language == "HTML" ? DocumentParser.GetBodyContents(newContent) : newContent,
                        categories = categories,
                        format = blog.Language
                    };
                    newpost.postid = proxy.NewPost(blog, newpost, true);
                }
                else
                {
                    newpost = proxy.GetPost(document.Id, blog);
                    newpost.title = document.Title;
                    newpost.description = blog.Language == "HTML" ? DocumentParser.GetBodyContents(newContent) : newContent;
                    newpost.categories = categories;
                    newpost.format = blog.Language;

                    proxy.EditPost(document.Id, blog, newpost, true);
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

            return new SaveResult
                   {
                       Id = newpost.postid.ToString(),
                       NewDocumentContent = newContent
                   };
        }

        public static string GetSha1(string value)
        {
            var data = Encoding.ASCII.GetBytes(value);
            var hashData = new SHA1Managed().ComputeHash(data);

            return hashData.Aggregate(string.Empty, (current, b) => current + b.ToString("X2"));
        }
    }
}