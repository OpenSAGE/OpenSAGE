using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LLGfx;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Data.Map;
using OpenSage.Graphics.Effects;
using OpenSage.Terrain.Util;

namespace OpenSage.Terrain
{
    public sealed class Map : GraphicsObject
    {
        private readonly Terrain _terrain;

        private readonly Dictionary<TimeOfDay, Lights> _lightConfigurations;
        private Lights _lights;

        public Terrain Terrain => _terrain;

        private readonly MeshEffect _meshEffect;

        private readonly List<Thing> _things;
        private readonly List<Road> _roads;

        private TimeOfDay _currentTimeOfDay;
        public TimeOfDay CurrentTimeOfDay
        {
            get { return _currentTimeOfDay; }
            set
            {
                _currentTimeOfDay = value;
                _lights = _lightConfigurations[value];
            }
        }

        public Map(
            MapFile mapFile, 
            FileSystem fileSystem,
            GraphicsDevice graphicsDevice)
        {
            _lightConfigurations = new Dictionary<TimeOfDay, Lights>();
            foreach (var kvp in mapFile.GlobalLighting.LightingConfigurations)
            {
                _lightConfigurations[kvp.Key] = kvp.Value.ToLights();
            }

            CurrentTimeOfDay = mapFile.GlobalLighting.Time;

            var iniDataContext = new IniDataContext();
            iniDataContext.LoadIniFile(fileSystem.GetFile(@"Data\INI\Terrain.ini"));
            foreach (var iniFile in fileSystem.GetFiles(@"Data\INI\Object"))
            {
                iniDataContext.LoadIniFile(iniFile);
            }

            var contentManager = AddDisposable(new ContentManager(fileSystem, graphicsDevice));

            var uploadBatch = new ResourceUploadBatch(graphicsDevice);
            uploadBatch.Begin();

            _terrain = AddDisposable(new Terrain(
                mapFile, 
                graphicsDevice,
                fileSystem,
                iniDataContext,
                contentManager));

            uploadBatch.End();

            _meshEffect = AddDisposable(new MeshEffect(graphicsDevice));

            _things = new List<Thing>();
            _roads = new List<Road>();

            foreach (var mapObject in mapFile.ObjectsList.Objects)
            {
                uploadBatch.Begin();

                switch (mapObject.RoadType)
                {
                    case RoadType.None:
                        var objectDefinition = iniDataContext.Objects.FirstOrDefault(x => x.Name == mapObject.TypeName);
                        if (objectDefinition != null)
                        {
                            _things.Add(AddDisposable(new Thing(
                                mapObject,
                                _terrain.HeightMap,
                                objectDefinition,
                                fileSystem,
                                contentManager,
                                uploadBatch,
                                graphicsDevice)));
                        }
                        break;

                    default:
                        _roads.Add(AddDisposable(new Road()));
                        break;
                }

                uploadBatch.End();
            }
        }

        public void Draw(
            CommandEncoder commandEncoder,
            ref Matrix4x4 view,
            ref Matrix4x4 projection)
        {
            Terrain.Draw(
                commandEncoder,
                ref view,
                ref projection,
                ref _lights);

            _meshEffect.Begin(commandEncoder);

            _meshEffect.SetLights(ref _lights);

            foreach (var thing in _things)
            {
                thing.Draw(
                    commandEncoder,
                    _meshEffect,
                    ref view,
                    ref projection);
            }
        }
    }
}
