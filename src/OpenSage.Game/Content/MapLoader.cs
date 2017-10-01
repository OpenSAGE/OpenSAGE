using System;
using System.IO;
using System.Linq;
using System.Numerics;
using LLGfx;
using OpenSage.Content.Util;
using OpenSage.Data;
using OpenSage.Data.Map;
using OpenSage.Graphics.Effects;
using OpenSage.Mathematics;
using OpenSage.Terrain;
using OpenSage.Terrain.Util;

namespace OpenSage.Content
{
    internal sealed class MapLoader : ContentLoader<Scene>
    {
        protected override Scene LoadEntry(FileSystemEntry entry, ContentManager contentManager, ResourceUploadBatch uploadBatch)
        {
            var mapFile = MapFile.FromFileSystemEntry(entry);

            var result = new Scene();

            result.Settings.LightingConfigurations = mapFile.GlobalLighting.LightingConfigurations.ToLightSettingsDictionary();
            result.Settings.TimeOfDay = mapFile.GlobalLighting.Time;

            var heightMap = new HeightMap(mapFile.HeightMapData);

            var terrainEntity = new Entity();
            result.Entities.Add(terrainEntity);

            terrainEntity.Components.Add(new TerrainComponent
            {
                HeightMap = heightMap
            });

            var terrainEffect = AddDisposable(new TerrainEffect(
                contentManager.GraphicsDevice, 
                mapFile.BlendTileData.Textures.Length));

            var pipelineStateSolid = new EffectPipelineState(
                RasterizerStateDescription.CullBackSolid,
                DepthStencilStateDescription.Default,
                BlendStateDescription.Opaque)
                .GetHandle();

            var indexBufferCache = AddDisposable(new TerrainPatchIndexBufferCache(contentManager.GraphicsDevice));

            CreatePatches(
                contentManager.GraphicsDevice,
                uploadBatch,
                terrainEntity,
                heightMap,
                mapFile.BlendTileData,
                terrainEffect,
                pipelineStateSolid,
                indexBufferCache);

            var tileDataTexture = AddDisposable(CreateTileDataTexture(
                contentManager.GraphicsDevice,
                uploadBatch,
                mapFile,
                heightMap));

            var cliffDetailsBuffer = AddDisposable(CreateCliffDetails(
                contentManager.GraphicsDevice,
                uploadBatch,
                mapFile));

            CreateTextures(
                contentManager,
                uploadBatch,
                mapFile.BlendTileData,
                out var textures,
                out var textureDetails);

            var textureDetailsBuffer = AddDisposable(StaticBuffer.Create(
                contentManager.GraphicsDevice,
                uploadBatch,
                textureDetails));

            var textureSet = AddDisposable(new TextureSet(
                contentManager.GraphicsDevice, 
                textures));

            terrainEffect.SetTileData(tileDataTexture);
            terrainEffect.SetCliffDetails(cliffDetailsBuffer);
            terrainEffect.SetTextureDetails(textureDetailsBuffer);
            terrainEffect.SetTextures(textureSet);

            var objectsEntity = new Entity();
            result.Entities.Add(objectsEntity);
            LoadObjects(
                contentManager,
                objectsEntity, 
                heightMap,
                mapFile.ObjectsList.Objects);

            return result;
        }

        private void LoadObjects(
            ContentManager contentManager,
            Entity objectsEntity, 
            HeightMap heightMap,
            MapObject[] mapObjects)
        {
            foreach (var mapObject in mapObjects)
            {
                switch (mapObject.RoadType)
                {
                    case RoadType.None:
                        var objectDefinition = contentManager.IniDataContext.Objects.FirstOrDefault(x => x.Name == mapObject.TypeName);
                        if (objectDefinition != null)
                        {
                            var position = mapObject.Position.ToVector3();
                            position.Z = heightMap.GetHeight(position.X, position.Y);

                            var objectEntity = Entity.FromObjectDefinition(objectDefinition);

                            objectEntity.Transform.LocalPosition = position;
                            objectEntity.Transform.LocalEulerAngles = new Vector3(0, 0, mapObject.Angle);

                            objectsEntity.AddChild(objectEntity);
                        }
                        break;

                    default:
                        // TODO: Roads.
                        break;
                }
            }
        }

