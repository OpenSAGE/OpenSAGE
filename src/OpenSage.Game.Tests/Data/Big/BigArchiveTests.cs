﻿using System.IO;
using OpenSage.FileFormats.Big;
using Xunit;

namespace OpenSage.Tests.Data.Big;

public class BigArchiveTests
{
    private readonly string _bigFilePath;

    public BigArchiveTests()
    {
        _bigFilePath = Path.Combine(InstalledFilesTestData.GetInstallationDirectory(SageGame.CncGeneralsZeroHour), "W3DZH.big");
    }

    [GameFact(SageGame.CncGeneralsZeroHour)]
    public void OpenBigArchive()
    {
        using (var bigArchive = new BigArchive(_bigFilePath))
        {
            Assert.Equal(4432, bigArchive.Entries.Count);
        }
    }

    [GameFact(SageGame.CncGeneralsZeroHour)]
    public void ReadFirstEntry()
    {
        using (var bigArchive = new BigArchive(_bigFilePath))
        {
            var firstEntry = bigArchive.Entries[0];

            Assert.Equal(@"Art\W3D\ABBarracks_AC.W3D", firstEntry.FullName);
            Assert.Equal(9334u, firstEntry.Length);
        }
    }

    [GameFact(SageGame.CncGeneralsZeroHour)]
    public void ReadFirstEntryStream()
    {
        using (var bigArchive = new BigArchive(_bigFilePath))
        {
            var firstEntry = bigArchive.Entries[0];

            using (var firstEntryStream = firstEntry.Open())
            {
                Assert.Equal(9334, firstEntryStream.Length);

                var buffer = new byte[10000];
                var readBytes = firstEntryStream.Read(buffer, 0, buffer.Length);
                Assert.Equal(9334, readBytes);
            }
        }
    }

    [GameFact(SageGame.CncGeneralsZeroHour)]
    public void GetEntryByName()
    {
        using (var bigArchive = new BigArchive(_bigFilePath))
        {
            var entry = bigArchive.GetEntry(@"Art\W3D\ABBarracks_AC.W3D");

            Assert.Equal(@"Art\W3D\ABBarracks_AC.W3D", entry.FullName);
            Assert.Equal(9334u, entry.Length);
        }
    }
}
