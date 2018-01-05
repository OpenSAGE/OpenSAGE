using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Content.Util;
using OpenSage.Data;
using OpenSage.Data.Map;
using OpenSage.Data.Tga;
using OpenSage.Mathematics;
using OpenSage.Scripting;
using OpenSage.Scripting.Actions;
using OpenSage.Scripting.Conditions;
using OpenSage.Settings;
using OpenSage.Terrain;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace OpenSage.Content
{
    internal sealed class MapLoader : ContentLoader<Scene>
    {
        protected override Scene LoadEntry(FileSystemEntry entry, ContentManager contentManager, LoadOptions loadOptions)
        {
            contentManager.IniDataContext.LoadIniFile(@"Data\INI\Terrain.ini");

            var mapFile = MapFile.FromFileSystemEntry(entry);

            var result = new Scene();

            result.MapFile = mapFile;

            result.Settings.LightingConfigurations = mapFile.GlobalLighting.LightingConfigurations.ToLightSettingsDictionary();
            result.Settings.TimeOfDay = mapFile.GlobalLighting.Time;

            var heightMap = new HeightMap(mapFile.HeightMapData);
            result.HeightMap = heightMap;

            var indexBufferCache = AddDisposable(new TerrainPatchIndexBufferCache(contentManager.GraphicsDevice));

            var tileDataTexture = AddDisposable(CreateTileDataTexture(
                contentManager.GraphicsDevice,
                mapFile,
                heightMap));

            var cliffDetailsBuffer = AddDisposable(CreateCliffDetails(
                contentManager.GraphicsDevice,
                mapFile));

            CreateTextures(
                contentManager,
                mapFile.BlendTileData,
                out var textureArray,
                out var textureDetails);

            var textureDetailsBuffer = AddDisposable(Buffer<TextureInfo>.CreateStatic(
                contentManager.GraphicsDevice,
                textureDetails,
                BufferBindFlags.ShaderResource));

            var terrainMaterial = new TerrainMaterial(contentManager.EffectLibrary.Terrain);

            terrainMaterial.SetTileData(tileDataTexture);
            terrainMaterial.SetCliffDetails(cliffDetailsBuffer);
            terrainMaterial.SetTextureDetails(textureDetailsBuffer);
            terrainMaterial.SetTextureArray(textureArray);

            var terrainPatches = CreatePatches(
                contentManager.GraphicsDevice,
                heightMap,
                mapFile.BlendTileData,
                terrainMaterial,
                indexBufferCache);

            var terrain = new Terrain.Terrain(heightMap, terrainPatches);

            var world = new World(terrain);

            result.Scene3D = new Scene3D(world);

            var objectsEntity = new Entity();
            result.Entities.Add(objectsEntity);
            LoadObjects(
                contentManager,
                objectsEntity, 
                heightMap,
                mapFile.ObjectsList.Objects,
                result.Settings);

            foreach (var team in mapFile.SidesList.Teams ?? mapFile.Teams.Items)
            {
                var name = (string) team.Properties["teamName"].Value;
                var owner = (string) team.Properties["teamOwner"].Value;
                var isSingleton = (bool) team.Properties["teamIsSingleton"].Value;

                // TODO
            }

            var waypointPaths = mapFile.WaypointsList.WaypointPaths.Select(path =>
            {
                var start = result.Settings.Waypoints[path.StartWaypointID];
                var end = result.Settings.Waypoints[path.EndWaypointID];
                return new Settings.WaypointPath(start, end);
            }).ToList();

            result.Settings.WaypointPaths = new WaypointPathCollection(waypointPaths);

            var scriptsEntity = new Entity();
            result.Entities.Add(scriptsEntity);

            // TODO: Don't hardcode this.
            // Perhaps add one ScriptComponent for the neutral player, 
            // and one for the active player.
            var scriptList = (mapFile.SidesList.PlayerScripts ?? mapFile.PlayerScriptsList).ScriptLists[0];
            AddScripts(scriptsEntity, scriptList, result.Settings);

            return result;
        }

        private void AddScripts(Entity scriptsEntity, ScriptList scriptList, SceneSettings sceneSettings)
        {
            scriptsEntity.AddComponent(new ScriptComponent
            {
                ScriptGroups = CreateMapScriptGroups(scriptList.ScriptGroups, sceneSettings),
                Scripts = CreateMapScripts(scriptList.Scripts, sceneSettings)
            });
        }

        private static MapScriptGroup[] CreateMapScriptGroups(ScriptGroup[] scriptGroups, SceneSettings sceneSettings)
        {
            var result = new MapScriptGroup[scriptGroups.Length];

            for (var i = 0; i < result.Length; i++)
            {
                result[i] = CreateMapScriptGroup(scriptGroups[i], sceneSettings);
            }

            return result;
        }

        private static MapScriptGroup CreateMapScriptGroup(ScriptGroup scriptGroup, SceneSettings sceneSettings)
        {
            return new MapScriptGroup(
                scriptGroup.Name,
                CreateMapScripts(scriptGroup.Scripts, sceneSettings),
                scriptGroup.IsActive,
                scriptGroup.IsSubroutine);
        }

        private static MapScript[] CreateMapScripts(Script[] scripts, SceneSettings sceneSettings)
        {
            var result = new MapScript[scripts.Length];

            for (var i = 0; i < scripts.Length; i++)
            {
                result[i] = CreateMapScript(scripts[i], sceneSettings);
            }

            return result;
        }

        private static MapScript CreateMapScript(Script script, SceneSettings sceneSettings)
        {
            var conditions = CreateMapScriptConditions(script.OrConditions, sceneSettings);

            var actionsIfTrue = CreateMapScriptActions(script.ActionsIfTrue, sceneSettings);
            var actionsIfFalse = CreateMapScriptActions(script.ActionsIfFalse, sceneSettings);

            return new MapScript(
                script.Name,
                conditions,
                actionsIfTrue,
                actionsIfFalse,
                script.IsActive,
                script.DeactivateUponSuccess,
                script.IsSubroutine,
                script.EvaluationInterval);
        }

        private static MapScriptConditions CreateMapScriptConditions(ScriptOrCondition[] orConditions, SceneSettings sceneSettings)
        {
            var result = new MapScriptOrCondition[orConditions.Length];

            for (var i = 0; i < orConditions.Length; i++)
            {
                var orCondition = orConditions[i];

                var andConditions = new MapScriptCondition[orCondition.Conditions.Length];

                for (var j = 0; j < andConditions.Length; j++)
                {
                    andConditions[j] = MapScriptConditionFactory.Create(orCondition.Conditions[j], sceneSettings);
                }

                result[i] = new MapScriptOrCondition(andConditions);
            }

            return new MapScriptConditions(result);
        }

        private static List<MapScriptAction> CreateMapScriptActions(ScriptAction[] actions, SceneSettings sceneSettings)
        {
            var result = new List<MapScriptAction>(actions.Length);

            for (var i = 0; i < actions.Length; i++)
            {
                result.Add(MapScriptActionFactory.Create(actions[i], sceneSettings, result));
            }

            return result;
        }

        private static Waypoint CreateWaypoint(MapObject mapObject)
        {
            var waypointID = (uint) mapObject.Properties["waypointID"].Value;
            var waypointName = (string) mapObject.Properties["waypointName"].Value;

            string[] pathLabels = null;

            var label1 = mapObject.Properties["waypointPathLabel1"];

            // It seems that if one of the label properties exists, all of them do
            if (label1 != null)
            {
                pathLabels = new[]
                {
                    (string) label1.Value,
                    (string) mapObject.Properties["waypointPathLabel2"].Value,
                    (string) mapObject.Properties["waypointPathLabel3"].Value
                };
            }

            return new Waypoint(waypointID, waypointName, mapObject.Position, pathLabels);
        }

        private static void LoadObjects(
            ContentManager contentManager,
            Entity objectsEntity, 
            HeightMap heightMap,
            MapObject[] mapObjects,
            SceneSettings sceneSettings)
        {
            var waypoints = new List<Waypoint>();

            foreach (var mapObject in mapObjects)
            {
                switch (mapObject.RoadType)
                {
                    case RoadType.None:
                        var position = mapObject.Position;

                        switch (mapObject.TypeName)
                        {
                            case "*Waypoints/Waypoint":
                                waypoints.Add(CreateWaypoint(mapObject));
                                break;

                            default:
                                // TODO: Handle locomotors when they're implemented.
                                position.Z += heightMap.GetHeight(position.X, position.Y);

                                var objectEntity = contentManager.InstantiateObject(mapObject.TypeName);
                                if (objectEntity != null)
                                {
                                    objectEntity.Transform.LocalPosition = position;
                                    objectEntity.Transform.LocalEulerAngles = new Vector3(0, 0, mapObject.Angle);

                                    objectsEntity.AddChild(objectEntity);
                                }
                                else
                                {
                                    // TODO
                                }
                                break;
                        }
                        break;

                    default:
                        // TODO: Roads.
                        break;
                }
            }

            sceneSettings.Waypoints = new WaypointCollection(waypoints);
        }

        private List<TerrainPatch> CreatePatches(
            GraphicsDevice graphicsDevice,
            HeightMap heightMap,
            BlendTileData blendTileData,
            TerrainMaterial terrainMaterial,
            TerrainPatchIndexBufferCache indexBufferCache)
        {
            const int numTilesPerPatch = Terrain.Terrain.PatchSize - 1;

            var heightMapWidthMinusOne = heightMap.Width - 1;
            var numPatchesX = heightMapWidthMinusOne / numTilesPerPatch;
            if (heightMapWidthMinusOne % numTilesPerPatch != 0)
            {
                numPatchesX += 1;
            }

            var heightMapHeightMinusOne = heightMap.Height - 1;
            var numPatchesY = heightMapHeightMinusOne / numTilesPerPatch;
            if (heightMapHeightMinusOne % numTilesPerPatch != 0)
            {
                numPatchesY += 1;
            }

            var patches = new List<TerrainPatch>();

            for (var y = 0; y < numPatchesY; y++)
            {
                for (var x = 0; x < numPatchesX; x++)
                {
                    var patchX = x * numTilesPerPatch;
                    var patchY = y * numTilesPerPatch;

                    var patchBounds = new Rectangle(
                        patchX,
                        patchY,
                        Math.Min(Terrain.Terrain.PatchSize, heightMap.Width - patchX),
                        Math.Min(Terrain.Terrain.PatchSize, heightMap.Height - patchY));

                    patches.Add(CreatePatch(
                        terrainMaterial,
                        heightMap,
                        blendTileData,
                        patchBounds,
                        graphicsDevice,
                        indexBufferCache));
                }
            }

            return patches;
        }

        private TerrainPatch CreatePatch(
            TerrainMaterial terrainMaterial,
            HeightMap heightMap,
            BlendTileData blendTileData,
            Rectangle patchBounds,
            GraphicsDevice graphicsDevice,
            TerrainPatchIndexBufferCache indexBufferCache)
        {
            var indexBuffer = indexBufferCache.GetIndexBuffer(
                patchBounds.Width,
                patchBounds.Height,
                out var indices);

            var vertexBuffer = AddDisposable(CreateVertexBuffer(
                graphicsDevice,
                heightMap,
                patchBounds,
                indices,
                out var boundingBox,
                out var triangles));

            return new TerrainPatch(
                terrainMaterial,
                patchBounds,
                vertexBuffer,
                indexBuffer,
                triangles,
                boundingBox);
        }

        private static Buffer<TerrainVertex> CreateVertexBuffer(
           GraphicsDevice graphicsDevice,
           HeightMap heightMap,
           Rectangle patchBounds,
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

            return Buffer<TerrainVertex>.CreateStatic(
                graphicsDevice,
                vertices,
                BufferBindFlags.VertexBuffer);
        }

        private static Texture CreateTileDataTexture(
            GraphicsDevice graphicsDevice,
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

        private static Buffer<CliffInfo> CreateCliffDetails(
            GraphicsDevice graphicsDevice,
            MapFile mapFile)
        {
            var cliffDetails = new CliffInfo[mapFile.BlendTileData.CliffTextureMappings.Length];

            const int cliffScalingFactor = 64;
            for (var i = 0; i < cliffDetails.Length; i++)
            {
                var cliffMapping = mapFile.BlendTileData.CliffTextureMappings[i];
                cliffDetails[i] = new CliffInfo
                {
                    BottomLeftUV = cliffMapping.BottomLeftCoords * cliffScalingFactor,
                    BottomRightUV = cliffMapping.BottomRightCoords * cliffScalingFactor,
                    TopLeftUV = cliffMapping.TopLeftCoords * cliffScalingFactor,
                    TopRightUV = cliffMapping.TopRightCoords * cliffScalingFactor
                };
            }

            return cliffDetails.Length > 0
                ? Buffer<CliffInfo>.CreateStatic(
                    graphicsDevice,
                    cliffDetails,
                    BufferBindFlags.ShaderResource)
                : null;
        }

        private void CreateTextures(
            ContentManager contentManager,
            BlendTileData blendTileData,
            out Texture textureArray,
            out TextureInfo[] textureDetails)
        {
            var graphicsDevice = contentManager.GraphicsDevice;

            var numTextures = blendTileData.Textures.Length;

            var textureInfo = new (int size, FileSystemEntry entry)[numTextures];
            var largestTextureSize = int.MinValue;

            textureDetails = new TextureInfo[numTextures];

            for (var i = 0; i < numTextures; i++)
            {
                var mapTexture = blendTileData.Textures[i];

                var terrainType = contentManager.IniDataContext.TerrainTextures.First(x => x.Name == mapTexture.Name);
                var texturePath = Path.Combine("Art", "Terrain", terrainType.Texture);
                var entry = contentManager.FileSystem.GetFile(texturePath);

                var size = TgaFile.GetSquareTextureSize(entry);

                textureInfo[i] = (size, entry);

                if (size > largestTextureSize)
                {
                    largestTextureSize = size;
                }

                textureDetails[i] = new TextureInfo
                {
                    TextureIndex = (uint) i,
                    CellSize = mapTexture.CellSize * 2
                };
            }

            textureArray = AddDisposable(Texture.CreateTexture2DArray(
                graphicsDevice,
                PixelFormat.Rgba8UNorm,
                numTextures,
                MipMapUtility.CalculateMipMapCount(largestTextureSize, largestTextureSize),
                largestTextureSize,
                largestTextureSize));

            for (var i = 0; i < numTextures; i++)
            {
                var tgaFile = TgaFile.FromFileSystemEntry(textureInfo[i].entry);
                var originalData = TextureLoader.GetData(tgaFile, false)[0].Data;

                byte[] resizedData;
                if (tgaFile.Header.Width == largestTextureSize)
                {
                    resizedData = originalData;
                }
                else
                {
                    using (var tgaImage = Image.LoadPixelData<Rgba32>(originalData, tgaFile.Header.Width, tgaFile.Header.Height))
                    {
                        tgaImage.Mutate(x => x
                            .Resize(largestTextureSize, largestTextureSize, new Lanczos2Resampler()));

                        resizedData = tgaImage.SavePixelData();
                    }
                }

                var resizedMipMaps = MipMapUtility.GenerateMipMaps(
                    largestTextureSize,
                    largestTextureSize,
                    resizedData);

                using (var sourceTexture = Texture.CreateTexture2D(
                    graphicsDevice,
                    PixelFormat.Rgba8UNorm,
                    largestTextureSize,
                    largestTextureSize,
                    resizedMipMaps))
                {
                    textureArray.CopyFromTexture(sourceTexture, i);
                }
            }
        }
    }
}
