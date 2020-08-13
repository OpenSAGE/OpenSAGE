﻿using OpenSage.Data.Ani;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Tests.Data.Ani
{
    public class AniFileTests
    {
        private readonly ITestOutputHelper _output;

        public AniFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanReadAniCursors()
        {
            InstalledFilesTestData.ReadFiles(".ani", _output, entry =>
            {
                switch (entry.FilePath)
                {
                    // BFME cursors - don't know how to parse them.
                    // They seem to be "raw" - no RIFF container.
                    // They don't seem to be referenced.
                    case @"data\cursors\magnify.ani":
                    case @"data\cursors\sccmagic.ani":
                        return;
                }

                var aniFile = AniFile.FromFileSystemEntry(entry);

                Assert.NotNull(aniFile);

                Assert.True(aniFile.ArtistName == null || aniFile.ArtistName == "PRobb");
            });
        }
    }
}
