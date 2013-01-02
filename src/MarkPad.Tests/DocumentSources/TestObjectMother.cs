﻿using System.Drawing;
using System.Threading.Tasks;
using MarkPad.Infrastructure;
using NSubstitute;

namespace MarkPad.Tests.DocumentSources
{
    public class TestObjectMother
    {
        public static IFileSystem GetFileSystem()
        {
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.GetTempPath().Returns(@"C:\Temp");
            fileSystem.OpenBitmap(Arg.Any<string>()).Returns(new Bitmap(1, 1));
            fileSystem.File.WriteAllTextAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(TaskEx.Run(() => { }));
            return fileSystem;
        }
    }
}