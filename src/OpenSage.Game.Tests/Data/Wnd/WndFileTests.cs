using OpenSage.Data.Wnd;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Tests.Data.Wnd
{
    public class WndFileTests
    {
        private readonly ITestOutputHelper _output;

        public WndFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanReadWndFiles()
        {
            InstalledFilesTestData.ReadFiles(".wnd", _output, entry =>
            {
                var wndFile = WndFile.FromFileSystemEntry(entry, null); // TODO

                Assert.NotNull(wndFile);
            });
        }
    }
}
