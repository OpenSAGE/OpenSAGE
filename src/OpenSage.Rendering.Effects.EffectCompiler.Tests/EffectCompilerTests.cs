using System.IO;
using NUnit.Framework;
using Veldrid;

namespace OpenSage.Rendering.Effects.EffectCompiler.Tests;

internal class EffectCompilerTests
{
    [TestCaseSource(typeof(TestUtility), nameof(TestUtility.GetEffectFiles))]
    public void CanParseEffectFile(string filePath)
    {
        var fullFilePath = Path.Combine(TestUtility.FixturesRootPath, filePath);

        var outputPath = Path.ChangeExtension(fullFilePath, ".vfxo");

        EffectCompiler.CompileToFile(fullFilePath, GraphicsBackend.Direct3D11, outputPath);
    }
}
