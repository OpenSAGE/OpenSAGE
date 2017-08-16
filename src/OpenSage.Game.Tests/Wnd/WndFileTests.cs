using OpenSage.Data.Wnd;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Data.Tests.Wnd
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
            InstalledFilesTestData.ReadFiles(".wnd", _output, (fileName, openFileStream) =>
            {
                WndFile wndFile;
                using (var fileStream = openFileStream())
                    wndFile = WndFile.FromStream(fileStream);

                Assert.NotNull(wndFile);
            });
        }
    }
}
