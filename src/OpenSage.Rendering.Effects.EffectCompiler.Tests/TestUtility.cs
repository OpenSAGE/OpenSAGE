using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSage.Rendering.Effects.EffectCompiler.Tests;

public static class TestUtility
{
    public static readonly string FixturesRootPath = FixturesRootPath = Path.GetFullPath("Fixtures");

    public static IEnumerable<string> GetEffectFiles()
    {
        return Directory
            .EnumerateFiles(FixturesRootPath, "*.fx", SearchOption.AllDirectories)
            .Select(x => Path.GetRelativePath(FixturesRootPath, x));
    }
}
