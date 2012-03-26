using MarkPad.OpenFromWeb;
using MarkPad.Services.Interfaces;
using NSubstitute;
using Xunit;

namespace MarkPad.Tests.OpenFromWeb
{
    public class OpenFromWebViewModelTests
    {
        OpenFromWebViewModel subject;
        
        readonly IMetaWeblogService metaWeblogService = Substitute.For<IMetaWeblogService>();
        readonly IDialogService dialogService = Substitute.For<IDialogService>();

        public OpenFromWebViewModelTests()
        {
            subject = new OpenFromWebViewModel(dialogService, s => metaWeblogService);
        }

        [Fact]
        public void Monkey()
        {
            Assert.NotNull(subject);
        }
    }
}
