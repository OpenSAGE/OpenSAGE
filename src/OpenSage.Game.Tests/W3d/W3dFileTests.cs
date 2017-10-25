using System.IO;
using OpenSage.Data.Big;
using OpenSage.Data.W3d;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Data.Tests.W3d
{
    public class W3dFileTests
    {
        private readonly ITestOutputHelper _output;

        public W3dFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanReadW3dFiles()
        {
            InstalledFilesTestData.ReadFiles(".w3d", _output, entry =>
            {
                switch (Path.GetFileName(entry.FilePath))
                {
                    case "UISabotr_idel.w3d":
                    case "UISabotr_Jump.w3d":
                    case "UISabotr_Left.w3d":
                    case "UISabotr_Right.w3d":
                    case "UISabotr_Up.w3d":
                    case "cusheep_grza.w3d":
                    case "gbmtwalld.w3d":
                    case "gbmtwalldramp.w3d":
                    case "gbmtwalle.w3d":
                    case "bbbags.w3d":
                    case "cuwyrm_cld_skl.w3d":
                    case "cuwyrm_cld_skn.w3d":
                    case "gugandalfcrstl.w3d":
                    case "guhbtshfb_cinb.w3d":
                    case "guhbtshfb_cinc.w3d":
                    case "lwbanhfllbst.w3d":
                    case "lwbanhnazgul.w3d":
                    case "lwbanhwtchkng.w3d":
                    case "psupplies04.w3d":
                    case "wbcave_d2a.w3d":
                    case "wbcave_d2c.w3d":
                        return; // Corrupt, or unreferenced and contain chunks that don't exist elsewhere.
                }

                var w3dFile = W3dFile.FromFileSystemEntry(entry);

                foreach (var mesh in w3dFile.Meshes)
                {
                    Assert.Equal((int) mesh.Header.NumVertices, mesh.Vertices.Length);

                    Assert.Equal((int) mesh.Header.NumTris, mesh.Triangles.Length);

                    Assert.Equal(mesh.Vertices.Length, mesh.Influences.Length);

                    Assert.Equal((int) mesh.MaterialInfo.PassCount, mesh.MaterialPasses.Length);

                    Assert.Equal((int) mesh.MaterialInfo.ShaderCount, mesh.Shaders.Length);

                    Assert.Equal(mesh.Vertices.Length, mesh.ShadeIndices.Length);

                    Assert.True(mesh.Materials.Length <= 16);

                    foreach (var material in mesh.Materials)
                    {
                        Assert.Equal(W3dVertexMaterialFlags.None, material.VertexMaterialInfo.Attributes);

                        Assert.Equal(0, material.VertexMaterialInfo.Translucency);
                    }

                    Assert.True(mesh.MaterialPasses.Length <= 3);

                    foreach (var materialPass in mesh.MaterialPasses)
                    {
                        Assert.True(materialPass.Dcg == null || materialPass.Dcg.Length == mesh.Vertices.Length);
                        Assert.Null(materialPass.Dig);
                        Assert.Null(materialPass.Scg);

                        Assert.True(materialPass.TextureStages.Count <= 2);

                        foreach (var textureStage in materialPass.TextureStages)
                        {
                            Assert.True(textureStage.TexCoords == null || textureStage.TexCoords.Length == mesh.Header.NumVertices);

                            Assert.Null(textureStage.PerFaceTexCoordIds);

                            var numTextureIds = textureStage.TextureIds.Length;
                            Assert.True(numTextureIds == 1 || numTextureIds == mesh.Header.NumTris);
                        }

                        Assert.True((materialPass.ShaderIds != null && materialPass.VertexMaterialIds != null && materialPass.TexCoords == null) || materialPass.ShaderMaterialId != null);

                        if (materialPass.ShaderIds != null)
                        {
                            var numShaderIds = materialPass.ShaderIds.Length;
                            Assert.True(numShaderIds == 1 || numShaderIds == mesh.Header.NumTris);
                        }

                        if (materialPass.VertexMaterialIds != null)
                        {
                            var numVertexMaterialIds = materialPass.VertexMaterialIds.Length;
                            Assert.True(numVertexMaterialIds == 1 || numVertexMaterialIds == mesh.Header.NumVertices);
                        }
                    }

                    Assert.True(mesh.Textures.Count <= 29);
                }
            });
        }

        [Fact]
        public void LoadW3dFromBigFile()
        {
            var bigFilePath = Path.Combine(InstalledFilesTestData.GetInstallationDirectory(SageGame.CncGeneralsZeroHour), "W3DZH.big");
        
            using (var bigStream = File.OpenRead(bigFilePath))
            using (var bigArchive = new BigArchive(bigStream))
            {
                var entry = bigArchive.GetEntry(@"Art\W3D\ABBarracks_AC.W3D");

                var w3dFile = W3dFile.FromFileSystemEntry(new FileSystemEntry(null, entry.FullName, entry.Length, entry.Open));
                Assert.Equal(3, w3dFile.Meshes.Count);
            }
        }
    }
}
