using System.IO;
using LLGfx;
using OpenSage.Data;
using OpenSage.Data.W3d;
using OpenSage.Graphics;

namespace OpenSage.Content
{
    internal sealed class ModelLoader : ContentLoader<Model>
    {
        protected override Model LoadEntry(FileSystemEntry entry, ContentManager contentManager, ResourceUploadBatch uploadBatch)
        {
            var w3dFile = W3dFile.FromFileSystemEntry(entry);

            var w3dHierarchy = w3dFile.Hierarchy;
            if (w3dFile.HLod != null && w3dHierarchy == null)
            {
                // Load referenced hierarchy.
                var hierarchyFileName = w3dFile.HLod.Header.HierarchyName + ".W3D";
                var hierarchyFilePath = Path.Combine(Path.GetDirectoryName(w3dFile.FilePath), hierarchyFileName);
                var hierarchyFileEntry = contentManager.FileSystem.GetFile(hierarchyFilePath);
                var hierarchyFile = W3dFile.FromFileSystemEntry(hierarchyFileEntry);
                w3dHierarchy = hierarchyFile.Hierarchy;
            }

            return new Model(
                contentManager,
                uploadBatch,
                w3dFile,
                w3dHierarchy);
        }
    }
}
