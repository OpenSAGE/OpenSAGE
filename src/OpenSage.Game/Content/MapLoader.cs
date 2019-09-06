using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenSage.Content.Util;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Data.Map;
using OpenSage.Graphics.Cameras;
using OpenSage.Logic.Object;
using OpenSage.Scripting;
using OpenSage.Settings;
using OpenSage.Terrain;
using Player = OpenSage.Logic.Player;
using Team = OpenSage.Logic.Team;

namespace OpenSage.Content
{
    internal sealed class MapLoader : ContentLoader<Scene3D>
    {
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

            var terrain = new Terrain.Terrain(mapFile, contentManager);

            var players = Player.FromMapData(mapFile.SidesList.Players, contentManager).ToArray();

            var teams = (mapFile.SidesList.Teams ?? mapFile.Teams.Items)
                .Select(team => Team.FromMapData(team, players))
                .ToArray();

            LoadObjects(
                contentManager,
                terrain.HeightMap,
                mapFile.ObjectsList.Objects,
                teams,
                out var waypoints,
                out var gameObjects,
                out var roads,
                out var bridges);

            var waterAreas = AddDisposable(new WaterAreaCollection(mapFile.PolygonTriggers, contentManager));

            var lighting = new WorldLighting(
                mapFile.GlobalLighting.LightingConfigurations.ToLightSettingsDictionary(),
                mapFile.GlobalLighting.Time);

            var waypointPaths = new WaypointPathCollection(waypoints, mapFile.WaypointsList.WaypointPaths);

            // TODO: Don't hardcode this.
            // Perhaps add one ScriptComponent for the neutral player, 
            // and one for the active player.
            var scriptList = mapFile.GetPlayerScriptsList().ScriptLists[0];
            var mapScripts = new MapScriptCollection(scriptList);

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
                waterAreas,
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

        private static GameObject CreateGameObject(MapObject mapObject, Team[] teams, ContentManager contentManager, GameObjectCollection parent)
        {
            var gameObject = contentManager.InstantiateObject(mapObject.TypeName, parent);

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

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
                                waypoints.Add(new Waypoint(mapObject));
                                break;

                            default:
                                position.Z += heightMap.GetHeight(position.X, position.Y);

                                var gameObject = CreateGameObject(mapObject, teams, contentManager, gameObjects);

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
                        if ((i + 1) >= mapObjects.Length || !mapObjects[i + 1].RoadType.HasFlag(RoadType.BridgeEnd))
                        {
                            logger.Warn($"Invalid bridge: {mapObject.ToString()}, skipping...");
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
                            out var bridge,
                            gameObjects))
                        {
                            bridgesList.Add(AddDisposable(bridge));
                        }

                        break;

                    case RoadType.Start:
                    case RoadType.End:
                        var roadEnd = mapObjects[++i];

                        // Some maps have roads with invalid start- or endpoints.
                        // We'll skip processing them altogether.
                        if (mapObject.TypeName == "" || roadEnd.TypeName == "")
                        {
                            logger.Warn($"Road {mapObject.ToString()} has invalid start- or endpoint, skipping...");
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
            var worldMatrix =
                Matrix4x4.CreateFromQuaternion(gameObject.Transform.Rotation)
                * Matrix4x4.CreateTranslation(gameObject.Transform.Translation);

            var landmarkBridgeTemplate = GetBridgeTemplate(contentManager, mapObject);

            var halfLength = gameObject.Definition.Geometry.MinorRadius;
            var halfWidth = gameObject.Definition.Geometry.MajorRadius;

            AddDisposable(new BridgeTowers(
                landmarkBridgeTemplate,
                contentManager,
                gameObjects,
                worldMatrix,
                -halfWidth,
                -halfLength,
                halfWidth,
                halfLength,
                gameObject.Transform.Rotation));
        }
    }
}
