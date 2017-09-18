using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Data.Map;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Util;
using OpenSage.Terrain.Util;

namespace OpenSage.Terrain
{
    public sealed class Map : GraphicsObject
    {
        private readonly TerrainEffect _terrainEffect;

        private readonly Terrain _terrain;

        private readonly PipelineState _pipelineState;
        private readonly PipelineState _pipelineStateWireframe;

        private readonly DynamicBuffer _transformConstantBuffer;
        private TransformConstants _transformConstants;

        private readonly Dictionary<TimeOfDay, LightConfiguration> _lightConfigurations;

        private readonly DynamicBuffer _lightingConstantBuffer;
        private LightingConstants _lightingConstants;

        public Terrain Terrain => _terrain;

        private readonly ModelRenderer _modelRenderer;
        private readonly List<Thing> _things;
        private readonly List<Road> _roads;

        private TimeOfDay _currentTimeOfDay;
        public TimeOfDay CurrentTimeOfDay
        {
            get { return _currentTimeOfDay; }
            set
            {
                _currentTimeOfDay = value;

                var lightConfiguration = _lightConfigurations[value];
                _lightingConstants.Light0 = lightConfiguration.Light0;
                _lightingConstants.Light1 = lightConfiguration.Light1;
                _lightingConstants.Light2 = lightConfiguration.Light2;
            }
        }

        public bool RenderWireframeOverlay { get; set; }

        public Map(
            MapFile mapFile, 
            FileSystem fileSystem,
            GraphicsDevice graphicsDevice)
        {
            _terrainEffect = AddDisposable(new TerrainEffect(graphicsDevice, mapFile.BlendTileData.Textures.Length));

            _pipelineState = _terrainEffect.GetPipelineState(false);
            _pipelineStateWireframe = _terrainEffect.GetPipelineState(true);

            _transformConstantBuffer = AddDisposable(DynamicBuffer.Create<TransformConstants>(graphicsDevice));

            _lightingConstantBuffer = AddDisposable(DynamicBuffer.Create<LightingConstants>(graphicsDevice));

            _lightConfigurations = new Dictionary<TimeOfDay, LightConfiguration>();
            foreach (var kvp in mapFile.GlobalLighting.LightingConfigurations)
            {
                _lightConfigurations[kvp.Key] = ToLightConfiguration(kvp.Value);
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
                contentManager,
                _terrainEffect));

            uploadBatch.End();

            _modelRenderer = AddDisposable(new ModelRenderer(contentManager));

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

        private sealed class LightConfiguration
        {
            public Light Light0;
            public Light Light1;
            public Light Light2;
        }

        private static LightConfiguration ToLightConfiguration(GlobalLightingConfiguration mapLightingConfiguration)
        {
            return new LightConfiguration
            {
                Light0 = ToLight(mapLightingConfiguration.TerrainSun),
                Light1 = ToLight(mapLightingConfiguration.TerrainAccent1),
                Light2 = ToLight(mapLightingConfiguration.TerrainAccent2),
            };
        }

        private static Light ToLight(GlobalLight mapLight)
        {
            return new Light
            {
                Ambient = mapLight.Ambient.ToVector3(),
                Color = mapLight.Color.ToVector3(),
                Direction = Vector3.Normalize(new Vector3(
                    mapLight.EulerAngles.X,
                    mapLight.EulerAngles.Y,
                    mapLight.EulerAngles.Z))
            };
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TransformConstants
        {
            public Matrix4x4 WorldViewProjection;
            public Matrix4x4 World;
        }

        public void Draw(CommandEncoder commandEncoder,
            ref Vector3 cameraPosition,
            ref Matrix4x4 view,
            ref Matrix4x4 projection)
        {
            _terrainEffect.Apply(commandEncoder);

            Draw(commandEncoder, 
                _pipelineState, 
                ref _lightingConstants, 
                ref cameraPosition, 
                ref view, 
                ref projection);

            if (RenderWireframeOverlay)
            {
                var nullLightingConstants = new LightingConstants();
                Draw(
                    commandEncoder, 
                    _pipelineStateWireframe, 
                    ref nullLightingConstants, 
                    ref cameraPosition, 
                    ref view, 
                    ref projection);
            }
        }

        private void Draw(
            CommandEncoder commandEncoder,
            PipelineState pipelineState,
            ref LightingConstants lightingConstants,
            ref Vector3 cameraPosition,
            ref Matrix4x4 view,
            ref Matrix4x4 projection)
        {
            commandEncoder.SetPipelineState(pipelineState);

            _transformConstants.WorldViewProjection = view * projection;
            _transformConstants.World = Matrix4x4.Identity;
            _transformConstantBuffer.SetData(ref _transformConstants);
            commandEncoder.SetInlineConstantBuffer(0, _transformConstantBuffer);

            _lightingConstants.CameraPosition = cameraPosition;
            _lightingConstantBuffer.SetData(ref lightingConstants);
            commandEncoder.SetInlineConstantBuffer(1, _lightingConstantBuffer);

            _terrain.Draw(commandEncoder);

            _modelRenderer.PreDrawModels(commandEncoder, ref _lightingConstants);

            foreach (var thing in _things)
            {
                thing.Draw(
                    commandEncoder, 
                    ref cameraPosition, 
                    ref view, 
                    ref projection);
            }
        }
    }
}
