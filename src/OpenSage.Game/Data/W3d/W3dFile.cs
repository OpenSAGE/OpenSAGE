﻿using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenSage.Data.W3d
{
    public sealed class W3dFile : W3dChunk
    {
        public string FilePath { get; private set; }

        public IReadOnlyList<W3dMesh> Meshes { get; private set; }

        public IReadOnlyList<W3dBox> Boxes { get; private set; }
        
        public W3dHierarchyDef Hierarchy { get; private set; }

        public W3dHLod HLod { get; private set; }

        public IReadOnlyList<W3dAnimation> Animations { get; private set; }

        public IReadOnlyList<W3dCompressedAnimation> CompressedAnimations { get; private set; }

        public IReadOnlyList<W3dEmitter> Emitters { get; private set; }

        public static W3dFile FromFileSystemEntry(FileSystemEntry entry)
        {
            using (var stream = entry.Open())
            {
                return FromStream(stream, entry.FilePath);
            }
        }

        public static W3dFile FromStream(Stream stream, string filePath)
        {
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                return Parse(reader, filePath);
            }
        }

        private static W3dFile Parse(BinaryReader reader, string filePath)
        {
            var meshes = new List<W3dMesh>();
            var boxes = new List<W3dBox>();
            W3dHierarchyDef hierarchy = null;
            W3dHLod hlod = null;
            var animations = new List<W3dAnimation>();
            var compressedAnimations = new List<W3dCompressedAnimation>();
            var emitters = new List<W3dEmitter>();

            W3dFile result;
            if (reader.BaseStream.Length > 0)
            {
               result = ParseChunk<W3dFile>(reader, (uint)reader.BaseStream.Length, (x, header) =>
               {
                   switch (header.ChunkType)
                   {
                       case W3dChunkType.W3D_CHUNK_MESH:
                           meshes.Add(W3dMesh.Parse(reader, header.ChunkSize));
                           break;

                       case W3dChunkType.W3D_CHUNK_BOX:
                           boxes.Add(W3dBox.Parse(reader));
                           break;

                       case W3dChunkType.W3D_CHUNK_HIERARCHY:
                           if (hierarchy != null)
                           {
                               throw new InvalidDataException();
                           }
                           hierarchy = W3dHierarchyDef.Parse(reader, header.ChunkSize);
                           break;

                       case W3dChunkType.W3D_CHUNK_HLOD:
                           if (hlod != null)
                           {
                               throw new InvalidDataException();
                           }
                           hlod = W3dHLod.Parse(reader, header.ChunkSize);
                           break;

                       case W3dChunkType.W3D_CHUNK_ANIMATION:
                           animations.Add(W3dAnimation.Parse(reader, header.ChunkSize));
                           break;

                       case W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION:
                           compressedAnimations.Add(W3dCompressedAnimation.Parse(reader, header.ChunkSize));
                           break;

                       case W3dChunkType.W3D_CHUNK_EMITTER:
                           emitters.Add(W3dEmitter.Parse(reader, header.ChunkSize));
                           break;

                       default:
                           throw CreateUnknownChunkException(header);
                   }
               });
            }
            else
            {
                result = new W3dFile();
            }

            result.FilePath = filePath;
            result.Meshes = meshes;
            result.Boxes = boxes;
            result.Hierarchy = hierarchy;
            result.HLod = hlod;
            result.Animations = animations;
            result.CompressedAnimations = compressedAnimations;
            result.Emitters = emitters;

            return result;
        }

        public void WriteTo(Stream stream)
        {
            using (var writer = new BinaryWriter(stream))
            {
                if (Hierarchy != null)
                {
                    WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_HIERARCHY, true, () =>
                    {
                        Hierarchy.WriteTo(writer);
                    });
                }

                foreach (var animation in Animations)
                {
                    WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_ANIMATION, true, () =>
                    {
                        animation.WriteTo(writer);
                    });
                }

                foreach (var animation in CompressedAnimations)
                {
                    WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION, true, () =>
                    {
                        animation.WriteTo(writer);
                    });
                }

                foreach (var mesh in Meshes)
                {
                    WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_MESH, true, () =>
                    {
                        mesh.WriteTo(writer);
                    });
                }

                foreach (var box in Boxes)
                {
                    WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_BOX, false, () =>
                    {
                        box.WriteTo(writer);
                    });
                }

                if (HLod != null)
                {
                    WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_HLOD, true, () =>
                    {
                        HLod.WriteTo(writer);
                    });
                }

                foreach (var emitter in Emitters)
                {
                    WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_EMITTER, true, () =>
                    {
                        emitter.WriteTo(writer);
                    });
                }
            }
        }
    }
}
