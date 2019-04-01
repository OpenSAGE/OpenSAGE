using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenSage.Content.Util;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Data.Map;
using OpenSage.Data.Tga;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Shaders;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Scripting;
using OpenSage.Settings;
using OpenSage.Terrain;
using OpenSage.Utilities;
using OpenSage.Utilities.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using Veldrid;
using Veldrid.ImageSharp;
using Player = OpenSage.Logic.Player;
using Rectangle = OpenSage.Mathematics.Rectangle;
using Team = OpenSage.Logic.Team;

namespace OpenSage.Content
{
    internal sealed class MapLoader : ContentLoader<Scene3D>
    {
        private static readonly IResampler MapTextureResampler = new Lanczos2Resampler();

        protected override Scene3D LoadEntry(FileSystemEntry entry, ContentManager contentManager, Game game, LoadOptions loadOptions)
        {
            switch (contentManager.SageGame)
            {
                case SageGame.Ra3:
                case SageGame.Ra3Uprising:
                case SageGame.Cnc4:
                    // TODO
                    break;

                default:
                    contentManager.IniDataContext.LoadIniFile(@"Data\INI\Terrain.ini");
                    contentManager.IniDataContext.LoadIniFile(@"Data\INI\Roads.ini");
                    break;
            }

            var mapFile = MapFile.FromFileSystemEntry(entry);

            var heightMap = new HeightMap(mapFile.HeightMapData);

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

            var textureDetailsBuffer = AddDisposable(contentManager.GraphicsDevice.CreateStaticStructuredBuffer(textureDetails));

            var terrainPipeline = contentManager.ShaderResources.Terrain.Pipeline;

            Texture LoadTexture(string name)
            {
                var texture = contentManager.Load<Texture>(Path.Combine("Art", "Textures", name), fallbackToPlaceholder: false);
                if (texture == null)
                {
                    texture = contentManager.Load<Texture>(Path.Combine("Art", "CompiledTextures", name.Substring(0, 2), name));
                }
                return texture;
            }

            var materialConstantsBuffer = AddDisposable(contentManager.GraphicsDevice.CreateStaticBuffer(
                new TerrainShaderResources.TerrainMaterialConstants
                {
                    MapBorderWidth = new Vector2(mapFile.HeightMapData.BorderWidth, mapFile.HeightMapData.BorderWidth) * HeightMap.HorizontalScale,
                    MapSize = new Vector2(mapFile.HeightMapData.Width, mapFile.HeightMapData.Height) * HeightMap.HorizontalScale,
                    IsMacroTextureStretched = false // TODO: This must be one of the EnvironmentData unknown values.
                },
                BufferUsage.UniformBuffer));

            var macroTexture = LoadTexture(mapFile.EnvironmentData?.MacroTexture ?? "tsnoiseurb.dds");

            var materialResourceSet = AddDisposable(contentManager.ShaderResources.Terrain.CreateMaterialResourceSet(
                materialConstantsBuffer,
                tileDataTexture,
                cliffDetailsBuffer ?? contentManager.GetNullStructuredBuffer(TerrainShaderResources.CliffInfo.Size),
                textureDetailsBuffer,
                textureArray,
                macroTexture));

            var terrainPatches = CreatePatches(
                contentManager.GraphicsDevice,
                heightMap,
                mapFile.BlendTileData,
                indexBufferCache,
                materialResourceSet);

            var cloudTexture = LoadTexture(mapFile.EnvironmentData?.CloudTexture ?? "tscloudmed.dds");
            cloudTexture.Name = "Cloud texture";

            var cloudResourceLayout = AddDisposable(contentManager.GraphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Global_CloudTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment))));

            var cloudResourceSet = AddDisposable(contentManager.GraphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    cloudResourceLayout,
                    cloudTexture)));
            cloudResourceSet.Name = "Cloud resource set";

            var terrain = new Terrain.Terrain(
                heightMap,
                terrainPatches,
                contentManager.ShaderResources.Terrain.ShaderSet,
                terrainPipeline,
                cloudResourceSet);

            var players = Player.FromMapData(mapFile.SidesList.Players, contentManager).ToArray();

            var teams = (mapFile.SidesList.Teams ?? mapFile.Teams.Items)
                .Select(team => Team.FromMapData(team, players))
                .ToArray();

            LoadObjects(
                contentManager,
                heightMap,
                mapFile.ObjectsList.Objects,
                teams,
                out var waypoints,
                out var gameObjects,
                out var roads,
                out var bridges);

            var waterAreas = new List<WaterArea>();

            if (mapFile.PolygonTriggers != null)
            {
                foreach (var polygonTrigger in mapFile.PolygonTriggers.Triggers)
                {
                    switch (polygonTrigger.TriggerType)
                    {
                        case PolygonTriggerType.Water:
                        case PolygonTriggerType.River: // TODO: Handle this differently. Water texture should be animated "downstream".
                        case PolygonTriggerType.WaterAndRiver:
                            if (WaterArea.TryCreate(contentManager, polygonTrigger, out var waterArea))
                            {
                                waterAreas.Add(AddDisposable(waterArea));
                            }
                            break;
                    }
                }
            }

            var lighting = new WorldLighting(
                mapFile.GlobalLighting.LightingConfigurations.ToLightSettingsDictionary(),
                mapFile.GlobalLighting.Time);

            var waypointPaths = new WaypointPathCollection(mapFile.WaypointsList.WaypointPaths
                .Select(path =>
                {
                    var start = waypoints[path.StartWaypointID];
                    var end = waypoints[path.EndWaypointID];
                    return new Settings.WaypointPath(start, end);
                }));

            // TODO: Don't hardcode this.
            // Perhaps add one ScriptComponent for the neutral player, 
            // and one for the active player.
            var scriptList = mapFile.GetPlayerScriptsList().ScriptLists[0];
            var mapScripts = CreateScripts(scriptList);

            var cameraController = new RtsCameraController(contentManager)
            {
                TerrainPosition = terrain.HeightMap.GetPosition(
                    terrain.HeightMap.Width / 2,
                    terrain.HeightMap.Height / 2)
            };

            contentManager.GraphicsDevice.WaitForIdle();

            return new Scene3D(
                game,
                game.InputMessageBuffer,
                () => game.Viewport,
                cameraController,
                mapFile,
                terrain,
                waterAreas.ToArray(),
                roads,
                bridges,
                mapScripts,
                gameObjects,
                waypoints,
                waypointPaths,
                lighting,
                players,
                teams);
        }

        private MapScriptCollection CreateScripts(ScriptList scriptList)
        {
            return new MapScriptCollection(
                CreateMapScriptGroups(scriptList.ScriptGroups),
                CreateMapScripts(scriptList.Scripts));
        }

        private static MapScriptGroup[] CreateMapScriptGroups(ScriptGroup[] scriptGroups)
        {
            var result = new MapScriptGroup[scriptGroups.Length];

            for (var i = 0; i < result.Length; i++)
            {
                result[i] = CreateMapScriptGroup(scriptGroups[i]);
            }

            return result;
        }

        private static MapScriptGroup CreateMapScriptGroup(ScriptGroup scriptGroup)
        {
            return new MapScriptGroup(
                scriptGroup.Name,
                CreateMapScripts(scriptGroup.Scripts),
                scriptGroup.IsActive,
                scriptGroup.IsSubroutine);
        }

        private static MapScript[] CreateMapScripts(Script[] scripts)
        {
            var result = new MapScript[scripts.Length];

            for (var i = 0; i < scripts.Length; i++)
            {
                result[i] = CreateMapScript(scripts[i]);
            }

            return result;
        }

        private static MapScript CreateMapScript(Script script)
        {
            var actionsIfTrue = script.ActionsIfTrue;
            var actionsIfFalse = script.ActionsIfFalse;

            return new MapScript(
                script.Name,
                script.OrConditions,
                actionsIfTrue,
                actionsIfFalse,
                script.IsActive,
                script.DeactivateUponSuccess,
                script.IsSubroutine,
                script.EvaluationInterval);
        }

        private static Waypoint CreateWaypoint(MapObject mapObject)
        {
            var waypointID = (uint) mapObject.Properties["waypointID"].Value;
            var waypointName = (string) mapObject.Properties["waypointName"].Value;

            string[] pathLabels = null;

            // It seems that if one of the label properties exists, all of them do
            if (mapObject.Properties.TryGetValue("waypointPathLabel1", out var label1))
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

        private static GameObject CreateGameObject(MapObject mapObject, Team[] teams, ContentManager contentManager)
        {
            var gameObject = contentManager.InstantiateObject(mapObject.TypeName);

            // TODO: Is there any valid case where we'd want to return null instead of throwing an exception?
            if (gameObject == null)
            {
                return null;
            }

            // TODO: If the object doesn't have a health value, how do we initialise it?
            if (gameObject.Definition.Body is ActiveBodyModuleData body)
            {
                var healthMultiplier = mapObject.Properties.TryGetValue("objectInitialHealth", out var health)
                    ? (uint) health.Value / 100.0f
                    : 1.0f;

                // TODO: Should we use InitialHealth or MaximumHealth here?
                var initialHealth = body.InitialHealth * healthMultiplier;
                gameObject.Health = (decimal) initialHealth;
            }

            if (mapObject.Properties.TryGetValue("originalOwner", out var teamName))
            {
                var name = (string) teamName.Value;
                if (name.Contains('/'))
                {
                    name = name.Split('/')[1];
                }
                var team = teams.FirstOrDefault(t => t.Name == name);
                gameObject.Team = team;
                gameObject.Owner = team?.Owner;
            }

            if (mapObject.Properties.TryGetValue("objectSelectable", out var selectable))
            {
                gameObject.IsSelectable = (bool) selectable.Value;
            }

            return gameObject;
        }

        private void LoadObjects(
            ContentManager contentManager,
            HeightMap heightMap,
            MapObject[] mapObjects,
            Team[] teams,
            out WaypointCollection waypointCollection,
            out GameObjectCollection gameObjects,
            out Road[] roads,
            out Bridge[] bridges)
        {
            var waypoints = new List<Waypoint>();
            gameObjects = new GameObjectCollection(contentManager);
            var roadsList = new List<Road>();
            var bridgesList = new List<Bridge>();

            var roadTopology = new RoadTopology();

            for (var i = 0; i < mapObjects.Length; i++)
            {
                var mapObject = mapObjects[i];

                var position = mapObject.Position;

                switch (mapObject.RoadType & RoadType.PrimaryType)
                {
                    case RoadType.None:
                        switch (mapObject.TypeName)
                        {
                            case "*Waypoints/Waypoint":
                                waypoints.Add(CreateWaypoint(mapObject));
                                break;

                            default:
                                position.Z += heightMap.GetHeight(position.X, position.Y);

                                var gameObject = CreateGameObject(mapObject, teams, contentManager);

                                if (gameObject != null)
                                {
                                    gameObject.Transform.Translation = position;
                                    gameObject.Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, mapObject.Angle);

                                    gameObjects.Add(gameObject);

                                    if (gameObject.Definition.IsBridge)
                                    {
                                        // This is a landmark bridge. We need to add towers at the corners.
                                        CreateTowers(contentManager, gameObjects, gameObject, mapObject);
                                    }
                                }

                                break;
                        }
                        break;

                    case RoadType.BridgeStart:
                    case RoadType.BridgeEnd:
                        // Multiple invalid bridges can be found in e.g GLA01.
                        // TODO: Log a warning.
                        if ((i + 1) >= mapObjects.Length || !mapObjects[i + 1].RoadType.HasFlag(RoadType.BridgeEnd))
                        {
                            continue;
                        }

                        var bridgeEnd = mapObjects[++i];

                        var bridgeTemplate = GetBridgeTemplate(contentManager, mapObject);

                        if (Bridge.TryCreateBridge(
                            contentManager,
                            heightMap,
                            bridgeTemplate,
                            mapObject.Position,
                            bridgeEnd.Position,
                            out var bridge))
                        {
                            bridgesList.Add(AddDisposable(bridge));
                        }

                        
                        break;

                    case RoadType.Start:
                    case RoadType.End:
                        var roadEnd = mapObjects[++i];

                        // Some maps have roads with invalid start- or endpoints.
                        // We'll skip processing them altogether.
                        // TODO: Log a warning.
                        if (mapObject.TypeName == "" || roadEnd.TypeName == "")
                        {
                            continue;
                        }

                        if (!mapObject.RoadType.HasFlag(RoadType.Start) || !roadEnd.RoadType.HasFlag(RoadType.End))
                        {
                            throw new InvalidDataException();
                        }

                        // Note that we're searching with the type of either end.
                        // This is because of weirdly corrupted roads with unmatched ends in USA04, which work fine in WB and SAGE.
                        var roadTemplate = contentManager.IniDataContext.RoadTemplates.Find(x =>
                            x.Name == mapObject.TypeName || x.Name == roadEnd.TypeName);

                        if (roadTemplate == null)
                        {
                            throw new InvalidDataException($"Missing road template: {mapObject.TypeName}");
                        }

                        roadTopology.AddSegment(roadTemplate, mapObject, roadEnd);
                        break;

                }

                contentManager.GraphicsDevice.WaitForIdle();
            }

            // The map stores road segments with no connectivity:
            // - a segment is from point A to point B
            // - with a road type name
            // - and start and end curve types (angled, tight curve, broad curve).

            // The goal is to create road networks of connected road segments,
            // where a network has only a single road type.

            // A road network is composed of 2 or more nodes.
            // A network is a (potentially) cyclic graph.

            // A road node has > 1 and <= 4 edges connected to it.
            // A node can be part of multiple networks.

            // An edge can only exist in one network.

            // TODO: If a node stored in the map has > 4 edges, the extra edges
            // are put into a separate network.

            var networks = roadTopology.BuildNetworks();

            foreach (var network in networks)
            {
                foreach (var edge in network.Edges)
                {
                    var startPosition = edge.Start.TopologyNode.Position;
                    var endPosition = edge.End.TopologyNode.Position;

                    startPosition.Z += heightMap.GetHeight(startPosition.X, startPosition.Y);
                    endPosition.Z += heightMap.GetHeight(endPosition.X, endPosition.Y);

                    roadsList.Add(AddDisposable(new Road(
                        contentManager,
                        heightMap,
                        edge.TopologyEdge.Template,
                        startPosition,
                        endPosition)));
                }
            }

            waypointCollection = new WaypointCollection(waypoints);
            roads = roadsList.ToArray();
            bridges = bridgesList.ToArray();
        }

        private static BridgeTemplate GetBridgeTemplate(ContentManager contentManager, MapObject mapObject)
        {
            var template = contentManager.IniDataContext.Bridges.Find(x => x.Name == mapObject.TypeName);

            if (template == null)
            {
                throw new InvalidDataException($"Missing bridge template: {mapObject.TypeName}");
            }

            return template;
        }

        private void CreateTowers(
            ContentManager contentManager,
            GameObjectCollection gameObjects,
            GameObject gameObject,
            MapObject mapObject)
        {
            var towers = new List<GameObject>();

            void CreateTower(string objectName, float x, float y)
            {
                var tower = AddDisposable(contentManager.InstantiateObject(objectName));

                var offset = new Vector3(x, y, 0);
                var transformedOffset = Vector3.Transform(offset, gameObject.Transform.Rotation);

                tower.Transform.Translation = gameObject.Transform.Translation + transformedOffset;
                tower.Transform.Rotation = gameObject.Transform.Rotation;

                gameObjects.Add(tower);
            }

            var landmarkBridgeTemplate = GetBridgeTemplate(contentManager, mapObject);

            var halfLength = gameObject.Definition.Geometry.MinorRadius;
            var halfWidth = gameObject.Definition.Geometry.MajorRadius;

            CreateTower(landmarkBridgeTemplate.TowerObjectNameFromLeft, -halfWidth, -halfLength);
            CreateTower(landmarkBridgeTemplate.TowerObjectNameFromRight, halfWidth, -halfLength);
            CreateTower(landmarkBridgeTemplate.TowerObjectNameToLeft, -halfWidth, halfLength);
            CreateTower(landmarkBridgeTemplate.TowerObjectNameToRight, halfWidth, halfLength);
        }

        private List<TerrainPatch> CreatePatches(
            GraphicsDevice graphicsDevice,
            HeightMap heightMap,
            BlendTileData blendTileData,
            TerrainPatchIndexBufferCache indexBufferCache,
            ResourceSet materialResourceSet)
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
                        heightMap,
                        blendTileData,
                        patchBounds,
                        graphicsDevice,
                        indexBufferCache,
                        materialResourceSet));
                }
            }

            return patches;
        }

        private TerrainPatch CreatePatch(
            HeightMap heightMap,
            BlendTileData blendTileData,
            Rectangle patchBounds,
            GraphicsDevice graphicsDevice,
            TerrainPatchIndexBufferCache indexBufferCache,
            ResourceSet materialResourceSet)
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
                patchBounds,
                vertexBuffer,
                indexBuffer,
                (uint) indices.Length,
                triangles,
                boundingBox,
                materialResourceSet);
        }

        private static DeviceBuffer CreateVertexBuffer(
           GraphicsDevice graphicsDevice,
           HeightMap heightMap,
           Rectangle patchBounds,
           ushort[] indices,
           out BoundingBox boundingBox,
           out Triangle[] triangles)
        {
            var numVertices = patchBounds.Width * patchBounds.Height;

            var vertices = new TerrainShaderResources.TerrainVertex[numVertices];
            var points = new Vector3[numVertices];

            var vertexIndex = 0;
            for (var y = patchBounds.Y; y < patchBounds.Y + patchBounds.Height; y++)
            {
                for (var x = patchBounds.X; x < patchBounds.X + patchBounds.Width; x++)
                {
                    var position = heightMap.GetPosition(x, y);
                    points[vertexIndex] = position;
                    vertices[vertexIndex++] = new TerrainShaderResources.TerrainVertex
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
                    triangles[triangleIndex++] = new Triangle(
                        points[indices[indexIndex++]],
                        points[indices[indexIndex++]],
                        points[indices[indexIndex++]]);

                    // Triangle 2
                    triangles[triangleIndex++] = new Triangle(
                        points[indices[indexIndex++]],
                        points[indices[indexIndex++]],
                        points[indices[indexIndex++]]);
                }
            }

            return graphicsDevice.CreateStaticBuffer(vertices, BufferUsage.VertexBuffer);
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

            var textureIDsByteArray = new byte[tileData.Length * sizeof(float)];
            Buffer.BlockCopy(tileData, 0, textureIDsByteArray, 0, tileData.Length * sizeof(float));

            var rowPitch = (uint) heightMap.Width * sizeof(float) * 4;

            return graphicsDevice.CreateStaticTexture2D(
                (uint) heightMap.Width,
                (uint) heightMap.Height,
                1u,
                new TextureMipMapData(
                    textureIDsByteArray,
                    rowPitch,
                    rowPitch * (uint) heightMap.Height,
                    (uint) heightMap.Width,
                    (uint) heightMap.Height),
                PixelFormat.R32_G32_B32_A32_UInt);
        }

        private static BlendData GetBlendData(
            MapFile mapFile,
            int x, int y,
            uint blendIndex,
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

        private static DeviceBuffer CreateCliffDetails(
            GraphicsDevice graphicsDevice,
            MapFile mapFile)
        {
            var cliffDetails = new TerrainShaderResources.CliffInfo[mapFile.BlendTileData.CliffTextureMappings.Length];

            const int cliffScalingFactor = 64;
            for (var i = 0; i < cliffDetails.Length; i++)
            {
                var cliffMapping = mapFile.BlendTileData.CliffTextureMappings[i];
                cliffDetails[i] = new TerrainShaderResources.CliffInfo
                {
                    BottomLeftUV = cliffMapping.BottomLeftCoords * cliffScalingFactor,
                    BottomRightUV = cliffMapping.BottomRightCoords * cliffScalingFactor,
                    TopLeftUV = cliffMapping.TopLeftCoords * cliffScalingFactor,
                    TopRightUV = cliffMapping.TopRightCoords * cliffScalingFactor
                };
            }

            return cliffDetails.Length > 0
                ? graphicsDevice.CreateStaticStructuredBuffer(cliffDetails)
                : null;
        }

        private void CreateTextures(
            ContentManager contentManager,
            BlendTileData blendTileData,
            out Texture textureArray,
            out TerrainShaderResources.TextureInfo[] textureDetails)
        {
            var graphicsDevice = contentManager.GraphicsDevice;

            var numTextures = (uint) blendTileData.Textures.Length;

            var textureInfo = new(uint size, FileSystemEntry entry)[numTextures];
            var largestTextureSize = uint.MinValue;

            textureDetails = new TerrainShaderResources.TextureInfo[numTextures];

            for (var i = 0; i < numTextures; i++)
            {
                var mapTexture = blendTileData.Textures[i];

                var terrainType = contentManager.IniDataContext.TerrainTextures.First(x => x.Name == mapTexture.Name);
                var texturePath = Path.Combine("Art", "Terrain", terrainType.Texture);
                var entry = contentManager.FileSystem.GetFile(texturePath);

                var size = (uint) TgaFile.GetSquareTextureSize(entry);

                textureInfo[i] = (size, entry);

                if (size > largestTextureSize)
                {
                    largestTextureSize = size;
                }

                textureDetails[i] = new TerrainShaderResources.TextureInfo
                {
                    TextureIndex = (uint) i,
                    CellSize = mapTexture.CellSize * 2
                };
            }

            textureArray = AddDisposable(graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    largestTextureSize,
                    largestTextureSize,
                    CalculateMipMapCount(largestTextureSize, largestTextureSize),
                    numTextures,
                    PixelFormat.R8_G8_B8_A8_UNorm,
                    TextureUsage.Sampled)));

            var commandList = graphicsDevice.ResourceFactory.CreateCommandList();
            commandList.Begin();

            var texturesToDispose = new List<Texture>();

            for (var i = 0u; i < numTextures; i++)
            {
                var tgaFile = TgaFile.FromFileSystemEntry(textureInfo[i].entry);
                var originalData = TgaFile.ConvertPixelsToRgba8(tgaFile, true);

                using (var tgaImage = Image.LoadPixelData<Rgba32>(
                    originalData,
                    tgaFile.Header.Width,
                    tgaFile.Header.Height))
                {
                    if (tgaFile.Header.Width != largestTextureSize)
                    {
                        tgaImage.Mutate(x => x
                            .Resize((int) largestTextureSize, (int) largestTextureSize, MapTextureResampler));
                    }

                    var imageSharpTexture = new ImageSharpTexture(tgaImage);

                    var sourceTexture = CreateTextureViaStaging(
                        imageSharpTexture,
                        graphicsDevice,
                        graphicsDevice.ResourceFactory);

                    texturesToDispose.Add(sourceTexture);

                    for (var mipLevel = 0u; mipLevel < imageSharpTexture.MipLevels; mipLevel++)
                    {
                        commandList.CopyTexture(
                            sourceTexture,
                            0, 0, 0,
                            mipLevel,
                            0,
                            textureArray,
                            0, 0, 0,
                            mipLevel,
                            i,
                            (uint) imageSharpTexture.Images[mipLevel].Width,
                            (uint) imageSharpTexture.Images[mipLevel].Height,
                            1,
                            1);
                    }
                }
            }

            commandList.End();

            graphicsDevice.SubmitCommands(commandList);

            foreach (var texture in texturesToDispose)
            {
                graphicsDevice.DisposeWhenIdle(texture);
            }

            graphicsDevice.DisposeWhenIdle(commandList);

            graphicsDevice.WaitForIdle();
        }

        private unsafe Texture CreateTextureViaStaging(ImageSharpTexture texture, GraphicsDevice gd, ResourceFactory factory)
        {
            var staging = factory.CreateTexture(
                TextureDescription.Texture2D(
                    texture.Width,
                    texture.Height,
                    texture.MipLevels,
                    1,
                    PixelFormat.R8_G8_B8_A8_UNorm,
                    TextureUsage.Staging));

            var cl = gd.ResourceFactory.CreateCommandList();
            cl.Begin();
            for (uint level = 0; level < texture.MipLevels; level++)
            {
                var image = texture.Images[level];
                fixed (void* pin = &MemoryMarshal.GetReference(image.GetPixelSpan()))
                {
                    var map = gd.Map(staging, MapMode.Write, level);
                    var rowWidth = (uint) (image.Width * 4);
                    if (rowWidth == map.RowPitch)
                    {
                        Unsafe.CopyBlock(map.Data.ToPointer(), pin, (uint) (image.Width * image.Height * 4));
                    }
                    else
                    {
                        for (uint y = 0; y < image.Height; y++)
                        {
                            var dstStart = (byte*) map.Data.ToPointer() + y * map.RowPitch;
                            var srcStart = (byte*) pin + y * rowWidth;
                            Unsafe.CopyBlock(dstStart, srcStart, rowWidth);
                        }
                    }
                    gd.Unmap(staging, level);
                }
            }
            cl.End();

            gd.SubmitCommands(cl);
            gd.DisposeWhenIdle(cl);

            return staging;
        }

        private static uint CalculateMipMapCount(uint width, uint height)
        {
            return 1u + (uint) Math.Floor(Math.Log(Math.Max(width, height), 2));
        }
    }
}
