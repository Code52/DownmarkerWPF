using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace MarkPad.Infrastructure
{
    public interface IFileSystem
    {
        string GetTempPath();
        IFile File { get; }
        IDirectory Directory { get; }
        Bitmap OpenBitmap(string fullPath);
        void SaveImagePng(Bitmap image, string imageFileName);
        IFileInfo FileInfo(string fileName);
    }

    public interface IFileInfo
    {
        bool IsReadOnly { get; set; }
        string Name { get; }
    }

    public interface IDirectory
    {
        string[] GetDirectories(string path);
        string[] GetFiles(string path);
        bool Exists(string path);
        DirectoryInfo CreateDirectory(string path);
        void Move(string sourceDirName, string destDirName);
    }

    public interface IFile
    {
        bool Exists(string path);
        void Move(string sourceFileName, string destFileName);
        void Delete(string path);
        byte[] ReadAllBytes(string path);
        Task WriteAllTextAsync(string fileName, string markdownContent);
        Task<string> ReadAllTextAsync(string path);
    }

    public class FileSystem : IFileSystem
    {
        public FileSystem()
        {
            File = new FileOperations();
            Directory = new DirectoryOperations();
        }

        public string GetTempPath()
        {
            return Path.GetTempPath();
        }

        public IFile File { get; private set; }
        public IDirectory Directory { get; private set; }

        public StreamWriter NewStreamWriter(string path)
        {
            return new StreamWriter(path);
        }

        public Bitmap OpenBitmap(string fullPath)
        {
            return (Bitmap)Image.FromFile(fullPath);
        }

        public void SaveImagePng(Bitmap image, string imageFileName)
        {
            using (var stream = new FileStream(imageFileName, FileMode.Create))
            {
                image.Save(stream, ImageFormat.Png);
            }
        }

        public IFileInfo FileInfo(string fileName)
        {
            return new FileInfoWrapper(new FileInfo(fileName));
        }
    }

    public class FileInfoWrapper : IFileInfo
    {
        readonly FileInfo fileInfo;

        public FileInfoWrapper(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
        }

        public bool IsReadOnly
        {
            get { return fileInfo.IsReadOnly; }
            set { fileInfo.IsReadOnly = value; }
        }

        public string Name { get { return fileInfo.Name; } }
    }

    public class DirectoryOperations : IDirectory
    {
        public string[] GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }

        public string[] GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }

        public DirectoryInfo CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }

        public void Move(string sourceDirName, string destDirName)
        {
            Directory.Move(sourceDirName, destDirName);
        }
    }

    public class FileOperations : IFile
    {
        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public void Move(string sourceFileName, string destFileName)
        {
            File.Move(sourceFileName, destFileName);
        }

        public void Delete(string path)
        {
            File.Delete(path);
        }

        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public async Task WriteAllTextAsync(string fileName, string markdownContent)
        {
            using (var streamWriter = new StreamWriter(fileName))
                await streamWriter.WriteAsync(markdownContent);
        }

        public Task<string> ReadAllTextAsync(string path)
        {
            using (var streamWriter = new StreamReader(path))
            {
                return streamWriter.ReadToEndAsync();
            }
        }
    }
}