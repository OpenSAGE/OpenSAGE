using System.IO;
using OpenSage.FileFormats.Big;
using OpenSage.FileFormats.W3d;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Tests.Data.W3d
{
    public class W3dFileTests
    {
        private readonly ITestOutputHelper _output;

        public W3dFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanRoundtripW3dFiles()
        {
            InstalledFilesTestData.ReadFiles(".w3d", _output, entry =>
            {
                switch (Path.GetFileName(entry.FilePath).ToLower())
                {
                    case "uisabotr_idel.w3d":
                    case "uisabotr_jump.w3d":
                    case "uisabotr_left.w3d":
                    case "uisabotr_right.w3d":
                    case "uisabotr_up.w3d":
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
                    case "kbpostgaten_al.w3d":
                    case "kbpostgaten_am.w3d":
                    case "lwbanhfllbst.w3d":
                    case "lwbanhnazgul.w3d":
                    case "lwbanhwtchkng.w3d":
                    case "psupplies04.w3d":
                    case "readonly-0-rev-2-lwbanhwitchk.w3d":
                    case "wbcave_d2a.w3d":
                    case "wbcave_d2c.w3d":
                    case "npc14b.w3d":
                    case "npc15b.w3d":
                        return; // Corrupt, or unreferenced and contain chunks that don't exist elsewhere.
                }

                var w3dFile = TestUtility.DoRoundtripTest(
                    () => entry.Open(),
                    stream => W3dFile.FromStream(stream, entry.FilePath),
                    (w3d, stream) => w3d.WriteTo(stream),
                    true);

                foreach (var renderableObject in w3dFile.RenderableObjects)
                {
                    if (!(renderableObject is W3dMesh mesh))
                    {
                        continue;
                    }

                    Assert.Equal((int) mesh.Header.NumVertices, mesh.Vertices.Items.Length);

                    Assert.Equal((int) mesh.Header.NumTris, mesh.Triangles.Items.Length);

                    if (mesh.Influences != null)
                    {
                        Assert.Equal(mesh.Vertices.Items.Length, mesh.Influences.Items.Length);
                    }

                    Assert.Equal((int) mesh.MaterialInfo.PassCount, mesh.MaterialPasses.Count);

                    Assert.Equal((int) mesh.MaterialInfo.ShaderCount, mesh.Shaders?.Items.Count ?? 0);

                    Assert.Equal(mesh.Vertices.Items.Length, mesh.ShadeIndices.Items.Length);

                    if (mesh.VertexMaterials != null)
                    {
                        Assert.True(mesh.VertexMaterials.Items.Count <= 16);

                        foreach (var material in mesh.VertexMaterials.Items)
                        {
                            Assert.Equal(W3dVertexMaterialFlags.None, material.Info.Attributes);

                            Assert.Equal(0, material.Info.Translucency);
                        }
                    }

                    Assert.True(mesh.MaterialPasses.Count <= 3);

                    Assert.True(mesh.ShaderMaterials == null || mesh.ShaderMaterials.Items.Count == 1);

                    if (mesh.ShaderMaterials != null)
                    {
                        Assert.Null(mesh.VertexMaterials);
                        Assert.Single(mesh.MaterialPasses);
                    }

                    foreach (var materialPass in mesh.MaterialPasses)
                    {
                        Assert.True(materialPass.Dcg == null || materialPass.Dcg.Items.Length == mesh.Vertices.Items.Length);
                        Assert.Null(materialPass.Dig);
                        Assert.Null(materialPass.Scg);

                        Assert.True(materialPass.TextureStages.Count <= 2);

                        foreach (var textureStage in materialPass.TextureStages)
                        {
                            Assert.True(textureStage.TexCoords == null || textureStage.TexCoords.Items.Length == mesh.Header.NumVertices);

                            Assert.Null(textureStage.PerFaceTexCoordIds);

                            var numTextureIds = textureStage.TextureIds.Items.Count;
                            Assert.True(numTextureIds == 1 || numTextureIds == mesh.Header.NumTris);
                        }

                        Assert.True((materialPass.ShaderIds != null && materialPass.VertexMaterialIds != null && materialPass.TexCoords == null) || materialPass.ShaderMaterialIds != null);

                        if (materialPass.ShaderIds != null)
                        {
                            var numShaderIds = materialPass.ShaderIds.Items.Length;
                            Assert.True(numShaderIds == 1 || numShaderIds == mesh.Header.NumTris);
                        }

                        if (materialPass.VertexMaterialIds != null)
                        {
                            var numVertexMaterialIds = materialPass.VertexMaterialIds.Items.Length;
                            Assert.True(numVertexMaterialIds == 1 || numVertexMaterialIds == mesh.Header.NumVertices);
                        }

                        Assert.True(materialPass.ShaderMaterialIds == null || materialPass.ShaderMaterialIds.Items[0] == 0);
                    }

                    if (mesh.Textures != null)
                    {
                        Assert.True(mesh.Textures.Items.Count <= 29);
                    }
                }

                foreach (var animation in w3dFile.GetCompressedAnimations())
                {
                    foreach (var channel in animation.TimeCodedChannels)
                    {
                        switch (channel.ChannelType)
                        {
                            case W3dAnimationChannelType.UnknownBfme:
                                Assert.Equal(1, channel.VectorLength);
                                break;
                        }
                    }
                }
            });
        }

        [GameFact(SageGame.CncGeneralsZeroHour)]
        public void LoadW3dFromBigFile()
        {
            var bigFilePath = Path.Combine(InstalledFilesTestData.GetInstallationDirectory(SageGame.CncGeneralsZeroHour), "W3DZH.big");
        
            using (var bigArchive = new BigArchive(bigFilePath))
            {
                var entry = bigArchive.GetEntry(@"Art\W3D\ABBarracks_AC.W3D");

                W3dFile w3dFile;
                using (var entryStream = entry.Open())
                {
                    w3dFile = W3dFile.FromStream(entryStream, entry.FullName);
                }

                Assert.Equal(3, w3dFile.RenderableObjects.Count);
            }
        }
    }
}