        private void CreatePatches(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            Entity terrainEntity,
            HeightMap heightMap,
            BlendTileData blendTileData,
            TerrainEffect terrainEffect,
            EffectPipelineStateHandle pipelineStateHandle,
            TerrainPatchIndexBufferCache indexBufferCache)
        {
            const int numTilesPerPatch = TerrainComponent.PatchSize - 1;

            var numPatchesX = heightMap.Width / numTilesPerPatch;
            if (heightMap.Width % numTilesPerPatch != 0)
            {
                numPatchesX += 1;
            }

            var numPatchesY = heightMap.Height / numTilesPerPatch;
            if (heightMap.Height % numTilesPerPatch != 0)
            {
                numPatchesY += 1;
            }

            for (var y = 0; y < numPatchesY; y++)
            {
                for (var x = 0; x < numPatchesX; x++)
                {
                    var patchX = x * numTilesPerPatch;
                    var patchY = y * numTilesPerPatch;

                    var patchBounds = new Int32Rect
                    {
                        X = patchX,
                        Y = patchY,
                        Width = Math.Min(TerrainComponent.PatchSize, heightMap.Width - patchX),
                        Height = Math.Min(TerrainComponent.PatchSize, heightMap.Height - patchY)
                    };

                    terrainEntity.Components.Add(CreatePatch(
                        terrainEffect,
                        pipelineStateHandle,
                        heightMap,
                        blendTileData,
                        patchBounds,
                        graphicsDevice,
                        uploadBatch,
                        indexBufferCache));
                }
            }
        }

        private TerrainPatchComponent CreatePatch(
            TerrainEffect terrainEffect,
            EffectPipelineStateHandle pipelineStateHandle,
            HeightMap heightMap,
            BlendTileData blendTileData,
            Int32Rect patchBounds,
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            TerrainPatchIndexBufferCache indexBufferCache)
        {
            var indexBuffer = indexBufferCache.GetIndexBuffer(
                patchBounds.Width,
                patchBounds.Height,
                uploadBatch,
                out var indices);

            var vertexBuffer = AddDisposable(CreateVertexBuffer(
                graphicsDevice,
                uploadBatch,
                heightMap,
                patchBounds,
                indices,
                out var boundingBox,
                out var triangles));

            return new TerrainPatchComponent(
                terrainEffect,
                pipelineStateHandle,
                patchBounds,
                vertexBuffer,
                indexBuffer,
                triangles,
                boundingBox);
        }

        private static StaticBuffer<TerrainVertex> CreateVertexBuffer(
           GraphicsDevice graphicsDevice,
           ResourceUploadBatch uploadBatch,
           HeightMap heightMap,
           Int32Rect patchBounds,
           ushort[] indices,
           out BoundingBox boundingBox,
           out Triangle[] triangles)
        {
            var numVertices = patchBounds.Width * patchBounds.Height;

            var vertices = new TerrainVertex[numVertices];
            var points = new Vector3[numVertices];

            var vertexIndex = 0;
            for (var y = patchBounds.Y; y < patchBounds.Y + patchBounds.Height; y++)
            {
                for (var x = patchBounds.X; x < patchBounds.X + patchBounds.Width; x++)
                {
                    var position = heightMap.GetPosition(x, y);
                    points[vertexIndex] = position;
                    vertices[vertexIndex++] = new TerrainVertex
                    {
                        Position = position,
                        Normal = heightMap.Normals[x, y],
                        UV = new Vector2(x, y)
                    };
                }
            }

            boundingBox = BoundingBox.CreateFromPoints(points);

            triangles = new Triangle[(patchBounds.Width - 1) * (patchBounds.Height) * 2];

            var triangleIndex = 0;
            var indexIndex = 0;
            for (var y = 0; y < patchBounds.Height - 1; y++)
            {
                for (var x = 0; x < patchBounds.Width - 1; x++)
                {
                    // Triangle 1
                    triangles[triangleIndex++] = new Triangle
                    {
                        V0 = points[indices[indexIndex++]],
                        V1 = points[indices[indexIndex++]],
                        V2 = points[indices[indexIndex++]]
                    };

                    // Triangle 2
                    triangles[triangleIndex++] = new Triangle
                    {
                        V0 = points[indices[indexIndex++]],
                        V1 = points[indices[indexIndex++]],
                        V2 = points[indices[indexIndex++]]
                    };
                }
            }

            return StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                vertices);
        }

        private static Texture CreateTileDataTexture(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            MapFile mapFile,
            HeightMap heightMap)
        {
            var tileData = new uint[heightMap.Width * heightMap.Height * 4];

            var tileDataIndex = 0;
            for (var y = 0; y < heightMap.Height; y++)
            {
                for (var x = 0; x < heightMap.Width; x++)
                {
                    var baseTextureIndex = (byte) mapFile.BlendTileData.TextureIndices[mapFile.BlendTileData.Tiles[x, y]].TextureIndex;

                    var blendData1 = GetBlendData(mapFile, x, y, mapFile.BlendTileData.Blends[x, y], baseTextureIndex);
                    var blendData2 = GetBlendData(mapFile, x, y, mapFile.BlendTileData.ThreeWayBlends[x, y], baseTextureIndex);

                    uint packedTextureIndices = 0;
                    packedTextureIndices |= baseTextureIndex;
                    packedTextureIndices |= (uint) (blendData1.TextureIndex << 8);
                    packedTextureIndices |= (uint) (blendData2.TextureIndex << 16);

                    tileData[tileDataIndex++] = packedTextureIndices;

                    var packedBlendInfo = 0u;
                    packedBlendInfo |= blendData1.BlendDirection;
                    packedBlendInfo |= (uint) (blendData1.Flags << 8);
                    packedBlendInfo |= (uint) (blendData2.BlendDirection << 16);
                    packedBlendInfo |= (uint) (blendData2.Flags << 24);

                    tileData[tileDataIndex++] = packedBlendInfo;

                    tileData[tileDataIndex++] = mapFile.BlendTileData.CliffTextures[x, y];

                    tileData[tileDataIndex++] = 0;
                }
            }

            byte[] textureIDsByteArray = new byte[tileData.Length * sizeof(uint)];
            System.Buffer.BlockCopy(tileData, 0, textureIDsByteArray, 0, tileData.Length * sizeof(uint));

            return Texture.CreateTexture2D(
                graphicsDevice,
                uploadBatch,
                PixelFormat.Rgba32UInt,
                heightMap.Width,
                heightMap.Height,
                new[]
                {
                    new TextureMipMapData
                    {
                        BytesPerRow = heightMap.Width * sizeof(uint) * 4,
                        Data = textureIDsByteArray
                    }
                });
        }

        private static BlendData GetBlendData(
            MapFile mapFile, 
            int x, int y, 
            ushort blendIndex, 
            byte baseTextureIndex)
        {
            if (blendIndex > 0)
            {
                var blendDescription = mapFile.BlendTileData.BlendDescriptions[blendIndex - 1];
                var flipped = blendDescription.Flags.HasFlag(BlendFlags.Flipped);
                var flags = (byte) (flipped ? 1 : 0);
                if (blendDescription.TwoSided)
                {
                    flags |= 2;
                }
                return new BlendData
                {
                    TextureIndex = (byte) mapFile.BlendTileData.TextureIndices[(int) blendDescription.SecondaryTextureTile].TextureIndex,
                    BlendDirection = (byte) blendDescription.BlendDirection,
                    Flags = flags
                };
            }
            else
            {
                return new BlendData
                {
                    TextureIndex = baseTextureIndex
                };
            }
        }

        private struct BlendData
        {
            public byte TextureIndex;
            public byte BlendDirection;
            public byte Flags;
        }

        private static StaticBuffer<CliffInfo> CreateCliffDetails(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            MapFile mapFile)
        {
            var cliffDetails = new CliffInfo[mapFile.BlendTileData.CliffTextureMappings.Length];

            const int cliffScalingFactor = 64;
            for (var i = 0; i < cliffDetails.Length; i++)
            {
                var cliffMapping = mapFile.BlendTileData.CliffTextureMappings[i];
                cliffDetails[i] = new CliffInfo
                {
                    BottomLeftUV = cliffMapping.BottomLeftCoords.ToVector2() * cliffScalingFactor,
                    BottomRightUV = cliffMapping.BottomRightCoords.ToVector2() * cliffScalingFactor,
                    TopLeftUV = cliffMapping.TopLeftCoords.ToVector2() * cliffScalingFactor,
                    TopRightUV = cliffMapping.TopRightCoords.ToVector2() * cliffScalingFactor
                };
            }

            return cliffDetails.Length > 0
                ? StaticBuffer.Create(
                    graphicsDevice,
                    uploadBatch,
                    cliffDetails)
                : null;
        }

        private static void CreateTextures(
            ContentManager contentManager,
            ResourceUploadBatch uploadBatch,
            BlendTileData blendTileData,
            out Texture[] textures,
            out TextureInfo[] textureDetails)
        {
            var numTextures = blendTileData.Textures.Length;

            textures = new Texture[numTextures];
            textureDetails = new TextureInfo[numTextures];
            for (var i = 0; i < numTextures; i++)
            {
                var mapTexture = blendTileData.Textures[i];

                var terrainType = contentManager.IniDataContext.TerrainTextures.First(x => x.Name == mapTexture.Name);

                var texturePath = Path.Combine("Art", "Terrain", terrainType.Texture);
                textures[i] = contentManager.Load<Texture>(texturePath, uploadBatch);

                textureDetails[i] = new TextureInfo
                {
                    CellSize = mapTexture.CellSize * 2
                };
            }
        }
    }
}
